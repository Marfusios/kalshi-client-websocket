using System.Collections.Generic;
using Newtonsoft.Json;
using Kalshi.Client.Websocket.Enums;
using Kalshi.Client.Websocket.Validations;

namespace Kalshi.Client.Websocket.Requests
{
    /// <summary>
    /// Update an existing subscription.
    /// </summary>
    public class UpdateSubscriptionRequest
    {
        /// <summary>
        /// Create update-subscription request.
        /// </summary>
        public UpdateSubscriptionRequest(long id, long subscriptionId, KalshiSubscriptionAction action, IEnumerable<string>? marketTickers = null)
        {
            Id = id;
            Params = new UpdateSubscriptionParams(subscriptionId, null, action, marketTickers);
        }

        /// <summary>
        /// Create update-subscription request using Kalshi's sids-array payload shape.
        /// </summary>
        public UpdateSubscriptionRequest(long id, IEnumerable<long> subscriptionIds, KalshiSubscriptionAction action, IEnumerable<string>? marketTickers = null)
        {
            Id = id;
            Params = new UpdateSubscriptionParams(null, subscriptionIds, action, marketTickers);
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
        public KalshiCommand Command { get; } = KalshiCommand.UpdateSubscription;

        /// <summary>
        /// Update parameters.
        /// </summary>
        [JsonProperty("params")]
        public UpdateSubscriptionParams Params { get; }
    }

    /// <summary>
    /// Update-subscription command parameters.
    /// </summary>
    public class UpdateSubscriptionParams
    {
        internal UpdateSubscriptionParams(long? subscriptionId, IEnumerable<long>? subscriptionIds, KalshiSubscriptionAction action, IEnumerable<string>? marketTickers)
        {
            if (action == KalshiSubscriptionAction.Unknown)
            {
                throw new Exceptions.KalshiBadInputException("Subscription update action is required");
            }

            Sid = subscriptionId;
            Sids = subscriptionIds == null ? null : ValidateSingleSubscriptionArray(subscriptionIds);
            Action = action;
            MarketTickers = marketTickers == null ? null : KalshiValidations.ValidateArray(marketTickers, nameof(marketTickers));
        }

        [JsonProperty("sid", NullValueHandling = NullValueHandling.Ignore)]
        public long? Sid { get; }

        [JsonProperty("sids", NullValueHandling = NullValueHandling.Ignore)]
        public long[]? Sids { get; }

        [JsonProperty("action")]
        public KalshiSubscriptionAction Action { get; }

        [JsonProperty("market_tickers", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? MarketTickers { get; }

        private static long[] ValidateSingleSubscriptionArray(IEnumerable<long> subscriptionIds)
        {
            var sids = KalshiValidations.ValidateArray(subscriptionIds, nameof(subscriptionIds));
            if (sids.Length != 1)
            {
                throw new Exceptions.KalshiBadInputException("Exactly one subscription ID is required for update_subscription");
            }

            return sids;
        }
    }
}
