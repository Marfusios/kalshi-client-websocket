using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kalshi.Client.Websocket.Responses.Private
{
    /// <summary>
    /// RFQ created response.
    /// </summary>
    public class RfqCreatedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public JObject Message { get; set; }
    }

    /// <summary>
    /// RFQ deleted response.
    /// </summary>
    public class RfqDeletedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public JObject Message { get; set; }
    }

    /// <summary>
    /// Quote created response.
    /// </summary>
    public class QuoteCreatedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public JObject Message { get; set; }
    }

    /// <summary>
    /// Quote accepted response.
    /// </summary>
    public class QuoteAcceptedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public JObject Message { get; set; }
    }

    /// <summary>
    /// Quote executed response.
    /// </summary>
    public class QuoteExecutedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public JObject Message { get; set; }
    }
}
