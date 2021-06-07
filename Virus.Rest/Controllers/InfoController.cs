using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface.Client;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Virus.Rest.Controllers
{
    [Route("info")]
    public class InfoController : Controller
    {
        private readonly IClient _client;

        public InfoController(IClient client)
        {
            this._client = client;
        }

        [HttpPost("totals")]
        public async Task<List<InfectionTotals>> Totals([FromBody] SearchRequest request) => await this._client.GetInfoTotals(request);

        [HttpPost("actors")]
        public async Task<List<ActorSearchResult>> Actors([FromBody] SearchRequest request) => await this._client.GetInfoActors(request);

        [HttpPost("homes")]
        public async Task<List<ActorSearchResult>> Homes([FromBody] SearchRequest request) => await this._client.GetInfoHomes(request);

        [HttpPost("actor-data")]
        public async Task<List<Actor>> ActorData([FromBody] List<int> ids) => await this._client.GetInfoActor(ids);

        [HttpPost("test-results")]
        public async Task<List<TestResults>> TestResults([FromBody] SearchRequest request) => await this._client.GetInfoTestResults(request);
    }
}
