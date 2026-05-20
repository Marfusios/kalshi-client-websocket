using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kalshi.Client.Websocket.Client;
using Kalshi.Client.Websocket.Enums;
using Kalshi.Client.Websocket.Files;
using Kalshi.Client.Websocket.Responses.Control;
using Kalshi.Client.Websocket.Responses.MarketData;
using Kalshi.Client.Websocket.Responses.Private;
using Xunit;

namespace Kalshi.Client.Websocket.Tests.Integration
{
    public class KalshiFileCommunicatorTests
    {
        [Fact]
        [Trait("Cat", "Base")]
        public async Task Start_WhenRawFileProvided_ReplaysAllModeledStreams()
        {
            var fileName = Path.Combine(AppContext.BaseDirectory, "data", "kalshi_raw_sample.txt");
            using var communicator = new KalshiFileCommunicator
            {
                FileNames = new[] { fileName },
                Delimiter = ";;"
            };
            using var client = new KalshiWebsocketClient(communicator);
            var snapshots = new List<OrderbookSnapshotResponse>();
            var deltas = new List<OrderbookDeltaResponse>();
            var tickers = new List<TickerResponse>();
            var trades = new List<TradeResponse>();
            var fills = new List<FillResponse>();
            var positions = new List<MarketPositionResponse>();
            var userOrders = new List<UserOrderResponse>();
            var orderGroups = new List<OrderGroupUpdateResponse>();
            var rfqs = new List<RfqCreatedResponse>();
            var quotes = new List<QuoteCreatedResponse>();

            client.Streams.OrderbookSnapshotStream.Subscribe(snapshots.Add);
            client.Streams.OrderbookDeltaStream.Subscribe(deltas.Add);
            client.Streams.TickerStream.Subscribe(tickers.Add);
            client.Streams.TradeStream.Subscribe(trades.Add);
            client.Streams.FillStream.Subscribe(fills.Add);
            client.Streams.MarketPositionStream.Subscribe(positions.Add);
            client.Streams.UserOrderStream.Subscribe(userOrders.Add);
            client.Streams.OrderGroupUpdateStream.Subscribe(orderGroups.Add);
            client.Streams.RfqCreatedStream.Subscribe(rfqs.Add);
            client.Streams.QuoteCreatedStream.Subscribe(quotes.Add);

            await communicator.Start();

            Assert.Single(snapshots);
            Assert.Single(deltas);
            Assert.Single(tickers);
            Assert.Single(trades);
            Assert.Single(fills);
            Assert.Single(positions);
            Assert.Single(userOrders);
            Assert.Single(orderGroups);
            Assert.Single(rfqs);
            Assert.Single(quotes);
            Assert.Equal("KXTEST-YESNO", snapshots[0].Message.MarketTicker);
            Assert.Equal(0.0800m, snapshots[0].Message.YesDollars[0].Price);
            Assert.Equal(KalshiSide.Yes, deltas[0].Message.Side);
            Assert.Equal(KalshiAction.Buy, fills[0].Message.Action);
            Assert.Equal(KalshiOrderStatus.Resting, userOrders[0].Message.Status);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public async Task Start_WhenCollectedPublicReplayProvided_RoutesTypedPublicStreams()
        {
            var fileName = Path.Combine(AppContext.BaseDirectory, "data", "kalshi_public_replay.txt");
            using var communicator = new KalshiFileCommunicator
            {
                FileNames = new[] { fileName },
                Delimiter = ";;"
            };
            using var client = new KalshiWebsocketClient(communicator);
            var rawMessages = new List<string>();
            var subscribed = new List<SubscribedResponse>();
            var snapshots = new List<OrderbookSnapshotResponse>();
            var deltas = new List<OrderbookDeltaResponse>();
            var tickers = new List<TickerResponse>();
            var trades = new List<TradeResponse>();
            var lifecycles = new List<MarketLifecycleResponse>();
            var oks = new List<OkResponse>();

            client.Streams.RawMessageStream.Subscribe(rawMessages.Add);
            client.Streams.SubscribedStream.Subscribe(subscribed.Add);
            client.Streams.OrderbookSnapshotStream.Subscribe(snapshots.Add);
            client.Streams.OrderbookDeltaStream.Subscribe(deltas.Add);
            client.Streams.TickerStream.Subscribe(tickers.Add);
            client.Streams.TradeStream.Subscribe(trades.Add);
            client.Streams.MarketLifecycleStream.Subscribe(lifecycles.Add);
            client.Streams.OkStream.Subscribe(oks.Add);

            await communicator.Start();

            Assert.Equal(8, rawMessages.Count);
            Assert.Equal(2, subscribed.Count);
            Assert.Single(snapshots);
            Assert.Single(deltas);
            Assert.Single(tickers);
            Assert.Single(trades);
            Assert.Single(lifecycles);
            Assert.Single(oks);
            Assert.False(string.IsNullOrWhiteSpace(snapshots[0].Message.MarketTicker));
            Assert.True((snapshots[0].Message.YesDollars?.Length ?? 0) + (snapshots[0].Message.NoDollars?.Length ?? 0) > 0);
            Assert.True(
                tickers[0].Message.YesAsk.HasValue ||
                tickers[0].Message.YesBid.HasValue ||
                tickers[0].Message.Price.HasValue ||
                tickers[0].Message.YesAskDollars.HasValue ||
                tickers[0].Message.YesBidDollars.HasValue ||
                tickers[0].Message.PriceDollars.HasValue);
            Assert.Equal(KalshiLifecycleEvent.MetadataUpdated, lifecycles[0].Message.EventType);
        }

        [Fact]
        [Trait("Cat", "Base")]
        public async Task Start_WhenLiveReplayProvided_RoutesAndParsesCapturedStreams()
        {
            var fileName = Path.Combine(AppContext.BaseDirectory, "data", "kalshi_live_replay_20260520.txt");
            using var communicator = new KalshiFileCommunicator
            {
                FileNames = new[] { fileName },
                Delimiter = ";;"
            };
            using var client = new KalshiWebsocketClient(communicator);
            var rawMessages = new List<string>();
            var subscribed = new List<SubscribedResponse>();
            var errors = new List<ErrorResponse>();
            var oks = new List<OkResponse>();
            var snapshots = new List<OrderbookSnapshotResponse>();
            var tickers = new List<TickerResponse>();
            var trades = new List<TradeResponse>();
            var marketLifecycles = new List<MarketLifecycleResponse>();
            var eventLifecycles = new List<EventLifecycleResponse>();
            var multivariateLifecycles = new List<MultivariateMarketLifecycleResponse>();
            var rfqCreated = new List<RfqCreatedResponse>();
            var rfqDeleted = new List<RfqDeletedResponse>();
            var userOrders = new List<UserOrderResponse>();

            client.Streams.RawMessageStream.Subscribe(rawMessages.Add);
            client.Streams.SubscribedStream.Subscribe(subscribed.Add);
            client.Streams.ErrorStream.Subscribe(errors.Add);
            client.Streams.OkStream.Subscribe(oks.Add);
            client.Streams.OrderbookSnapshotStream.Subscribe(snapshots.Add);
            client.Streams.TickerStream.Subscribe(tickers.Add);
            client.Streams.TradeStream.Subscribe(trades.Add);
            client.Streams.MarketLifecycleStream.Subscribe(marketLifecycles.Add);
            client.Streams.EventLifecycleStream.Subscribe(eventLifecycles.Add);
            client.Streams.MultivariateMarketLifecycleStream.Subscribe(multivariateLifecycles.Add);
            client.Streams.RfqCreatedStream.Subscribe(rfqCreated.Add);
            client.Streams.RfqDeletedStream.Subscribe(rfqDeleted.Add);
            client.Streams.UserOrderStream.Subscribe(userOrders.Add);

            await communicator.Start();

            Assert.Equal(21, rawMessages.Count);
            Assert.Equal(10, subscribed.Count);
            Assert.Single(errors);
            Assert.Single(oks);
            Assert.Single(snapshots);
            Assert.Single(tickers);
            Assert.Single(trades);
            Assert.Single(marketLifecycles);
            Assert.Single(eventLifecycles);
            Assert.Single(multivariateLifecycles);
            Assert.Single(rfqCreated);
            Assert.Single(rfqDeleted);
            Assert.Single(userOrders);

            Assert.Equal(13, errors[0].Message.Code);
            Assert.Equal("KXMVESPORTSMULTIGAMEEXTENDED-S202686B833AA711-D1F4C9913CF", snapshots[0].Message.MarketTicker);
            Assert.Equal(0.0510m, tickers[0].Message.YesBidDollars);
            Assert.Equal(400.00m, tickers[0].Message.YesBidSizeFp);
            Assert.Equal(1779278084725L, tickers[0].Message.TimestampMilliseconds);
            Assert.Equal(5.10m, trades[0].Message.CountFp);
            Assert.Equal(KalshiSide.Yes, trades[0].Message.TakerOutcomeSide);
            Assert.Equal(KalshiBookSide.Bid, trades[0].Message.TakerBookSide);
            Assert.Equal(KalshiLifecycleEvent.Settled, marketLifecycles[0].Message.EventType);
            Assert.Equal("Combo", eventLifecycles[0].Message.Title);
            Assert.Equal("KXMVESPORTSMULTIGAMEEXTENDED", eventLifecycles[0].Message.SeriesTicker);
            Assert.Equal(KalshiLifecycleEvent.Created, multivariateLifecycles[0].Message.EventType);
            Assert.Equal("deci_cent", multivariateLifecycles[0].Message.PriceLevelStructure);
            Assert.Equal(10.0000m, rfqCreated[0].Message.TargetCostDollars);
            Assert.NotEmpty(rfqCreated[0].Message.MveSelectedLegs);
            Assert.Equal(KalshiSide.Yes, rfqCreated[0].Message.MveSelectedLegs[0].Side);
            Assert.Equal(10.0000m, rfqDeleted[0].Message.TargetCostDollars);
            Assert.Equal("00000000-0000-0000-0000-000000000002", userOrders[0].Message.UserId);
            Assert.Equal(KalshiBookSide.Bid, userOrders[0].Message.BookSide);
            Assert.Equal(10.00m, userOrders[0].Message.RemainingCountFp);
        }
    }
}
