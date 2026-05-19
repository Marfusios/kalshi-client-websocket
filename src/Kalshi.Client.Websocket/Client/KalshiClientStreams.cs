using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Kalshi.Client.Websocket.Responses;
using Kalshi.Client.Websocket.Responses.Control;
using Kalshi.Client.Websocket.Responses.MarketData;
using Kalshi.Client.Websocket.Responses.Private;

namespace Kalshi.Client.Websocket.Client
{
    /// <summary>
    /// All provided streams from Kalshi websocket API v2.
    /// </summary>
    public class KalshiClientStreams
    {
        internal readonly Subject<string> RawMessageSubject = new Subject<string>();
        internal readonly Subject<KalshiRawResponse> UnknownMessageSubject = new Subject<KalshiRawResponse>();

        internal readonly Subject<SubscribedResponse> SubscribedSubject = new Subject<SubscribedResponse>();
        internal readonly Subject<UnsubscribedResponse> UnsubscribedSubject = new Subject<UnsubscribedResponse>();
        internal readonly Subject<OkResponse> OkSubject = new Subject<OkResponse>();
        internal readonly Subject<ErrorResponse> ErrorSubject = new Subject<ErrorResponse>();

        internal readonly Subject<OrderbookSnapshotResponse> OrderbookSnapshotSubject = new Subject<OrderbookSnapshotResponse>();
        internal readonly Subject<OrderbookDeltaResponse> OrderbookDeltaSubject = new Subject<OrderbookDeltaResponse>();
        internal readonly Subject<TickerResponse> TickerSubject = new Subject<TickerResponse>();
        internal readonly Subject<TradeResponse> TradeSubject = new Subject<TradeResponse>();
        internal readonly Subject<MarketLifecycleResponse> MarketLifecycleSubject = new Subject<MarketLifecycleResponse>();
        internal readonly Subject<EventLifecycleResponse> EventLifecycleSubject = new Subject<EventLifecycleResponse>();
        internal readonly Subject<MultivariateMarketLifecycleResponse> MultivariateMarketLifecycleSubject = new Subject<MultivariateMarketLifecycleResponse>();
        internal readonly Subject<MultivariateLookupResponse> MultivariateLookupSubject = new Subject<MultivariateLookupResponse>();

        internal readonly Subject<FillResponse> FillSubject = new Subject<FillResponse>();
        internal readonly Subject<MarketPositionResponse> MarketPositionSubject = new Subject<MarketPositionResponse>();
        internal readonly Subject<UserOrderResponse> UserOrderSubject = new Subject<UserOrderResponse>();
        internal readonly Subject<OrderGroupUpdateResponse> OrderGroupUpdateSubject = new Subject<OrderGroupUpdateResponse>();
        internal readonly Subject<RfqCreatedResponse> RfqCreatedSubject = new Subject<RfqCreatedResponse>();
        internal readonly Subject<RfqDeletedResponse> RfqDeletedSubject = new Subject<RfqDeletedResponse>();
        internal readonly Subject<QuoteCreatedResponse> QuoteCreatedSubject = new Subject<QuoteCreatedResponse>();
        internal readonly Subject<QuoteAcceptedResponse> QuoteAcceptedSubject = new Subject<QuoteAcceptedResponse>();
        internal readonly Subject<QuoteExecutedResponse> QuoteExecutedSubject = new Subject<QuoteExecutedResponse>();

        /// <summary>
        /// Raw text messages received from websocket.
        /// </summary>
        public IObservable<string> RawMessageStream => RawMessageSubject.AsObservable();

        /// <summary>
        /// Unknown or not-yet-modeled messages.
        /// </summary>
        public IObservable<KalshiRawResponse> UnknownMessageStream => UnknownMessageSubject.AsObservable();

        public IObservable<SubscribedResponse> SubscribedStream => SubscribedSubject.AsObservable();

        public IObservable<UnsubscribedResponse> UnsubscribedStream => UnsubscribedSubject.AsObservable();

        public IObservable<OkResponse> OkStream => OkSubject.AsObservable();

        public IObservable<ErrorResponse> ErrorStream => ErrorSubject.AsObservable();

        public IObservable<OrderbookSnapshotResponse> OrderbookSnapshotStream => OrderbookSnapshotSubject.AsObservable();

        public IObservable<OrderbookDeltaResponse> OrderbookDeltaStream => OrderbookDeltaSubject.AsObservable();

        public IObservable<TickerResponse> TickerStream => TickerSubject.AsObservable();

        public IObservable<TradeResponse> TradeStream => TradeSubject.AsObservable();

        public IObservable<MarketLifecycleResponse> MarketLifecycleStream => MarketLifecycleSubject.AsObservable();

        public IObservable<EventLifecycleResponse> EventLifecycleStream => EventLifecycleSubject.AsObservable();

        public IObservable<MultivariateMarketLifecycleResponse> MultivariateMarketLifecycleStream => MultivariateMarketLifecycleSubject.AsObservable();

        public IObservable<MultivariateLookupResponse> MultivariateLookupStream => MultivariateLookupSubject.AsObservable();

        public IObservable<FillResponse> FillStream => FillSubject.AsObservable();

        public IObservable<MarketPositionResponse> MarketPositionStream => MarketPositionSubject.AsObservable();

        public IObservable<UserOrderResponse> UserOrderStream => UserOrderSubject.AsObservable();

        public IObservable<OrderGroupUpdateResponse> OrderGroupUpdateStream => OrderGroupUpdateSubject.AsObservable();

        public IObservable<RfqCreatedResponse> RfqCreatedStream => RfqCreatedSubject.AsObservable();

        public IObservable<RfqDeletedResponse> RfqDeletedStream => RfqDeletedSubject.AsObservable();

        public IObservable<QuoteCreatedResponse> QuoteCreatedStream => QuoteCreatedSubject.AsObservable();

        public IObservable<QuoteAcceptedResponse> QuoteAcceptedStream => QuoteAcceptedSubject.AsObservable();

        public IObservable<QuoteExecutedResponse> QuoteExecutedStream => QuoteExecutedSubject.AsObservable();
    }
}
