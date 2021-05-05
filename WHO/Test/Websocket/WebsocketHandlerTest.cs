using Moq;
using System;
using Websocket.Client;
using WHO.Websocket;
using Xunit;

namespace WHO.Test.Websocket
{
    public class WebsocketHandlerTest
    {

        private const string testUri = "ws://127.0.0.1";

        private readonly Action<string> stubAction = (_) => { };

        [Fact]
        public void TestInit_CreateNew()
        {
            bool callbackCalled = false;
            var handler = new WebsocketHandler(testUri, _ => callbackCalled = true);
            var client = handler.client;
            Assert.Null(client);
            handler.Init();
            client = handler.client;
            Assert.NotNull(client);
            client.StreamFakeMessage(ResponseMessage.TextMessage(""));
            Assert.True(callbackCalled, "Callback was not registered to the client");
        }

        [Fact]
        public void TestInit_UsingOld()
        {
            bool callbackCalled = false;
            var client = new WebsocketClient(new Uri(testUri));
            var handler = new WebsocketHandler(client, _ => callbackCalled = true);
            Assert.NotNull(client);
            handler.Init();
            Assert.NotNull(client);
            client.StreamFakeMessage(ResponseMessage.TextMessage(""));
            Assert.True(callbackCalled, "Callback was not registered to the client");
        }

        [Fact]
        public void TestConnect_HappyPath()
        {
            var client = new Mock<IWebsocketClient>();
            client.Setup(c => c.MessageReceived.Subscribe(It.IsAny<IObserver<ResponseMessage>>())).Returns((IDisposable)null);
            var handler = new WebsocketHandler(client.Object, stubAction);
            handler.Init();
            handler.Start();
            client.Verify(c => c.Start(), Times.Once);
        }

        [Fact]
        public void TestConnect_NullClient()
        {
            var handler = new WebsocketHandler(testUri, stubAction);
            Assert.Throws<InvalidOperationException>(handler.Start);
        }

        [Fact]
        public void TestConnect_AlreadyRunning()
        {
            var client = new Mock<IWebsocketClient>();
            client.Setup(c => c.IsRunning).Returns(true);
            client.Setup(c => c.MessageReceived.Subscribe(It.IsAny<IObserver<ResponseMessage>>())).Returns((IDisposable)null);
            var handler = new WebsocketHandler(client.Object, stubAction);
            handler.Init();
            Assert.Throws<InvalidOperationException>(handler.Start);
        }

        [Fact]
        public void TestConnect_AlreadyStarted()
        {
            var client = new Mock<IWebsocketClient>();
            client.Setup(c => c.IsStarted).Returns(true);
            client.Setup(c => c.MessageReceived.Subscribe(It.IsAny<IObserver<ResponseMessage>>())).Returns((IDisposable)null);
            var handler = new WebsocketHandler(client.Object, stubAction);
            handler.Init();
            Assert.Throws<InvalidOperationException>(handler.Start);
        }

        [Fact]
        public void TestSendFunction_HappyPath()
        {
            var client = new Mock<IWebsocketClient>();
            client.Setup(c => c.Send(It.IsAny<string>())).Verifiable();
            client.Setup(c => c.MessageReceived.Subscribe(It.IsAny<IObserver<ResponseMessage>>())).Returns((IDisposable)null);
            var handler = new WebsocketHandler(client.Object, stubAction);
            handler.Init();
            client.Setup(c => c.IsRunning).Returns(true);
            handler.SendMessage("Message");
            client.Verify(c => c.Send(It.IsAny<string>()));
        }

        [Fact]
        public void TestSendFunction_NullClient()
        {
            var handler = new WebsocketHandler(testUri, stubAction);
            Assert.Throws<InvalidOperationException>(() => handler.SendMessage("Message"));
        }

        [Fact]
        public void TestSendFunction_NotRunning()
        {
            var client = new Mock<IWebsocketClient>();
            client.Setup(c => c.IsRunning).Returns(false);
            client.Setup(c => c.MessageReceived.Subscribe(It.IsAny<IObserver<ResponseMessage>>())).Returns((IDisposable)null);
            var handler = new WebsocketHandler(client.Object, stubAction);
            handler.Init();
            Assert.Throws<InvalidOperationException>(() => handler.SendMessage("Message"));
        }
    }
}
