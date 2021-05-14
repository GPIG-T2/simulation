using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using Websocket.Client;

namespace WHO.Interface
{
    public class WebSocket : IClient, IAsyncDisposable
    {
        private readonly IWebsocketClient _client;

        public WebSocket(string uri)
        {
            _client = new WebsocketClient(new Uri(uri));
            _client.MessageReceived.Subscribe(msg => this.HandleMessage(msg));
            _client.Start();
        }

        public Task<Models.InfectionTotals> GetInfoTotals(Models.SearchRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Models.ActorSearchResult> GetInfoActors(Models.SearchRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Models.ActorSearchResult> GetInfoHomes(Models.SearchRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Models.Actor> GetInfoActor(List<int> ids)
        {
            throw new NotImplementedException();
        }

        public Task<Models.SimulationStatus> GetStatus()
        {
            throw new NotImplementedException();
        }

        public Task<Models.SimulationSettings> GetSettings()
        {
            throw new NotImplementedException();
        }

        public Task<List<Models.ActionResult>> ApplyActions(List<Models.WhoAction> actions)
        {
            throw new NotImplementedException();
        }

        private void HandleMessage(ResponseMessage msg)
        {
            if (msg.Text == null)
            {
                Log.Error("Received a message with no text");
                return;
            }
            Log.Debug($"Received Message: {msg.Text}");
            // TODO: handle message
        }

        public async ValueTask DisposeAsync()
        {
            await _client.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Client end");
            _client.Dispose();
        }
    }
}
