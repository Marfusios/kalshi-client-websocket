using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kalshi.Client.Websocket.Enums;

namespace Kalshi.Client.Websocket.Responses.MarketData
{
    /// <summary>
    /// Ticker update response.
    /// </summary>
    public class TickerResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public TickerMessage Message { get; set; }
    }

    /// <summary>
    /// Ticker update payload.
    /// </summary>
    public class TickerMessage
    {
        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("market_id")]
        public string MarketId { get; set; }

        [JsonProperty("yes_bid")]
        public decimal? YesBid { get; set; }

        [JsonProperty("yes_bid_dollars")]
        public decimal? YesBidDollars { get; set; }

        [JsonProperty("yes_ask")]
        public decimal? YesAsk { get; set; }

        [JsonProperty("yes_ask_dollars")]
        public decimal? YesAskDollars { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("price_dollars")]
        public decimal? PriceDollars { get; set; }

        [JsonProperty("volume")]
        public decimal? Volume { get; set; }

        [JsonProperty("open_interest")]
        public decimal? OpenInterest { get; set; }

        [JsonProperty("dollar_volume")]
        public decimal? DollarVolume { get; set; }

        [JsonProperty("dollar_open_interest")]
        public decimal? DollarOpenInterest { get; set; }
    }

    /// <summary>
    /// Trade update response.
    /// </summary>
    public class TradeResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public TradeMessage Message { get; set; }
    }

    /// <summary>
    /// Trade update payload.
    /// </summary>
    public class TradeMessage
    {
        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("market_id")]
        public string MarketId { get; set; }

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

        [JsonProperty("taker_side")]
        public KalshiSide TakerSide { get; set; }

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }
    }

    /// <summary>
    /// Market lifecycle response.
    /// </summary>
    public class MarketLifecycleResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public MarketLifecycleMessage Message { get; set; }
    }

    /// <summary>
    /// Event lifecycle response.
    /// </summary>
    public class EventLifecycleResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public EventLifecycleMessage Message { get; set; }
    }

    /// <summary>
    /// Market lifecycle payload.
    /// </summary>
    public class MarketLifecycleMessage
    {
        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("event_ticker")]
        public string EventTicker { get; set; }

        [JsonProperty("event_type")]
        public KalshiLifecycleEvent EventType { get; set; }

        [JsonProperty("market_title")]
        public string MarketTitle { get; set; }

        [JsonProperty("yes_sub_title")]
        public string YesSubTitle { get; set; }

        [JsonProperty("no_sub_title")]
        public string NoSubTitle { get; set; }

        [JsonProperty("open_time")]
        public string OpenTime { get; set; }

        [JsonProperty("close_time")]
        public string CloseTime { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    /// <summary>
    /// Event lifecycle payload.
    /// </summary>
    public class EventLifecycleMessage
    {
        [JsonProperty("event_ticker")]
        public string EventTicker { get; set; }

        [JsonProperty("event_type")]
        public KalshiLifecycleEvent EventType { get; set; }

        [JsonProperty("event_title")]
        public string EventTitle { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
