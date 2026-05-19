using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kalshi.Client.Websocket.Enums;

namespace Kalshi.Client.Websocket.Responses
{
    /// <summary>
    /// Base class for Kalshi websocket messages.
    /// </summary>
    public abstract class KalshiResponse
    {
        /// <summary>
        /// Message type.
        /// </summary>
        [JsonProperty("type")]
        public KalshiMessageType Type { get; set; }

        /// <summary>
        /// Subscription identifier, when present.
        /// </summary>
        [JsonProperty("sid")]
        public long? Sid { get; set; }

        /// <summary>
        /// Sequence number for subscribed stream messages.
        /// </summary>
        [JsonProperty("seq")]
        public long? Seq { get; set; }

        /// <summary>
        /// Client request identifier echoed by control responses.
        /// </summary>
        [JsonProperty("id")]
        public long? Id { get; set; }

        /// <summary>
        /// Unknown fields preserved for forward compatibility.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; } = new Dictionary<string, JToken>();
    }

    /// <summary>
    /// Raw envelope for unknown or not-yet-modeled messages.
    /// </summary>
    public sealed class KalshiRawResponse : KalshiResponse
    {
        /// <summary>
        /// Raw message payload.
        /// </summary>
        [JsonProperty("msg")]
        public JToken Message { get; set; }
    }
}
