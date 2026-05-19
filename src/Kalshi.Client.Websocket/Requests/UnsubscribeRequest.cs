using System.Collections.Generic;
using Newtonsoft.Json;
using Kalshi.Client.Websocket.Enums;
using Kalshi.Client.Websocket.Validations;

namespace Kalshi.Client.Websocket.Requests
{
    /// <summary>
    /// Unsubscribe by subscription IDs.
    /// </summary>
    public class UnsubscribeRequest
    {
        /// <summary>
        /// Create unsubscribe request.
        /// </summary>
        public UnsubscribeRequest(long id, IEnumerable<long> subscriptionIds)
        {
            KalshiValidations.ValidateInput(subscriptionIds, nameof(subscriptionIds));

            Id = id;
            Params = new UnsubscribeParams(subscriptionIds);
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
        public KalshiCommand Command { get; } = KalshiCommand.Unsubscribe;

        /// <summary>
        /// Unsubscribe parameters.
        /// </summary>
        [JsonProperty("params")]
        public UnsubscribeParams Params { get; }
    }

    /// <summary>
    /// Unsubscribe command parameters.
    /// </summary>
    public class UnsubscribeParams
    {
        internal UnsubscribeParams(IEnumerable<long> subscriptionIds)
        {
            Sids = KalshiValidations.ValidateArray(subscriptionIds, nameof(subscriptionIds));
        }

        [JsonProperty("sids")]
        public long[] Sids { get; }
    }
}
