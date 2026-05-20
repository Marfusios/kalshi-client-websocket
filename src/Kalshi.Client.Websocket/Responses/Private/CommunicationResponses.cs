using System;
using Newtonsoft.Json;
using Kalshi.Client.Websocket.Enums;

namespace Kalshi.Client.Websocket.Responses.Private
{
    /// <summary>
    /// RFQ created response.
    /// </summary>
    public class RfqCreatedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public RfqCreatedMessage Message { get; set; }
    }

    /// <summary>
    /// RFQ deleted response.
    /// </summary>
    public class RfqDeletedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public RfqDeletedMessage Message { get; set; }
    }

    /// <summary>
    /// Quote created response.
    /// </summary>
    public class QuoteCreatedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public QuoteCreatedMessage Message { get; set; }
    }

    /// <summary>
    /// Quote accepted response.
    /// </summary>
    public class QuoteAcceptedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public QuoteAcceptedMessage Message { get; set; }
    }

    /// <summary>
    /// Quote executed response.
    /// </summary>
    public class QuoteExecutedResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public QuoteExecutedMessage Message { get; set; }
    }

    public class RfqCreatedMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("creator_id")]
        public string CreatorId { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("event_ticker")]
        public string EventTicker { get; set; }

        [JsonProperty("contracts_fp")]
        public decimal? ContractsFp { get; set; }

        [JsonProperty("target_cost_dollars")]
        public decimal? TargetCostDollars { get; set; }

        [JsonProperty("created_ts")]
        public DateTime? CreatedTimestamp { get; set; }

        [JsonProperty("mve_collection_ticker")]
        public string MveCollectionTicker { get; set; }

        [JsonProperty("mve_selected_legs")]
        public MultivariateSelectedLeg[] MveSelectedLegs { get; set; }
    }

    public class RfqDeletedMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("creator_id")]
        public string CreatorId { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("event_ticker")]
        public string EventTicker { get; set; }

        [JsonProperty("contracts_fp")]
        public decimal? ContractsFp { get; set; }

        [JsonProperty("target_cost_dollars")]
        public decimal? TargetCostDollars { get; set; }

        [JsonProperty("deleted_ts")]
        public DateTime? DeletedTimestamp { get; set; }
    }

    public class QuoteCreatedMessage
    {
        [JsonProperty("quote_id")]
        public string QuoteId { get; set; }

        [JsonProperty("rfq_id")]
        public string RfqId { get; set; }

        [JsonProperty("quote_creator_id")]
        public string QuoteCreatorId { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("event_ticker")]
        public string EventTicker { get; set; }

        [JsonProperty("yes_bid_dollars")]
        public decimal? YesBidDollars { get; set; }

        [JsonProperty("no_bid_dollars")]
        public decimal? NoBidDollars { get; set; }

        [JsonProperty("yes_contracts_offered_fp")]
        public decimal? YesContractsOfferedFp { get; set; }

        [JsonProperty("no_contracts_offered_fp")]
        public decimal? NoContractsOfferedFp { get; set; }

        [JsonProperty("rfq_target_cost_dollars")]
        public decimal? RfqTargetCostDollars { get; set; }

        [JsonProperty("created_ts")]
        public DateTime? CreatedTimestamp { get; set; }
    }

    public class QuoteAcceptedMessage : QuoteCreatedMessage
    {
        [JsonProperty("accepted_side")]
        public KalshiSide AcceptedSide { get; set; }

        [JsonProperty("contracts_accepted_fp")]
        public decimal? ContractsAcceptedFp { get; set; }
    }

    public class QuoteExecutedMessage
    {
        [JsonProperty("quote_id")]
        public string QuoteId { get; set; }

        [JsonProperty("rfq_id")]
        public string RfqId { get; set; }

        [JsonProperty("quote_creator_id")]
        public string QuoteCreatorId { get; set; }

        [JsonProperty("rfq_creator_id")]
        public string RfqCreatorId { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("client_order_id")]
        public string ClientOrderId { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("executed_ts")]
        public DateTime? ExecutedTimestamp { get; set; }
    }

    public class MultivariateSelectedLeg
    {
        [JsonProperty("event_ticker")]
        public string EventTicker { get; set; }

        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("side")]
        public KalshiSide Side { get; set; }
    }
}
