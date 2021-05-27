using System;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Virus.Interface
{
    public class WebSocket : IServer
    {
        private readonly WebSocketServer _server = new();
        private bool _init = false;

        public void Start(IHandler handler)
        {
            if (!this._init)
            {
                this._server.AddWebSocketService<InterfaceBehaviour>("/", b => { b.Init(handler); });
                this._init = true;
            }

            this._server.Start();
        }

        public void Stop()
        {
            this._server.Stop(CloseStatusCode.Normal, "Server shutdown");
        }

        public void Dispose()
        {
            if (this._server.IsListening)
            {
                this.Stop();
            }
        }

        private class InterfaceBehaviour : WebSocketBehavior
        {
#pragma warning disable CS8618 // We know these fields are not-null here
            private IHandler _handler;
#pragma warning restore CS8618

            public void Init(IHandler handler)
            {
                this._handler = handler;
            }

            protected override async void OnMessage(MessageEventArgs e)
            {
                // Check invariants.
                if (!e.IsText)
                {
                    Console.Error.WriteLine("ERROR: WebSocket message is not text");
                    return;
                }

                // Deserialise message.
                var request = JsonSerializer.Deserialize<Models.WebSocket.Request>(e.Data);
                if (request == null)
                {
                    Console.Error.WriteLine("ERROR: Failed to parse message");
                    return;
                }

                // Handle message.
                var response = await this.HandleMessage(request);

                // Serialise and respond.
                string data = JsonSerializer.Serialize(response);
                this.Send(data);
            }

            private Task<Models.WebSocket.Response> HandleMessage(Models.WebSocket.Request request)
            {
                throw new NotImplementedException();
            }
        }
    }
}
