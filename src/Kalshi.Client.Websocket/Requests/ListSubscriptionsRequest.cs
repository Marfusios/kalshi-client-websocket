using Newtonsoft.Json;
using Kalshi.Client.Websocket.Enums;

namespace Kalshi.Client.Websocket.Requests
{
    /// <summary>
    /// Request the active websocket subscriptions.
    /// </summary>
    public class ListSubscriptionsRequest
    {
        /// <summary>
        /// Create list-subscriptions request.
        /// </summary>
        public ListSubscriptionsRequest(long id)
        {
            Id = id;
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
        public KalshiCommand Command { get; } = KalshiCommand.ListSubscriptions;
    }
}
