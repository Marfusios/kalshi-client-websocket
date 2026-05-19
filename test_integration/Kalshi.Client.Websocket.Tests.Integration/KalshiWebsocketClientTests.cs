using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Kalshi.Client.Websocket.Authentication;
using Kalshi.Client.Websocket.Client;
using Kalshi.Client.Websocket.Requests;
using Kalshi.Client.Websocket.Responses.MarketData;
using Kalshi.Client.Websocket.Websockets;
using Xunit;

namespace Kalshi.Client.Websocket.Tests.Integration
{
    public class KalshiWebsocketClientTests
    {
        [Fact]
        [Trait("Cat", "BaseExtended")]
        public async Task Websocket_WhenCredentialsProvided_ReceivesPublicOrderbookSnapshot()
        {
            var keyId = Environment.GetEnvironmentVariable("KALSHI_API_KEY_ID");
            var privateKeyPath = Environment.GetEnvironmentVariable("KALSHI_PRIVATE_KEY_PATH");
            var marketTicker = Environment.GetEnvironmentVariable("KALSHI_MARKET_TICKER");
            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(privateKeyPath) || string.IsNullOrWhiteSpace(marketTicker))
            {
                return;
            }

            using var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(privateKeyPath));
            var auth = KalshiAuthentication.FromRsaPrivateKey(keyId, rsa);
            var url = string.Equals(Environment.GetEnvironmentVariable("KALSHI_USE_DEMO"), "true", StringComparison.OrdinalIgnoreCase)
                ? KalshiValues.DemoTradeWebsocketApiUrl
                : KalshiValues.TradeWebsocketApiUrl;
            using var communicator = new KalshiWebsocketCommunicator(url, auth);
            using var client = new KalshiWebsocketClient(communicator);
            OrderbookSnapshotResponse received = null;
            var receivedEvent = new ManualResetEvent(false);

            client.Streams.OrderbookSnapshotStream.Subscribe(snapshot =>
            {
                received = snapshot;
                receivedEvent.Set();
            });

            await communicator.Start();
            client.Send(SubscribeRequest.Orderbook(1, marketTicker, sendInitialSnapshot: true));

            receivedEvent.WaitOne(TimeSpan.FromSeconds(30));

            Assert.NotNull(received);
        }
    }
}
