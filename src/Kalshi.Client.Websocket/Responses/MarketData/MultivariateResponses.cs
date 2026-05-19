using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kalshi.Client.Websocket.Responses.MarketData
{
    /// <summary>
    /// Multivariate market lifecycle response.
    /// </summary>
    public class MultivariateMarketLifecycleResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public JObject Message { get; set; }
    }

    /// <summary>
    /// Multivariate lookup response.
    /// </summary>
    public class MultivariateLookupResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public JObject Message { get; set; }
    }
}
