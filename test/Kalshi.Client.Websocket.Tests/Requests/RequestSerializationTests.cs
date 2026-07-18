using Kalshi.Client.Websocket.Enums;
using Kalshi.Client.Websocket.Exceptions;
using Kalshi.Client.Websocket.Json;
using Kalshi.Client.Websocket.Requests;
using Xunit;

namespace Kalshi.Client.Websocket.Tests.Requests
{
    public class RequestSerializationTests
    {
        [Fact]
        [Trait("Cat", "Base")]
        public void SubscribeRequest_WhenTickerMarketsProvided_SerializesExpectedPayload()
        {
            var request = SubscribeRequest.Ticker(1, "KXTEST-YES", "KXTEST-NO");

            var json = KalshiJsonSerializer.Serialize(request);

            Assert.Equal("{\"id\":1,\"cmd\":\"subscribe\",\"params\":{\"channels\":[\"ticker\"],\"market_tickers\":[\"KXTEST-YES\",\"KXTEST-NO\"]}}", json);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void SubscribeRequest_WhenOrderbookSnapshotRequested_SerializesExpectedPayload()
        {
            var request = SubscribeRequest.Orderbook(
                2,
                "KXTEST-YES",
                sendInitialSnapshot: true,
                useYesPrice: true);

            var json = KalshiJsonSerializer.Serialize(request);

            Assert.Equal("{\"id\":2,\"cmd\":\"subscribe\",\"params\":{\"channels\":[\"orderbook_delta\"],\"market_ticker\":\"KXTEST-YES\",\"send_initial_snapshot\":true,\"use_yes_price\":true}}", json);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void UpdateSubscriptionRequest_WhenMarketsAdded_SerializesExpectedPayload()
        {
            var request = new UpdateSubscriptionRequest(3, 7, KalshiSubscriptionAction.AddMarkets, new[] { "KXTEST-YES" });

            var json = KalshiJsonSerializer.Serialize(request);

            Assert.Equal("{\"id\":3,\"cmd\":\"update_subscription\",\"params\":{\"sid\":7,\"action\":\"add_markets\",\"market_tickers\":[\"KXTEST-YES\"]}}", json);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void UpdateSubscriptionRequest_WhenSnapshotRequestedWithSidsArray_SerializesExpectedPayload()
        {
            var request = new UpdateSubscriptionRequest(3, new[] { 7L }, KalshiSubscriptionAction.GetSnapshot, new[] { "KXTEST-YES" });

            var json = KalshiJsonSerializer.Serialize(request);

            Assert.Equal("{\"id\":3,\"cmd\":\"update_subscription\",\"params\":{\"sids\":[7],\"action\":\"get_snapshot\",\"market_tickers\":[\"KXTEST-YES\"]}}", json);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void UpdateSubscriptionRequest_WhenMultipleSubscriptionIdsProvided_Throws()
        {
            Assert.Throws<KalshiBadInputException>(() =>
                new UpdateSubscriptionRequest(3, new[] { 7L, 8L }, KalshiSubscriptionAction.AddMarkets, new[] { "KXTEST-YES" }));
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void UnsubscribeRequest_WhenSubscriptionIdsProvided_SerializesExpectedPayload()
        {
            var request = new UnsubscribeRequest(4, new[] { 7L, 8L });

            var json = KalshiJsonSerializer.Serialize(request);

            Assert.Equal("{\"id\":4,\"cmd\":\"unsubscribe\",\"params\":{\"sids\":[7,8]}}", json);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void ListSubscriptionsRequest_WhenSerialized_SerializesExpectedPayload()
        {
            var request = new ListSubscriptionsRequest(5);

            var json = KalshiJsonSerializer.Serialize(request);

            Assert.Equal("{\"id\":5,\"cmd\":\"list_subscriptions\"}", json);
        }
    }
}
