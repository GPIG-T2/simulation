using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WHO.Interface
{
    public interface IClient
    {
        Task<Models.InfectionTotals> GetInfoTotals(Models.SearchRequest request);
        Task<Models.ActorSearchResult> GetInfoActors(Models.SearchRequest request);
        Task<Models.ActorSearchResult> GetInfoHomes(Models.SearchRequest request);
        Task<Models.Actor> GetInfoActor(List<int> ids);
        Task<Models.SimulationStatus> GetStatus();
        Task<Models.SimulationSettings> GetSettings();
        Task<List<Models.ActionResult>> ApplyActions(List<Models.WhoAction> actions);
    }
}
