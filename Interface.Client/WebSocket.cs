using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using Serilog;
using Websocket.Client;

namespace Interface.Client
{
    public class WebSocket : IClient, IAsyncDisposable
    {
        private readonly IWebsocketClient _client;
        private readonly Dictionary<int, TaskCompletionSource<Models.WebSocket.Response>> _messages
            = new();
        private readonly object _sync = new();

        private int _id = 0;

        public event Action? Closed;

        public WebSocket(string uri)
        {
            this._client = new WebsocketClient(new Uri(uri));
            this._client.MessageReceived.Subscribe(this.HandleMessage);
            this._client.DisconnectionHappened.Subscribe(this.Disconnected);
            this._client.Start();
        }

        public Task<List<InfectionTotals>> GetInfoTotals(SearchRequest request) =>
            this.SendRequest<SearchRequest, List<InfectionTotals>>(Endpoints.InfoTotals, Models.WebSocket.HttpMethod.POST, request);

        public Task<List<ActorSearchResult>> GetInfoActors(SearchRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<List<ActorSearchResult>> GetInfoHomes(SearchRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<List<Actor>> GetInfoActor(List<int> ids)
        {
            throw new NotImplementedException();
        }

        public Task<List<TestResults>> GetInfoTestResults(SearchRequest request) =>
            this.SendRequest<SearchRequest, List<TestResults>>(Endpoints.InfoTestResults, Models.WebSocket.HttpMethod.POST, request);

        public Task<SimulationStatus> GetStatus() =>
            this.SendRequest<SimulationStatus>(Endpoints.Status, Models.WebSocket.HttpMethod.GET);

        public Task<SimulationStatus> EndTurn() =>
            this.SendRequest<SimulationStatusUpdate, SimulationStatus>(
                Endpoints.Status, Models.WebSocket.HttpMethod.POST, new SimulationStatusUpdate());

        public Task<SimulationSettings> GetSettings() =>
            this.SendRequest<SimulationSettings>(Endpoints.Settings, Models.WebSocket.HttpMethod.GET);

        public Task<List<ActionResult>> ApplyActions(List<WhoAction> actions) =>
            this.SendRequest<List<WhoAction>, List<ActionResult>>(Endpoints.Actions, Models.WebSocket.HttpMethod.POST, actions);

        private Task<T> SendRequest<T>(string endpoint, Models.WebSocket.HttpMethod method) =>
            this.SendRequest<object, T>(endpoint, method, null);

        private async Task<TResponse> SendRequest<TMessage, TResponse>(
            string endpoint,
            Models.WebSocket.HttpMethod method,
            TMessage? message)
        {
            var source = new TaskCompletionSource<Models.WebSocket.Response>();
            int id;

            // the ID and the message dict are not thread-safe, so just lock around them.
            lock (this._sync)
            {
                id = this._id++;
                this._messages[id] = source;
            }

            var request = new Models.WebSocket.Request(id, endpoint, method);
            if (message != null)
            {
                request.Message = Json.Serialize(message);
            }

            string msg = Json.Serialize(request);
            this._client.Send(msg);
            Log.Debug("Sent {0} to {1}", msg, endpoint);

            var response = await source.Task;
            if (response.Status != 200)
            {
                throw new Exception($"Failed request with {response.Message}");
            }

            var data = Json.Deserialize<TResponse>(response.Message);
            if (data == null)
            {
                throw new Exception("Failed to deserialise response");
            }

            return data;
        }

        private void HandleMessage(ResponseMessage msg)
        {
            if (msg.Text == null)
            {
                Log.Error("Received a message with no text");
                return;
            }
            Log.Debug("Received Message: {0}", msg.Text);

            var response = Json.Deserialize<Models.WebSocket.Response>(msg.Text);
            if (response == null)
            {
                Log.Error("Failed to parse message {0}", msg.Text);
                return;
            }

            TaskCompletionSource<Models.WebSocket.Response> source;
            // the message dict is not thread-safe, so just lock around it.
            lock (this._sync)
            {
                if (!this._messages.ContainsKey(response.Id))
                {
                    Log.Error("Got message with unknown ID {0}", response.Id);
                    return;
                }

                source = this._messages[response.Id];
                this._messages.Remove(response.Id);
            }

            source.SetResult(response);
        }

        private void Disconnected(DisconnectionInfo info)
        {
            Log.Information("WebSocket closed: {Info}", info.CloseStatusDescription);
            this.Closed?.Invoke();
        }

        public async ValueTask DisposeAsync()
        {
            await this._client.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Client end");
            this._client.Dispose();
        }
    }
}
