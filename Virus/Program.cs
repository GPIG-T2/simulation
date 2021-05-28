using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using Models.Parameters;

namespace Virus
{
    public class Program : Interface.IHandler, IDisposable
    {
        private readonly Interface.IServer _server = new Interface.WebSocket();
        private World _world;

        public Program(World world)
        {
            this._world = world;
        }

        public void Start()
        {
            this._server.Start(this);
        }

        public void Stop()
        {
            this._server.Stop();
        }

        public async Task<List<ActionResult>> ApplyActions(List<WhoAction> actions)
        {
            var results = new List<ActionResult>();

            foreach (var action in actions)
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

                        break;
                }
            }

            return results;
        }

        public Task<SimulationStatus> EndTurn()
        {
            throw new NotImplementedException();
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

        public Task<List<InfectionTotals>> GetInfoTotals(SearchRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<SimulationSettings> GetSettings()
        {
            throw new NotImplementedException();
        }

        public Task<SimulationStatus> GetStatus()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this._server.Dispose();
        }

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
    }
}
