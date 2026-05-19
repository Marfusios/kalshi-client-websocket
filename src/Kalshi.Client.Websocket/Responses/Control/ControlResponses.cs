using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kalshi.Client.Websocket.Enums;

namespace Kalshi.Client.Websocket.Responses.Control
{
    /// <summary>
    /// Response sent after a successful subscribe command.
    /// </summary>
    public class SubscribedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public SubscribedMessage Message { get; set; }
    }

    /// <summary>
    /// Subscribed response payload.
    /// </summary>
    public class SubscribedMessage
    {
        [JsonProperty("channel")]
        public KalshiChannel Channel { get; set; }

        [JsonProperty("sid")]
        public long Sid { get; set; }
    }

    /// <summary>
    /// Response sent after a successful unsubscribe command.
    /// </summary>
    public class UnsubscribedResponse : KalshiResponse
    {
    }

    /// <summary>
    /// Generic ok response.
    /// </summary>
    public class OkResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public JToken Message { get; set; }
    }

    /// <summary>
    /// Error response.
    /// </summary>
    public class ErrorResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public ErrorMessage Message { get; set; }
    }

    /// <summary>
    /// Error response payload.
    /// </summary>
    public class ErrorMessage
    {
        [JsonProperty("code")]
        public int? Code { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }
    }
}
