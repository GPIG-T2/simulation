using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface.Client;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Virus.Rest.Controllers
{
    [Route("")]
    public class IndexController : Controller
    {
        private readonly IClient _client;

        public IndexController(IClient client)
        {
            this._client = client;
        }

        [HttpGet("status")]
        public async Task<SimulationStatus> GetStatus() => await this._client.GetStatus();

        [HttpPost("status")]
        public async Task<SimulationStatus> PostStatus([FromBody] SimulationStatusUpdate statusUpdate) => await this._client.EndTurn();

        [HttpGet("settings")]
        public async Task<SimulationSettings> GetSettings() => await this._client.GetSettings();

        [HttpPost("actions")]
        public async Task<List<Models.ActionResult>> ApplyActions([FromBody] List<WhoAction> actions) => await this._client.ApplyActions(actions);
    }
}
