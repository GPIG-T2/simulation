using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

namespace Virus
{
    public class Program : Interface.IHandler, IDisposable
    {
        private readonly Interface.IServer _server = new Interface.WebSocket();
        private World _world;

        public Program()
        {
            // TODO: properly init world
        }

        public void Start()
        {
            this._server.Start(this);
        }

        public void Stop()
        {
            this._server.Stop();
        }

        public Task<List<ActionResult>> ApplyActions(List<WhoAction> actions)
        {
            throw new NotImplementedException();
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
    }
}
