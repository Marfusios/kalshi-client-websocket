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
                "\"side\":\"yes\",\"ts\":1784383200,\"ts_ms\":1784383200123}}"));

            Assert.NotNull(received);
            Assert.Equal(0.431m, received.Message.PriceDollars);
            Assert.Equal(-2m, received.Message.EffectiveDelta);
            Assert.Equal(1784383200L, received.Message.Timestamp);
            Assert.Equal(1784383200123L, received.Message.TimestampMilliseconds);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenOrderbookDeltaHasRfc3339Timestamp_ConvertsToUnixSeconds()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            OrderbookDeltaResponse received = null;

            client.Streams.OrderbookDeltaStream.Subscribe(x => received = x);

            // live production shape captured on 2026-09-11
            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"orderbook_delta\",\"sid\":1,\"seq\":3," +
                "\"msg\":{\"market_ticker\":\"KXBTC15M-26SEP110545-45\",\"market_id\":\"5b93128e-380d-4862-bf4e-6ee11d9d8a25\"," +
                "\"price_dollars\":\"0.3600\",\"delta_fp\":\"20.00\",\"side\":\"yes\"," +
                "\"ts\":\"2026-09-11T09:31:20.25424Z\",\"ts_ms\":1789119080254}}"));

            Assert.NotNull(received);
            Assert.Equal(0.36m, received.Message.PriceDollars);
            Assert.Equal(20m, received.Message.EffectiveDelta);
            Assert.Equal(KalshiSide.Yes, received.Message.Side);
            Assert.Equal(1789119080L, received.Message.Timestamp);
            Assert.Equal(1789119080254L, received.Message.TimestampMilliseconds);
            // Time keeps the microseconds of the RFC3339 value
            Assert.Equal(new DateTime(2026, 9, 11, 9, 31, 20, DateTimeKind.Utc).AddTicks(2542400), received.Message.Time);
            Assert.Null(received.Message.ClientOrderId);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenOrderbookDeltaHasUnixTimestamp_TimeFallsBackToMilliseconds()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            OrderbookDeltaResponse received = null;

            client.Streams.OrderbookDeltaStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"orderbook_delta\",\"sid\":1,\"seq\":3," +
                "\"msg\":{\"market_ticker\":\"KXBTC15M-TEST\",\"price_dollars\":\"0.3600\",\"delta_fp\":\"20.00\",\"side\":\"yes\"," +
                "\"ts\":1789119080,\"ts_ms\":1789119080254,\"client_order_id\":\"c-1\",\"subaccount\":2}}"));

            Assert.NotNull(received);
            Assert.Equal(1789119080L, received.Message.Timestamp);
            Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1789119080254).UtcDateTime, received.Message.Time);
            Assert.Equal("c-1", received.Message.ClientOrderId);
            Assert.Equal(2, received.Message.Subaccount);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenTradeReceived_ParsesBlockTradeFlagAndTime()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            TradeResponse received = null;

            client.Streams.TradeStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"trade\",\"sid\":2,\"seq\":1,\"msg\":{\"trade_id\":\"0722b0f4\",\"market_ticker\":\"KXBTC15M-26SEP110545-45\"," +
                "\"yes_price_dollars\":\"0.4500\",\"no_price_dollars\":\"0.5500\",\"count_fp\":\"12.50\",\"taker_side\":\"no\"," +
                "\"taker_outcome_side\":\"no\",\"taker_book_side\":\"ask\",\"is_block_trade\":false,\"ts\":1789119080,\"ts_ms\":1789119080777}}"));

            Assert.NotNull(received);
            Assert.False(received.Message.IsBlockTrade);
            Assert.Equal(12.5m, received.Message.CountFp);
            Assert.Equal(KalshiSide.No, received.Message.TakerSide);
            Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1789119080777).UtcDateTime, received.Message.Time);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenCfBenchmarksValueReceived_ParsesFrameAndAverages()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            CfBenchmarksValueResponse received = null;

            client.Streams.CfBenchmarksValueStream.Subscribe(x => received = x);

            // live production shape captured on 2026-09-11
            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"cfbenchmarks_value\",\"sid\":1,\"seq\":2,\"msg\":{\"index_id\":\"BRTI\",\"received_at\":1789123105077," +
                "\"data\":\"{\\\"type\\\":\\\"value\\\",\\\"time\\\":1789123105000,\\\"id\\\":\\\"BRTI\\\",\\\"value\\\":\\\"77002.30\\\"}\"," +
                "\"avg_60s_data\":{\"value\":\"77002.30000000\",\"window_size\":0,\"window_start_ts_ms\":1789123045000,\"window_end_ts_exclusive\":1789123105000}," +
                "\"last_60s_windowed_average_15min\":{\"value\":\"76990.12\",\"window_size\":60,\"window_start_ts_ms\":1789122840000,\"window_end_ts_exclusive\":1789122900000}}}"));

            Assert.NotNull(received);
            Assert.Equal("BRTI", received.Message.IndexId);
            Assert.Equal(1789123105077L, received.Message.ReceivedAt);
            Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1789123105077).UtcDateTime, received.Message.ReceivedTime);
            Assert.Equal(77002.30m, received.Message.Average60s.Value);
            Assert.Equal(0, received.Message.Average60s.WindowSize);
            Assert.Equal(76990.12m, received.Message.Average60s15Min.Value);
            Assert.Equal(60, received.Message.Average60s15Min.WindowSize);

            var frame = received.Message.ParseFrame();
            Assert.Equal("BRTI", frame.Id);
            Assert.Equal(77002.30m, frame.Value);
            Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1789123105000).UtcDateTime, frame.SourceTime);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenFiveHzValueReceived_PublishesParsedValueAndSourceTime()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            CfBenchmarksValueResponse received = null;

            client.Streams.CfBenchmarksValue5HzStream.Subscribe(x => received = x);

            // live production shape captured on 2026-09-11
            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"cfbenchmarks_value_5hz\",\"sid\":1,\"seq\":2,\"msg\":{\"index_id\":\"BRTI\",\"value_usd\":\"76989.76000000\"," +
                "\"source_ts_ms\":1789127948800,\"received_at\":1789127948832," +
                "\"data\":\"{\\\"type\\\":\\\"value\\\",\\\"time\\\":1789127948800,\\\"id\\\":\\\"BRTI\\\",\\\"value\\\":\\\"76989.76\\\"}\"}}"));

            Assert.NotNull(received);
            Assert.Equal("BRTI", received.Message.IndexId);
            Assert.Equal(76989.76m, received.Message.ValueUsd);
            Assert.Equal(76989.76m, received.Message.EffectiveValue);
            Assert.Equal(1789127948800L, received.Message.SourceTimestampMilliseconds);
            Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1789127948800).UtcDateTime, received.Message.SourceTime);
            Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1789127948832).UtcDateTime, received.Message.ReceivedTime);
            Assert.Null(received.Message.Average60s);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenIndexListReceived_PublishesIndexIds()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            CfBenchmarksIndexListResponse received = null;

            client.Streams.CfBenchmarksIndexListStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"cfbenchmarks_value_5hz_indexlist\",\"id\":9,\"sid\":1,\"msg\":{\"index_ids\":[\"BRTI\",\"ETHUSD_RTI\"]}}"));

            Assert.NotNull(received);
            Assert.Equal(new[] { "BRTI", "ETHUSD_RTI" }, received.Message.IndexIds);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenMarketSettled_ParsesResultAndStrike()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            MarketLifecycleResponse received = null;

            client.Streams.MarketLifecycleStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"market_lifecycle_v2\",\"sid\":4,\"seq\":9,\"msg\":{\"market_ticker\":\"KXBTC15M-26SEP110545-45\"," +
                "\"event_type\":\"settled\",\"result\":\"yes\",\"settlement_value\":\"77401.12\",\"determination_ts\":1789119905," +
                "\"settled_ts\":1789119960,\"strike_type\":\"greater_or_equal\",\"floor_strike\":77336.05,\"open_ts\":1789119000,\"close_ts\":1789119900}}"));

            Assert.NotNull(received);
            Assert.Equal(KalshiLifecycleEvent.Settled, received.Message.EventType);
            Assert.Equal("yes", received.Message.Result);
            Assert.Equal("77401.12", received.Message.SettlementValue);
            Assert.Equal(77336.05m, received.Message.FloorStrike);
            Assert.Equal("greater_or_equal", received.Message.StrikeType);
            Assert.Equal(1789119905L, received.Message.DeterminationTimestamp);
            Assert.Equal(1789119960L, received.Message.SettledTimestamp);
            Assert.Equal(1789119900L, received.Message.CloseTimestamp);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void HandleMessage_WhenTickerHasIsoTime_ParsesTimeAndTimestamps()
        {
            using var communicator = new KalshiFileCommunicator();
            using var client = new KalshiWebsocketClient(communicator);
            TickerResponse received = null;

            client.Streams.TickerStream.Subscribe(x => received = x);

            communicator.StreamFakeMessage(ResponseMessage.TextMessage(
                "{\"type\":\"ticker\",\"sid\":3,\"msg\":{\"market_id\":\"8e4da2ec-8660-4066-956a-0437a34786ab\"," +
                "\"market_ticker\":\"KXETH15M-26SEP110545-45\",\"price_dollars\":\"0.5100\",\"yes_bid_dollars\":\"0.5000\"," +
                "\"yes_ask_dollars\":\"0.5100\",\"volume_fp\":\"2783.20\",\"open_interest_fp\":\"2249.46\"," +
                "\"dollar_volume\":1391,\"dollar_open_interest\":1124,\"yes_bid_size_fp\":\"310.93\",\"yes_ask_size_fp\":\"1.20\"," +
                "\"last_trade_size_fp\":\"10.57\",\"ts\":1789119080,\"ts_ms\":1789119080710,\"time\":\"2026-09-11T09:31:20.710855Z\"}}"));

            Assert.NotNull(received);
            Assert.Equal(0.51m, received.Message.PriceDollars);
            Assert.Equal(1789119080L, received.Message.Timestamp);
            Assert.Equal(1789119080710L, received.Message.TimestampMilliseconds);
            Assert.Equal(new DateTime(2026, 9, 11, 9, 31, 20, 710, DateTimeKind.Utc).AddTicks(8550), received.Message.Time);
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
