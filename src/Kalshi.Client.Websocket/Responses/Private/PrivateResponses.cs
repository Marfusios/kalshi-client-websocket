using System.Collections.Generic;
using System;
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

        [JsonProperty("count_fp")]
        public decimal? CountFp { get; set; }

        [JsonProperty("is_taker")]
        public bool? IsTaker { get; set; }

        [JsonProperty("post_position_fp")]
        public decimal? PostPositionFp { get; set; }

        [JsonProperty("purchased_side")]
        public KalshiSide PurchasedSide { get; set; }

        [JsonProperty("subaccount")]
        public int? Subaccount { get; set; }

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }

        [JsonProperty("ts_ms")]
        public long? TimestampMilliseconds { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class MarketPositionMessage
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("market_id")]
        public string MarketId { get; set; }

        [JsonProperty("position")]
        public decimal? Position { get; set; }

        [JsonProperty("position_fp")]
        public decimal? PositionFp { get; set; }

        [JsonProperty("position_cost_dollars")]
        public decimal? PositionCostDollars { get; set; }

        [JsonProperty("fees_paid")]
        public decimal? FeesPaid { get; set; }

        [JsonProperty("fees_paid_dollars")]
        public decimal? FeesPaidDollars { get; set; }

        [JsonProperty("realized_pnl")]
        public decimal? RealizedPnl { get; set; }

        [JsonProperty("realized_pnl_dollars")]
        public decimal? RealizedPnlDollars { get; set; }

        [JsonProperty("position_fee_cost_dollars")]
        public decimal? PositionFeeCostDollars { get; set; }

        [JsonProperty("volume_fp")]
        public decimal? VolumeFp { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class UserOrderMessage
    {
        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        [JsonProperty("market_id")]
        public string MarketId { get; set; }

        [JsonProperty("action")]
        public KalshiAction Action { get; set; }

        [JsonProperty("side")]
        public KalshiSide Side { get; set; }

        [JsonProperty("is_yes")]
        public bool? IsYes { get; set; }

        [JsonProperty("outcome_side")]
        public KalshiSide OutcomeSide { get; set; }

        [JsonProperty("book_side")]
        public KalshiBookSide BookSide { get; set; }

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

        [JsonProperty("fill_count_fp")]
        public decimal? FillCountFp { get; set; }

        [JsonProperty("remaining_count")]
        public decimal? RemainingCount { get; set; }

        [JsonProperty("remaining_count_fp")]
        public decimal? RemainingCountFp { get; set; }

        [JsonProperty("initial_count_fp")]
        public decimal? InitialCountFp { get; set; }

        [JsonProperty("taker_fill_cost_dollars")]
        public decimal? TakerFillCostDollars { get; set; }

        [JsonProperty("maker_fill_cost_dollars")]
        public decimal? MakerFillCostDollars { get; set; }

        [JsonProperty("taker_fees_dollars")]
        public decimal? TakerFeesDollars { get; set; }

        [JsonProperty("maker_fees_dollars")]
        public decimal? MakerFeesDollars { get; set; }

        [JsonProperty("client_order_id")]
        public string ClientOrderId { get; set; }

        [JsonProperty("created_time")]
        public DateTime? CreatedTime { get; set; }

        [JsonProperty("last_update_time")]
        public DateTime? LastUpdateTime { get; set; }

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }

        [JsonProperty("created_ts_ms")]
        public long? CreatedTimestampMilliseconds { get; set; }

        [JsonProperty("last_updated_ts_ms")]
        public long? LastUpdatedTimestampMilliseconds { get; set; }

        [JsonProperty("subaccount_number")]
        public int? SubaccountNumber { get; set; }

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
