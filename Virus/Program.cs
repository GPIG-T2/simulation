using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;
using Models.Parameters;

namespace Virus
{
    public class Program : Interface.IHandler, IDisposable
    {
        private readonly Interface.IServer _server = new Interface.WebSocket();
        private readonly World _world;
        private readonly Lock _lock = new();

        private readonly SemaphoreSlim _startWait = new(0);
        private bool _started = false;
        private readonly SemaphoreSlim _turnWait = new(0);
        private bool _running = true;

        private readonly SimulationSettings _settings;
        private readonly SimulationStatus _status = new(false, 0, 0);

        public Program(World world)
        {
            this._world = world;
            this._settings = new(
                new("day", 1),
                world.Nodes.Cast<LocationDefinition>().ToList(),
                new(new(World.LowLevelMask, World.HighLevelMask), new(), new(Node.BadTestEfficacy, Node.GoodTestEfficacy)),
                world.Edges.Cast<Models.Edge>().ToList()
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
            await this._startWait.WaitAsync();

            while (this._running)
            {
                {
                    using var _ = await this._lock.Aquire();

                    // Perform a tick.
                    this._world.Update();
                }

                // Wait until it is our turn again.
                this._status.IsWhoTurn = true;
                await this._turnWait.WaitAsync();
            }
        }

        public async Task<List<ActionResult>> ApplyActions(List<WhoAction> actions)
        {
            if (this._status.IsWhoTurn)
            {
                throw new Exceptions.TooEarlyException();
            }

            using var _ = await this._lock.Aquire();
            var results = new List<ActionResult>();

            foreach (var action in actions)
            {
                var result = new ActionResult(action.Id, 200, "");

                try
                {
                    switch (action.Mode)
                    {
                        case "create":
                            this.HandleAction(action, true);
                            break;
                        case "delete":
                            // TODO: pull action from storage
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
                    break;
                case Furlough.ActionName:
                    if (create) { this._world.FurloughScheme(action.Parameters); }
                    else { this._world.CancelFurloughScheme(action.Parameters); }
                    break;
                case InformationPressRelease.ActionName:
                    if (create) { this._world.InformationPressRelease(action.Parameters); }
                    break;
                case Loan.ActionName:
                    if (create) { this._world.TakeLoan(action.Parameters); }
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
    }
}
