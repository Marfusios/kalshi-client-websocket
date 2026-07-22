using System;
using Kalshi.Client.Websocket.Client;
using Kalshi.Client.Websocket.Enums;
using Kalshi.Client.Websocket.Files;
using Kalshi.Client.Websocket.Responses.Control;
using Kalshi.Client.Websocket.Responses.MarketData;
using Kalshi.Client.Websocket.Responses.Private;
using Websocket.Client;
using Xunit;

namespace Kalshi.Client.Websocket.Tests.Responses
{
    public class MessageRoutingTests
    {
        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenOrderbookSnapshotReceived_PublishesOrderbookSnapshotStream()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            OrderbookSnapshotResponse received = null;

            client.Streams.OrderbookSnapshotStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage("{\"type\":\"orderbook_snapshot\",\"sid\":1,\"seq\":1,\"msg\":{\"market_ticker\":\"KXTEST-YES\",\"yes\":[[8,300]],\"no\":[[54,20]],\"yes_dollars\":[[\"0.0800\",\"300.00\"]],\"no_dollars\":[[\"0.5400\",\"20.00\"]]}}"));

            Assert.NotNull(received);
            Assert.Equal(KalshiMessageType.OrderbookSnapshot, received.Type);
            Assert.Equal(1, received.Sid);
            Assert.Equal("KXTEST-YES", received.Message.MarketTicker);
            Assert.Equal(8m, received.Message.Yes[0].Price);
            Assert.Equal(300m, received.Message.Yes[0].Quantity);
            Assert.Equal(0.0800m, received.Message.YesDollars[0].Price);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenFixedPointOrderbookSnapshotReceived_PublishesFixedPointLevels()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            OrderbookSnapshotResponse received = null;

            client.Streams.OrderbookSnapshotStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"orderbook_snapshot\",\"sid\":1,\"seq\":41," +
                "\"msg\":{\"market_ticker\":\"KXBTC15M-TEST\"," +
                "\"yes_dollars_fp\":[[\"0.431\",\"12.00\"]]," +
                "\"no_dollars_fp\":[[\"0.560\",\"8.00\"]]}}"));

            Assert.NotNull(received);
            Assert.Equal(0.431m, received.Message.EffectiveYesDollars[0].Price);
            Assert.Equal(12m, received.Message.EffectiveYesDollars[0].Quantity);
            Assert.Equal(0.560m, received.Message.EffectiveNoDollars[0].Price);
            Assert.Equal(8m, received.Message.EffectiveNoDollars[0].Quantity);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenFixedPointOrderbookDeltaReceived_PublishesDeltaAndTimestamps()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            OrderbookDeltaResponse received = null;

            client.Streams.OrderbookDeltaStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"orderbook_delta\",\"sid\":1,\"seq\":42," +
                "\"msg\":{\"market_ticker\":\"KXBTC15M-TEST\"," +
                "\"price_dollars\":\"0.431\",\"delta_fp\":\"-2.00\"," +
                "\"side\":\"yes\",\"ts\":\"2026-07-18T14:00:00Z\",\"ts_ms\":1784383200123}}"));

            Assert.NotNull(received);
            Assert.Equal(0.431m, received.Message.PriceDollars);
            Assert.Equal(-2m, received.Message.EffectiveDelta);
            Assert.Equal(1784383200L, received.Message.Timestamp);
            Assert.Equal(1784383200123L, received.Message.TimestampMilliseconds);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenOptionalTimestampMalformed_UsesTimestampMilliseconds()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            OrderbookDeltaResponse received = null;

            client.Streams.OrderbookDeltaStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"orderbook_delta\",\"sid\":1,\"seq\":43," +
                "\"msg\":{\"market_ticker\":\"KXBTC15M-TEST\"," +
                "\"price_dollars\":\"0.432\",\"delta_fp\":\"1.00\"," +
                "\"side\":\"yes\",\"ts\":\"invalid\",\"ts_ms\":1784383201123}}"));

            Assert.NotNull(received);
            Assert.Null(received.Message.Timestamp);
            Assert.Equal(1784383201123L, received.Message.TimestampMilliseconds);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenTickerReceived_PublishesTickerStream()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            TickerResponse received = null;

            client.Streams.TickerStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage("{\"type\":\"ticker\",\"sid\":2,\"seq\":1,\"msg\":{\"market_ticker\":\"KXTEST-YES\",\"yes_bid\":8,\"yes_ask\":10,\"price\":\"9\",\"volume\":\"1234\",\"open_interest\":456}}"));

            Assert.NotNull(received);
            Assert.Equal(KalshiMessageType.Ticker, received.Type);
            Assert.Equal(8m, received.Message.YesBid);
            Assert.Equal(10m, received.Message.YesAsk);
            Assert.Equal(1234m, received.Message.Volume);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenTradeReceived_PublishesTradeStream()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            TradeResponse received = null;

            client.Streams.TradeStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage("{\"type\":\"trade\",\"sid\":3,\"seq\":1,\"msg\":{\"market_ticker\":\"KXTEST-YES\",\"yes_price\":9,\"yes_price_dollars\":\"0.0900\",\"no_price\":91,\"count\":\"25\",\"taker_side\":\"yes\",\"ts\":1770000000000}}"));

            Assert.NotNull(received);
            Assert.Equal(9m, received.Message.YesPrice);
            Assert.Equal(0.0900m, received.Message.YesPriceDollars);
            Assert.Equal(25m, received.Message.Count);
            Assert.Equal(KalshiSide.Yes, received.Message.TakerSide);
            Assert.Equal(1770000000000L, received.Message.Timestamp);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenControlMessagesReceived_PublishesControlStreams()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            SubscribedResponse subscribed = null;
            ErrorResponse error = null;

            client.Streams.SubscribedStream.Subscribe(x => subscribed = x);
            client.Streams.ErrorStream.Subscribe(x => error = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage("{\"id\":1,\"type\":\"subscribed\",\"msg\":{\"channel\":\"ticker\",\"sid\":5}}"));
            communicator.StreamFakeMessage(ResponseMessage.TextMessage("{\"id\":2,\"type\":\"error\",\"msg\":{\"code\":6,\"msg\":\"bad command\",\"market_ticker\":\"KXTEST-YES\"}}"));

            Assert.NotNull(subscribed);
            Assert.Equal(KalshiChannel.Ticker, subscribed.Message.Channel);
            Assert.Equal(5, subscribed.Message.Sid);
            Assert.NotNull(error);
            Assert.Equal(6, error.Message.Code);
            Assert.Equal("bad command", error.Message.Message);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenPrivateMessagesReceived_PublishesPrivateStreams()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            FillResponse fill = null;
            UserOrderResponse userOrder = null;

            client.Streams.FillStream.Subscribe(x => fill = x);
            client.Streams.UserOrderStream.Subscribe(x => userOrder = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage("{\"type\":\"fill\",\"sid\":8,\"seq\":1,\"msg\":{\"trade_id\":\"trade-1\",\"order_id\":\"order-1\",\"market_ticker\":\"KXTEST-YES\",\"action\":\"buy\",\"side\":\"yes\",\"yes_price\":\"9\",\"count\":\"5\",\"is_taker\":true,\"ts\":1770000000000}}"));
            communicator.StreamFakeMessage(ResponseMessage.TextMessage("{\"type\":\"user_order\",\"sid\":9,\"seq\":1,\"msg\":{\"order_id\":\"order-1\",\"market_ticker\":\"KXTEST-YES\",\"action\":\"buy\",\"side\":\"yes\",\"status\":\"resting\",\"yes_price\":\"9\",\"count\":\"10\",\"remaining_count\":\"5\"}}"));

            Assert.NotNull(fill);
            Assert.Equal(KalshiAction.Buy, fill.Message.Action);
            Assert.Equal(KalshiSide.Yes, fill.Message.Side);
            Assert.Equal(5m, fill.Message.Count);
            Assert.True(fill.Message.IsTaker);
            Assert.NotNull(userOrder);
            Assert.Equal(KalshiOrderStatus.Resting, userOrder.Message.Status);
            Assert.Equal(5m, userOrder.Message.RemainingCount);
        }
    }
}
