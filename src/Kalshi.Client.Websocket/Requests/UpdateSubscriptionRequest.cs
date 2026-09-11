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
        public UpdateSubscriptionRequest(
            long id,
            long subscriptionId,
            KalshiSubscriptionAction action,
            IEnumerable<string>? marketTickers = null,
            IEnumerable<string>? indexIds = null,
            IEnumerable<string>? underlyingTickers = null)
        {
            Id = id;
            Params = new UpdateSubscriptionParams(subscriptionId, null, action, marketTickers, indexIds, underlyingTickers);
        }

        /// <summary>
        /// Create update-subscription request using Kalshi's sids-array payload shape.
        /// </summary>
        public UpdateSubscriptionRequest(
            long id,
            IEnumerable<long> subscriptionIds,
            KalshiSubscriptionAction action,
            IEnumerable<string>? marketTickers = null,
            IEnumerable<string>? indexIds = null,
            IEnumerable<string>? underlyingTickers = null)
        {
            Id = id;
            Params = new UpdateSubscriptionParams(null, subscriptionIds, action, marketTickers, indexIds, underlyingTickers);
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
        internal UpdateSubscriptionParams(
            long? subscriptionId,
            IEnumerable<long>? subscriptionIds,
            KalshiSubscriptionAction action,
            IEnumerable<string>? marketTickers,
            IEnumerable<string>? indexIds,
            IEnumerable<string>? underlyingTickers)
        {
            if (action == KalshiSubscriptionAction.Unknown)
            {
                throw new Exceptions.KalshiBadInputException("Subscription update action is required");
            }

            Sid = subscriptionId;
            Sids = subscriptionIds == null ? null : ValidateSingleSubscriptionArray(subscriptionIds);
            Action = action;
            MarketTickers = marketTickers == null ? null : KalshiValidations.ValidateArray(marketTickers, nameof(marketTickers));
            IndexIds = indexIds == null ? null : KalshiValidations.ValidateArray(indexIds, nameof(indexIds));
            UnderlyingTickers = underlyingTickers == null ? null : KalshiValidations.ValidateArray(underlyingTickers, nameof(underlyingTickers));
        }

        [JsonProperty("sid", NullValueHandling = NullValueHandling.Ignore)]
        public long? Sid { get; }

        [JsonProperty("sids", NullValueHandling = NullValueHandling.Ignore)]
        public long[]? Sids { get; }

        [JsonProperty("action")]
        public KalshiSubscriptionAction Action { get; }

        [JsonProperty("market_tickers", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? MarketTickers { get; }

        /// <summary>
        /// CF Benchmarks index ids for subscribe_indices / unsubscribe_indices.
        /// </summary>
        [JsonProperty("index_ids", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? IndexIds { get; }

        /// <summary>
        /// Underlying tickers for subscribe_underlyings / unsubscribe_underlyings.
        /// </summary>
        [JsonProperty("underlying_tickers", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? UnderlyingTickers { get; }

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
