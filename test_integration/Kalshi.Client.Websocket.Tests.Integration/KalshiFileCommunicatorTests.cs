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
    }
}
