using GPIGCommon;
using Newtonsoft.Json;
using Serilog;
using WHO.Websocket;

namespace WHO.Main
{
    class HealthOrganisation
    {
        private readonly WebsocketHandler handler;

        public HealthOrganisation(string uri)
        {
            handler = new WebsocketHandler(uri, ReceiveMessage);
            handler.Init();
        }

        public void Start()
        {
            handler.Start();
        }

        private void ReceiveMessage(string msg)
        {
            Message message;
            // Try to deserialise the message into the Message type
            try
            {
                message = JsonConvert.DeserializeObject<Message>(msg);
            }
            catch (JsonReaderException)
            {
                Log.Error($"Failed to deserialise message: {msg}");
                return;
            }
            // Parse the body of the message into the type given in the message and pass to the appropriate function
            try
            {
                switch (message.path)
                {
                    case MessagePath.Action:
                        ActionResponse action = JsonConvert.DeserializeObject<ActionResponse>(message.body);
                        HandleActionResponse(action);
                        break;
                    case MessagePath.Info:
                        InfoResponse info = JsonConvert.DeserializeObject<InfoResponse>(message.body);
                        HandleInfoResponse(info);
                        break;
                    case MessagePath.Setting:
                        SettingResponse setting = JsonConvert.DeserializeObject<SettingResponse>(message.body);
                        HandleSettingResponse(setting);
                        break;
                    case MessagePath.Status:
                        StatusResponse status = JsonConvert.DeserializeObject<StatusResponse>(message.body);
                        HandleStatusResponse(status);
                        break;
                    default:
                        Log.Error($"Unhandled message type received: {message.path}");
                        break;
                }
            } catch (JsonReaderException)
            {
                Log.Error($"Failed to deserialise body: {message.body}");
                return;
            }
        }

        public void SendRequest(MessageRequest request)
        {
            string body = JsonConvert.SerializeObject(request);
            MessagePath path;
            switch (request)
            {
                case ActionRequest _:
                    path = MessagePath.Action;
                    break;
                case InfoRequest _:
                    path = MessagePath.Info;
                    break;
                case StatusRequest _:
                    path = MessagePath.Status;
                    break;
                case SettingRequest _:
                    path = MessagePath.Setting;
                    break;
                default:
                    Log.Error($"Unhandled request type: {request.GetType().Name}");
                    return;
            }
            Message message = new Message { body = body, path = path };
            string msg = JsonConvert.SerializeObject(message);
            handler.SendMessage(msg);
        }

        private void HandleInfoResponse(InfoResponse info)
        {
            // TODO: Implement this
        }

        private void HandleStatusResponse(StatusResponse status)
        {
            // TODO: Implement this
        }

        private void HandleSettingResponse(SettingResponse setting)
        {
            // TODO: Implement this
        }

        private void HandleActionResponse(ActionResponse action)
        {
            // TODO: Implement this
        }
    }
}
