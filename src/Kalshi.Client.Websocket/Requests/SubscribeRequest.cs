using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Kalshi.Client.Websocket.Enums;
using Kalshi.Client.Websocket.Validations;

namespace Kalshi.Client.Websocket.Requests
{
    /// <summary>
    /// Subscribe to one or more Kalshi websocket channels.
    /// </summary>
    public class SubscribeRequest
    {
        /// <summary>
        /// Create subscription request.
        /// </summary>
        public SubscribeRequest(
            long id,
            IEnumerable<KalshiChannel> channels,
            string? marketTicker = null,
            IEnumerable<string>? marketTickers = null,
            string? marketId = null,
            IEnumerable<string>? marketIds = null,
            bool? sendInitialSnapshot = null,
            int? shardFactor = null,
            int? shardKey = null,
            bool? useYesPrice = null)
        {
            KalshiValidations.ValidateInput(channels, nameof(channels));
            var channelArray = channels.Where(x => x != KalshiChannel.Unknown).ToArray();
            if (channelArray.Length == 0)
            {
                throw new Exceptions.KalshiBadInputException("At least one channel is required");
            }

            Id = id;
            Params = new SubscribeParams(
                channelArray,
                marketTicker,
                marketTickers,
                marketId,
                marketIds,
                sendInitialSnapshot,
                shardFactor,
                shardKey,
                useYesPrice);
        }

        /// <summary>
        /// Create an orderbook subscription for a single market.
        /// </summary>
        public static SubscribeRequest Orderbook(
            long id,
            string marketTicker,
            bool? sendInitialSnapshot = null,
            bool? useYesPrice = null)
        {
            return new SubscribeRequest(
                id,
                new[] { KalshiChannel.OrderbookDelta },
                marketTicker: marketTicker,
                sendInitialSnapshot: sendInitialSnapshot,
                useYesPrice: useYesPrice);
        }

        /// <summary>
        /// Create ticker subscription.
        /// </summary>
        public static SubscribeRequest Ticker(long id, params string[] marketTickers)
        {
            return new SubscribeRequest(id, new[] { KalshiChannel.Ticker }, marketTickers: marketTickers);
        }

        /// <summary>
        /// Create trade subscription.
        /// </summary>
        public static SubscribeRequest Trades(long id, params string[] marketTickers)
        {
            return new SubscribeRequest(id, new[] { KalshiChannel.Trade }, marketTickers: marketTickers);
        }

        /// <summary>
        /// Client request identifier.
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; }

        /// <summary>
        /// Command name.
        /// </summary>
        [JsonProperty("cmd")]
        public KalshiCommand Command { get; } = KalshiCommand.Subscribe;

        /// <summary>
        /// Subscription parameters.
        /// </summary>
        [JsonProperty("params")]
        public SubscribeParams Params { get; }
    }

    /// <summary>
    /// Subscribe command parameters.
    /// </summary>
    public class SubscribeParams
    {
        internal SubscribeParams(
            IEnumerable<KalshiChannel> channels,
            string? marketTicker,
            IEnumerable<string>? marketTickers,
            string? marketId,
            IEnumerable<string>? marketIds,
            bool? sendInitialSnapshot,
            int? shardFactor,
            int? shardKey,
            bool? useYesPrice)
        {
            Channels = channels.ToArray();
            MarketTicker = string.IsNullOrWhiteSpace(marketTicker) ? null : marketTicker;
            MarketTickers = marketTickers == null ? null : KalshiValidations.ValidateArray(marketTickers, nameof(marketTickers));
            MarketId = string.IsNullOrWhiteSpace(marketId) ? null : marketId;
            MarketIds = marketIds == null ? null : KalshiValidations.ValidateArray(marketIds, nameof(marketIds));
            SendInitialSnapshot = sendInitialSnapshot;
            ShardFactor = shardFactor;
            ShardKey = shardKey;
            UseYesPrice = useYesPrice;
        }

        [JsonProperty("channels")]
        public KalshiChannel[] Channels { get; }

        [JsonProperty("market_ticker", NullValueHandling = NullValueHandling.Ignore)]
        public string? MarketTicker { get; }

        [JsonProperty("market_tickers", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? MarketTickers { get; }

        [JsonProperty("market_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? MarketId { get; }

        [JsonProperty("market_ids", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? MarketIds { get; }

        [JsonProperty("send_initial_snapshot", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SendInitialSnapshot { get; }

        [JsonProperty("shard_factor", NullValueHandling = NullValueHandling.Ignore)]
        public int? ShardFactor { get; }

        [JsonProperty("shard_key", NullValueHandling = NullValueHandling.Ignore)]
        public int? ShardKey { get; }

        [JsonProperty("use_yes_price", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseYesPrice { get; }
    }
}
