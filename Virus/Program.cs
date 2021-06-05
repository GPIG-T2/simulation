using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;
using Models.Parameters;
using Serilog;

namespace Virus
{
    public class Program : Interface.IHandler, IDisposable
    {
        private const int _totalDays = 365;

        public static async Task<int> Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

                var data = LoadWorld(args);
                if (data == null)
                {
                    return 1;
                }

                var world = (World)data;
                Log.Information("Loaded world");

                using var program = new Program(world, data.Map);

                program.Start();
                await program.Loop();

                return 0;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static Serialization.WorldData? LoadWorld(string[] args)
        {
            string path;
            if (args.Length < 1)
            {
                Console.Write($"{Directory.GetCurrentDirectory()}> World file: ");
                string? result = Console.ReadLine();

                if (result == null)
                {
                    Log.Error("Failed to get response");
                    return null;
                }

                path = result;
            }
            else
            {
                path = args[0];
            }

            if (!File.Exists(path))
            {
                Log.Fatal($"File at '{path}' not found");
                return null;
            }

            string json = File.ReadAllText(path);
            var data = Json.Deserialize<Serialization.WorldData>(json);

            if (data == null)
            {
                Log.Fatal("Failed to load world");
                return null;
            }

            return data;
        }

        private readonly Interface.IServer _server = new Interface.WebSocket();
        private readonly World _world;
        private readonly Dictionary<int, WhoAction> _storedActions = new();
        private readonly Lock _lock = new();

        private readonly SemaphoreSlim _startWait = new(0);
        private bool _started = false;
        private readonly SemaphoreSlim _turnWait = new(0);

        private readonly SimulationSettings _settings;
        private readonly SimulationStatus _status = new(false, 0, 0);

        public Program(World world, SimulationSettings.SelectedMap map)
        {
            this._world = world;
            this._settings = new(
                new("day", 1),
                world.Nodes.Select(n => (LocationDefinition)n).ToList(),
                new(new(World.LowLevelMask, World.HighLevelMask), new(), new(Node.BadTestEfficacy, Node.GoodTestEfficacy)),
                world.Edges.Select(e => (Models.Edge)e).ToList(),
                map
            );
        }

        public void Start()
        {
            this._server.Start(this);
        }

        public void Stop()
        {
            this._server.Stop();
        }

        public async Task Loop()
        {
            List<List<InfectionTotals>> totals = new(_totalDays);
            totals.Add(this.Snapshot());

            Log.Information("Ready to start simulation");
            await this._startWait.WaitAsync();

            {
                using var _ = await this._lock.Aquire();

                // Infect a random person to start with.
                this._world.StartInfection();
            }

            while (this._world.Day < _totalDays)
            {
                {
                    using var _ = await this._lock.Aquire();

                    // Perform a tick.
                    Log.Information("Processing update...");
                    this._world.Update();

                    totals.Add(this.Snapshot());
                }

                // Wait until it is our turn again.
                Log.Information("Waiting for WHO...");
                this._status.IsWhoTurn = true;
                await this._turnWait.WaitAsync();
            }

            Directory.CreateDirectory("tmp");
            await File.WriteAllTextAsync("tmp/dump.json", Json.Serialize(totals));
        }

        public async Task<List<ActionResult>> ApplyActions(List<WhoAction> actions)
        {
            if (!this._status.IsWhoTurn)
            {
                throw new Exceptions.TooEarlyException();
            }

            using var _ = await this._lock.Aquire();
            var results = new List<ActionResult>(actions.Count);

            foreach (var action in actions)
            {
                var result = new ActionResult(action.Id, 200, "");

                try
                {
                    switch (action.Mode)
                    {
                        case "create":
                            if (this._storedActions.ContainsKey(action.Id))
                            {
                                throw new Exceptions.BadRequestException($"Action with ID {action.Id} already exists");
                            }

                            this.HandleAction(action, true);
                            this._storedActions[action.Id] = action;
                            break;
                        case "delete":
                            if (!this._storedActions.ContainsKey(action.Id))
                            {
                                throw new Exceptions.BadRequestException($"Action with ID {action.Id} does not exist");
                            }

                            var prev = this._storedActions[action.Id];
                            this.HandleAction(prev, false);
                            this._storedActions.Remove(action.Id);
                            break;
                        default:
                            throw new Exceptions.BadRequestException("Mode has to be either 'create' or 'delete'");
                    }
                }
                catch (Exceptions.BaseException ex)
                {
                    result.Code = ex.Code;
                    result.Message = ex.Message;
                }
                catch (Exception ex)
                {
                    result.Code = 500;
                    result.Message = ex.Message;
                }

                results.Add(result);
            }

            return results;
        }

        public async Task<SimulationStatus> EndTurn()
        {
            this._status.IsWhoTurn = false;
            var status = await this.GetStatus();

            this._turnWait.Release();

            return status;
        }

        public Task<List<Actor>> GetInfoActor(List<int> ids)
        {
            throw new NotImplementedException();
        }

        public Task<List<ActorSearchResult>> GetInfoActors(SearchRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<List<ActorSearchResult>> GetInfoHomes(SearchRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<List<TestResults>> GetInfoTestResults(SearchRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<List<InfectionTotals>> GetInfoTotals(SearchRequest request)
        {
            using var _ = await this._lock.Aquire();

            return request.Locations.Select(l => this._world.Nodes.Get(l).Totals).ToList();
        }

        public Task<SimulationSettings> GetSettings() => Task.FromResult(this._settings);

        public async Task<SimulationStatus> GetStatus()
        {
            using var _ = await this._lock.Aquire();

            if (!this._started)
            {
                this._startWait.Release();
                this._started = true;
            }

            this._status.Budget = (int)Math.Floor(this._world.Budget);

            return this._status;
        }

        public void Dispose() => this._server.Dispose();

        private void HandleAction(WhoAction action, bool create)
        {
            if (action.Parameters == null)
            {
                throw new NullReferenceException("Action parameters cannot be null");
            }

            switch (action.Action)
            {
                case TestAndIsolation.ActionName:
                    if (create) { this._world.TestAndIsolation(action.Parameters); }
                    else { this._world.CancelTestAndIsolate(action.Parameters); }
                    break;
                case StayAtHome.ActionName:
                    if (create) { this._world.StayAtHomeOrder(action.Parameters); }
                    else { this._world.CancelStayAtHomeOrder(action.Parameters); }
                    break;
                case CloseSchools.ActionName:
                    if (create) { this._world.CloseSchools(action.Parameters); }
                    else { this._world.CancelCloseSchools(action.Parameters); }
                    break;
                case CloseRecreationalLocations.ActionName:
                    if (create) { this._world.CloseRecreationalLocations(action.Parameters); }
                    else { this._world.CancelCloseRecreationalLocations(action.Parameters); }
                    break;
                case ShieldingProgram.ActionName:
                    if (create) { this._world.ShieldingProgram(action.Parameters); }
                    else { this._world.CancelShieldingPorgram(action.Parameters); }
                    break;
                case MovementRestrictions.ActionName:
                    if (create) { this._world.MovementRestrictions(action.Parameters); }
                    else { this._world.CancelMovementRestrictions(action.Parameters); }
                    break;
                case CloseBorders.ActionName:
                    if (create) { this._world.TestAndIsolation(action.Parameters); }
                    else { this._world.CancelTestAndIsolate(action.Parameters); }
                    break;
                case InvestInVaccine.ActionName:
                    if (create) { this._world.InvestInVaccine(action.Parameters); }
                    else { /* TODO: throw exception */ }
                    break;
                case Furlough.ActionName:
                    if (create) { this._world.FurloughScheme(action.Parameters); }
                    else { this._world.CancelFurloughScheme(action.Parameters); }
                    break;
                case InformationPressRelease.ActionName:
                    if (create) { this._world.InformationPressRelease(action.Parameters); }
                    else { /* TODO: throw exception */ }
                    break;
                case Loan.ActionName:
                    if (create) { this._world.TakeLoan(action.Parameters); }
                    else { /* TODO: throw exception */ }
                    break;
                case MaskMandate.ActionName:
                    if (create) { this._world.MaskMandate(action.Parameters); }
                    else { this._world.CancelMaskMandate(action.Parameters); }
                    break;
                case HealthDrive.ActionName:
                    if (create) { this._world.HealthDrive(action.Parameters); }
                    else { this._world.CancelHealthDrive(action.Parameters); }
                    break;
                case InvestInHealthServices.ActionName:
                    if (create) { this._world.InvestInHealthServices(action.Parameters); }
                    else { this._world.CancelInvestInHealthServices(action.Parameters); }
                    break;
                case SocialDistancingMandate.ActionName:
                    if (create) { this._world.SocialDistancing(action.Parameters); }
                    else { this._world.CancelSocialDistancing(action.Parameters); }
                    break;
                case Curfew.ActionName:
                    if (create) { this._world.Curfew(action.Parameters); }
                    else { this._world.CancelCurfew(action.Parameters); }
                    break;
            }
        }

        private List<InfectionTotals> Snapshot() => this._world.Nodes.Select(n => n.Totals.Clone()).ToList();
    }
}
