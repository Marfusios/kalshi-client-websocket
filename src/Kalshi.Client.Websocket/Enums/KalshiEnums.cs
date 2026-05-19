using System.Runtime.Serialization;
using Newtonsoft.Json;
using Kalshi.Client.Websocket.Json;

namespace Kalshi.Client.Websocket.Enums
{
    /// <summary>
    /// Kalshi websocket command.
    /// </summary>
    [JsonConverter(typeof(KalshiStringEnumConverter<KalshiCommand>))]
    public enum KalshiCommand
    {
        Unknown = 0,

        [EnumMember(Value = "subscribe")]
        Subscribe,

        [EnumMember(Value = "unsubscribe")]
        Unsubscribe,

        [EnumMember(Value = "update_subscription")]
        UpdateSubscription,

        [EnumMember(Value = "list_subscriptions")]
        ListSubscriptions
    }

    /// <summary>
    /// Kalshi websocket subscription channel.
    /// </summary>
    [JsonConverter(typeof(KalshiStringEnumConverter<KalshiChannel>))]
    public enum KalshiChannel
    {
        Unknown = 0,

        [EnumMember(Value = "orderbook_delta")]
        OrderbookDelta,

        [EnumMember(Value = "ticker")]
        Ticker,

        [EnumMember(Value = "trade")]
        Trade,

        [EnumMember(Value = "fill")]
        Fill,

        [EnumMember(Value = "market_positions")]
        MarketPositions,

        [EnumMember(Value = "market_lifecycle_v2")]
        MarketLifecycleV2,

        [EnumMember(Value = "multivariate_market_lifecycle")]
        MultivariateMarketLifecycle,

        [EnumMember(Value = "multivariate_lookup")]
        MultivariateLookup,

        [EnumMember(Value = "communications")]
        Communications,

        [EnumMember(Value = "order_group_updates")]
        OrderGroupUpdates,

        [EnumMember(Value = "user_orders")]
        UserOrders
    }

    /// <summary>
    /// Kalshi websocket message type.
    /// </summary>
    [JsonConverter(typeof(KalshiStringEnumConverter<KalshiMessageType>))]
    public enum KalshiMessageType
    {
        Unknown = 0,

        [EnumMember(Value = "subscribed")]
        Subscribed,

        [EnumMember(Value = "unsubscribed")]
        Unsubscribed,

        [EnumMember(Value = "ok")]
        Ok,

        [EnumMember(Value = "error")]
        Error,

        [EnumMember(Value = "orderbook_snapshot")]
        OrderbookSnapshot,

        [EnumMember(Value = "orderbook_delta")]
        OrderbookDelta,

        [EnumMember(Value = "ticker")]
        Ticker,

        [EnumMember(Value = "trade")]
        Trade,

        [EnumMember(Value = "fill")]
        Fill,

        [EnumMember(Value = "market_position")]
        MarketPosition,

        [EnumMember(Value = "market_lifecycle_v2")]
        MarketLifecycleV2,

        [EnumMember(Value = "event_lifecycle")]
        EventLifecycle,

        [EnumMember(Value = "multivariate_market_lifecycle")]
        MultivariateMarketLifecycle,

        [EnumMember(Value = "multivariate_lookup")]
        MultivariateLookup,

        [EnumMember(Value = "rfq_created")]
        RfqCreated,

        [EnumMember(Value = "rfq_deleted")]
        RfqDeleted,

        [EnumMember(Value = "quote_created")]
        QuoteCreated,

        [EnumMember(Value = "quote_accepted")]
        QuoteAccepted,

        [EnumMember(Value = "quote_executed")]
        QuoteExecuted,

        [EnumMember(Value = "order_group_updates")]
        OrderGroupUpdates,

        [EnumMember(Value = "user_order")]
        UserOrder
    }

    /// <summary>
    /// Binary market side.
    /// </summary>
    [JsonConverter(typeof(KalshiStringEnumConverter<KalshiSide>))]
    public enum KalshiSide
    {
        Unknown = 0,

        [EnumMember(Value = "yes")]
        Yes,

        [EnumMember(Value = "no")]
        No
    }

    /// <summary>
    /// Trade or order action.
    /// </summary>
    [JsonConverter(typeof(KalshiStringEnumConverter<KalshiAction>))]
    public enum KalshiAction
    {
        Unknown = 0,

        [EnumMember(Value = "buy")]
        Buy,

        [EnumMember(Value = "sell")]
        Sell
    }

    /// <summary>
    /// Subscription update action.
    /// </summary>
    [JsonConverter(typeof(KalshiStringEnumConverter<KalshiSubscriptionAction>))]
    public enum KalshiSubscriptionAction
    {
        Unknown = 0,

        [EnumMember(Value = "add_markets")]
        AddMarkets,

        [EnumMember(Value = "delete_markets")]
        DeleteMarkets,

        [EnumMember(Value = "get_snapshot")]
        GetSnapshot
    }

    /// <summary>
    /// Lifecycle event kind.
    /// </summary>
    [JsonConverter(typeof(KalshiStringEnumConverter<KalshiLifecycleEvent>))]
    public enum KalshiLifecycleEvent
    {
        Unknown = 0,

        [EnumMember(Value = "created")]
        Created,

        [EnumMember(Value = "activated")]
        Activated,

        [EnumMember(Value = "deactivated")]
        Deactivated,

        [EnumMember(Value = "close_date_updated")]
        CloseDateUpdated,

        [EnumMember(Value = "determined")]
        Determined,

        [EnumMember(Value = "settled")]
        Settled,

        [EnumMember(Value = "price_level_structure_updated")]
        PriceLevelStructureUpdated,

        [EnumMember(Value = "metadata_updated")]
        MetadataUpdated
    }

    /// <summary>
    /// Order status.
    /// </summary>
    [JsonConverter(typeof(KalshiStringEnumConverter<KalshiOrderStatus>))]
    public enum KalshiOrderStatus
    {
        Unknown = 0,

        [EnumMember(Value = "resting")]
        Resting,

        [EnumMember(Value = "executed")]
        Executed,

        [EnumMember(Value = "canceled")]
        Canceled,

        [EnumMember(Value = "pending")]
        Pending
    }

    /// <summary>
    /// Order group update event kind.
    /// </summary>
    [JsonConverter(typeof(KalshiStringEnumConverter<KalshiOrderGroupEvent>))]
    public enum KalshiOrderGroupEvent
    {
        Unknown = 0,

        [EnumMember(Value = "created")]
        Created,

        [EnumMember(Value = "updated")]
        Updated,

        [EnumMember(Value = "triggered")]
        Triggered,

        [EnumMember(Value = "reset")]
        Reset,

        [EnumMember(Value = "deleted")]
        Deleted
    }
}
