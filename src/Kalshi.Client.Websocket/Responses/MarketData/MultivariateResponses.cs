using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kalshi.Client.Websocket.Enums;

namespace Kalshi.Client.Websocket.Responses.MarketData
{
    /// <summary>
    /// Multivariate market lifecycle response.
    /// </summary>
    public class MultivariateMarketLifecycleResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public MultivariateMarketLifecycleMessage Message { get; set; }
    }

    /// <summary>
    /// Multivariate lookup response.
    /// </summary>
    public class MultivariateLookupResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public JObject Message { get; set; }
    }

    /// <summary>
    /// Multivariate market lifecycle payload.
    /// </summary>
    public class MultivariateMarketLifecycleMessage
    {
        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("event_type")]
        public KalshiLifecycleEvent EventType { get; set; }

        [JsonProperty("open_ts")]
        public long? OpenTimestamp { get; set; }

        [JsonProperty("close_ts")]
        public long? CloseTimestamp { get; set; }

        [JsonProperty("price_level_structure")]
        public string PriceLevelStructure { get; set; }

        [JsonProperty("additional_metadata")]
        public JObject AdditionalMetadata { get; set; }
    }
}
