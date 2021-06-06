using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Interface.Client
{
    public class Rest : IClient, IAsyncDisposable
    {
        private readonly HttpClient _client = new();

        public Rest(string uri)
        {
            this._client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            this._client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            this._client.BaseAddress = new Uri(uri);
        }

        public event Action? Closed;

        public Task<List<ActionResult>> ApplyActions(List<WhoAction> actions)
            => this.Post<List<WhoAction>, List<ActionResult>>(Endpoints.Actions, actions);

        public Task<SimulationStatus> EndTurn()
            => this.Post<SimulationStatusUpdate, SimulationStatus>(Endpoints.Status, new SimulationStatusUpdate());

        public Task<List<Actor>> GetInfoActor(List<int> ids)
            => this.Post<List<int>, List<Actor>>(Endpoints.InfoActors, ids);

        public Task<List<ActorSearchResult>> GetInfoActors(SearchRequest request)
            => this.Post<SearchRequest, List<ActorSearchResult>>(Endpoints.InfoActorData, request);

        public Task<List<ActorSearchResult>> GetInfoHomes(SearchRequest request)
            => this.Post<SearchRequest, List<ActorSearchResult>>(Endpoints.InfoHomes, request);

        public Task<List<TestResults>> GetInfoTestResults(SearchRequest request)
            => this.Post<SearchRequest, List<TestResults>>(Endpoints.InfoTestResults, request);

        public Task<List<InfectionTotals>> GetInfoTotals(SearchRequest request)
            => this.Post<SearchRequest, List<InfectionTotals>>(Endpoints.InfoTotals, request);

        public Task<SimulationSettings> GetSettings()
            => this.Get<SimulationSettings>(Endpoints.Settings);

        public Task<SimulationStatus> GetStatus()
            => this.Get<SimulationStatus>(Endpoints.Status);

        public ValueTask DisposeAsync()
        {
            this._client.Dispose();
            return new ValueTask();
        }

        private async Task<TResult> Get<TResult>(string path)
        {
            var raw = await this._client.GetStreamAsync(path);
            var result = await Json.DeserializeAsync<TResult>(raw);

            if (result == null)
            {
                throw new Exception($"Failed to deserialise result from /{path}");
            }

            return result;
        }

        private async Task<TResult> Post<TRequest, TResult>(string path, TRequest request)
        {
            var raw = await this._client.PostAsync(path, new StringContent(Json.Serialize(request), Encoding.UTF8, "application/json"));
            var result = await Json.DeserializeAsync<TResult>(await raw.Content.ReadAsStreamAsync());

            if (result == null)
            {
                throw new Exception($"Failed to deserialise result from /{path}");
            }

            return result;
        }
    }
}
