using Serilog;
using System;
using System.Text;
using Websocket.Client;

namespace WHO.Websocket
{

    public class WebsocketHandler
    {

        // The uri of the client
        private readonly Uri uri;

        // Reference to the client, can be retrieved but not set publicly 
        public IWebsocketClient client { get; private set; }

        // How long to wait to try reconnecting if no messages have been received from the server
        private const int keepAliveIntervalInSeconds = 5;

        // The callback which is invoked when the client receives a message
        private readonly Action<string> onMessageCallback;

        /// <summary>
        /// Creates the handler with a specified uri and callback, the client will be created in the init method
        /// </summary>
        /// <param name="uri">The uri of the client</param>
        /// <param name="onMessageCallback">The callback which is invoked when a message is received, takes a string as a parameter and returns void</param>
        /// <example>For example:
        /// <code>var handler = new WebsocketHandler("ws://127.0.0.1", MyCallbackFunc)</code>
        /// </example>
        public WebsocketHandler(string uri, Action<string> onMessageCallback)
        {
            this.uri = new Uri(uri);
            this.onMessageCallback = onMessageCallback;
        }

        /// <summary>
        /// Creates the handler with an already instantiated client and with a specific callback
        /// </summary>
        /// <param name="client">The client object</param>
        /// <param name="onMessageCallback">The callback which is invoked when a message is received, takes a string as a parameter and returns void</param>
        public WebsocketHandler(IWebsocketClient client, Action<string> onMessageCallback)
        {
            uri = client.Url;
            this.client = client;
            this.onMessageCallback = onMessageCallback;
        }

        /// <summary>
        /// Creates the client if it hasn't already been created and subscribes to the message handler
        /// </summary>
        public void Init()
        {
            if (client == null)
            {
                client = new WebsocketClient(uri)
                {
                    MessageEncoding = Encoding.ASCII,
                    ReconnectTimeout = TimeSpan.FromSeconds(keepAliveIntervalInSeconds)
                };
            }
            client.MessageReceived.Subscribe(msg => HandleMessage(msg));
        }

        /// <summary>
        /// Starts the client if it has not already started
        /// </summary>
        /// <exception cref="InvalidOperationException">If the client is null</exception>
        /// <exception cref="InvalidOperationException">If the client has already started</exception>
        public void Start()
        {
            if (client == null)
                throw new InvalidOperationException("Cannot connect before the client is created");
            if (client.IsStarted || client.IsRunning)
                throw new InvalidOperationException("Cannot connect if the client has already started");
            client.Start();
        }

        /// <summary>
        /// Sends the message from the client
        /// </summary>
        /// <param name="msg">The message that will be sent</param>
        /// <exception cref="InvalidOperationException">If the client is null</exception>
        /// <exception cref="InvalidOperationException">If the client isn't running</exception>
        public void SendMessage(String msg)
        {
            if (client == null)
                throw new InvalidOperationException("Cannot send message when client is null");
            if (!client.IsRunning)
                throw new InvalidOperationException("Cannot send message if client isn't running");
            Log.Debug($"Sending Message: {msg}");
            client.Send(msg);
        }

        /// <summary>
        /// The callback when a message is received, checks the message has text and invokes the callback
        /// </summary>
        /// <param name="msg">The message received</param>
        private void HandleMessage(ResponseMessage msg)
        {
            if (msg.Text == null)
            {
                Log.Error("Received a message with no text");
                return;
            }
            Log.Debug($"Received Message: {msg.Text}");
            onMessageCallback.Invoke(msg.Text);
        }

    }

}