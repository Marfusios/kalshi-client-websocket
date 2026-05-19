using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kalshi.Client.Websocket.Enums;

namespace Kalshi.Client.Websocket.Responses.Private
{
    /// <summary>
    /// Fill update response.
    /// </summary>
    public class FillResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public FillMessage Message { get; set; }
    }

    /// <summary>
    /// Market position response.
    /// </summary>
    public class MarketPositionResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public MarketPositionMessage Message { get; set; }
    }

    /// <summary>
    /// User order response.
    /// </summary>
    public class UserOrderResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public UserOrderMessage Message { get; set; }
    }

    /// <summary>
    /// Order group update response.
    /// </summary>
    public class OrderGroupUpdateResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public OrderGroupUpdateMessage Message { get; set; }
    }

    public class FillMessage
    {
        [JsonProperty("trade_id")]
        public string TradeId { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("market_id")]
        public string MarketId { get; set; }

        [JsonProperty("action")]
        public KalshiAction Action { get; set; }

        [JsonProperty("side")]
        public KalshiSide Side { get; set; }

        [JsonProperty("yes_price")]
        public decimal? YesPrice { get; set; }

        [JsonProperty("yes_price_dollars")]
        public decimal? YesPriceDollars { get; set; }

        [JsonProperty("no_price")]
        public decimal? NoPrice { get; set; }

        [JsonProperty("no_price_dollars")]
        public decimal? NoPriceDollars { get; set; }

        [JsonProperty("count")]
        public decimal Count { get; set; }

        [JsonProperty("is_taker")]
        public bool? IsTaker { get; set; }

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class MarketPositionMessage
    {
        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("market_id")]
        public string MarketId { get; set; }

        [JsonProperty("position")]
        public decimal? Position { get; set; }

        [JsonProperty("fees_paid")]
        public decimal? FeesPaid { get; set; }

        [JsonProperty("realized_pnl")]
        public decimal? RealizedPnl { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class UserOrderMessage
    {
        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("market_id")]
        public string MarketId { get; set; }

        [JsonProperty("action")]
        public KalshiAction Action { get; set; }

        [JsonProperty("side")]
        public KalshiSide Side { get; set; }

        [JsonProperty("status")]
        public KalshiOrderStatus Status { get; set; }

        [JsonProperty("yes_price")]
        public decimal? YesPrice { get; set; }

        [JsonProperty("yes_price_dollars")]
        public decimal? YesPriceDollars { get; set; }

        [JsonProperty("no_price")]
        public decimal? NoPrice { get; set; }

        [JsonProperty("no_price_dollars")]
        public decimal? NoPriceDollars { get; set; }

        [JsonProperty("count")]
        public decimal? Count { get; set; }

        [JsonProperty("remaining_count")]
        public decimal? RemainingCount { get; set; }

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class OrderGroupUpdateMessage
    {
        [JsonProperty("order_group_id")]
        public string OrderGroupId { get; set; }

        [JsonProperty("event_type")]
        public KalshiOrderGroupEvent EventType { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("market_id")]
        public string MarketId { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
