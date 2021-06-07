using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

namespace Virus.Interface
{
    public interface IHandler
    {
        event Action<Models.WebSocket.Response>? OnBroadcast;

        Task<List<InfectionTotals>> GetInfoTotals(SearchRequest request);
        Task<List<ActorSearchResult>> GetInfoActors(SearchRequest request);
        Task<List<ActorSearchResult>> GetInfoHomes(SearchRequest request);
        Task<List<Actor>> GetInfoActor(List<int> ids);
        Task<List<TestResults>> GetInfoTestResults(SearchRequest request);
        Task<SimulationStatus> GetStatus();
        Task<SimulationStatus> EndTurn();
        Task<SimulationSettings> GetSettings();
        Task<List<ActionResult>> ApplyActions(List<WhoAction> actions);
    }
}
