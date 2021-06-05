using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using Models.WebSocket;
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
                    Serilog.Log.Error("ERROR: WebSocket message is not text");
                    return;
                }

                // Deserialise message.
                var request = Json.Deserialize<Request>(e.Data);
                if (request == null)
                {
                    Serilog.Log.Error("ERROR: Failed to parse message");
                    return;
                }

                // Handle message.
                var response = await this.HandleMessage(request);

                // Serialise and respond.
                string data = Json.Serialize(response);
                this.Send(data);
            }

            private async Task<Response> HandleMessage(Request request)
            {
                var response = new Response(request.Id, 200, "");
                try
                {
                    switch (request.Endpoint)
                    {
                        case Endpoints.InfoTotals:
                            AssertMethod(request.Method, HttpMethod.POST, Endpoints.InfoTotals);
                            response.Message = Json.Serialize(
                                await this._handler.GetInfoTotals(Deserialize<SearchRequest>(request.Message))
                            );
                            break;
                        case Endpoints.InfoTestResults:
                            AssertMethod(request.Method, HttpMethod.POST, Endpoints.InfoTestResults);
                            response.Message = Json.Serialize(
                                await this._handler.GetInfoTestResults(Deserialize<SearchRequest>(request.Message))
                            );
                            break;
                        case Endpoints.Status:
                            switch (request.Method)
                            {
                                case HttpMethod.GET:
                                    response.Message = Json.Serialize(
                                        await this._handler.GetStatus()
                                    );
                                    break;
                                case HttpMethod.POST:
                                    response.Message = Json.Serialize(
                                        await this._handler.EndTurn()
                                    );
                                    break;
                            }
                            break;
                        case Endpoints.Settings:
                            AssertMethod(request.Method, HttpMethod.GET, Endpoints.Settings);
                            response.Message = Json.Serialize(
                                await this._handler.GetSettings()
                            );
                            break;
                        case Endpoints.Actions:
                            AssertMethod(request.Method, HttpMethod.POST, Endpoints.Actions);
                            response.Message = Json.Serialize(
                                await this._handler.ApplyActions(Deserialize<List<WhoAction>>(request.Message))
                            );
                            break;
                        default:
                            throw new Exceptions.BadRequestException($"Endpoint ${request.Endpoint} is not recognised");
                    }
                }
                catch (Exceptions.BaseException ex)
                {
                    Serilog.Log.Error("Exception occured: {Message}", ex.Message);
                    response.Status = ex.Code;
                    response.Message = ex.Message;
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error("Unhandled exception occured: {Message}", ex.Message);
                    response.Status = 500;
                    response.Message = ex.Message;
                }

                return response;
            }

            private static T Deserialize<T>(string? data)
            {
                if (data == null)
                {
                    throw new Exceptions.BadRequestException($"Expected message, got null");
                }

                T? o = Json.Deserialize<T>(data);

                if (o == null)
                {
                    throw new Exceptions.BadRequestException($"Failed to deserialise ${data}");
                }

                return o;
            }

            private static void AssertMethod(HttpMethod method, HttpMethod target, string endpoint)
            {
                if (method != target)
                {
                    throw new Exceptions.BadRequestException($"{endpoint} only supports {target}");
                }
            }
        }
    }
}
