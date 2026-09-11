using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kalshi.Client.Websocket.Json;
using Kalshi.Client.Websocket.Responses;
using Kalshi.Client.Websocket.Responses.Control;
using Kalshi.Client.Websocket.Responses.MarketData;
using Kalshi.Client.Websocket.Responses.Private;

namespace Kalshi.Client.Websocket.Client
{
    internal class KalshiMessageHandler
    {
        private readonly KalshiClientStreams _streams;
        private readonly ILogger _logger;

        public KalshiMessageHandler(KalshiClientStreams streams, ILogger logger)
        {
            _streams = streams;
            _logger = logger;
        }

        public void HandleMessage(string message)
        {
            HandleToken(ParseToken(message));
        }

        /// <summary>
        /// Parses without automatic date detection so RFC3339 strings reach the typed converters as strings.
        /// </summary>
        private static JToken ParseToken(string message)
        {
            using (var reader = new JsonTextReader(new StringReader(message)))
            {
                reader.DateParseHandling = DateParseHandling.None;
                return JToken.ReadFrom(reader);
            }
        }

        private void HandleToken(JToken token)
        {
            if (token.Type == JTokenType.Array)
            {
                foreach (var item in token.Children())
                {
                    HandleToken(item);
                }

                return;
            }

            var obj = token as JObject;
            if (obj == null)
            {
                _logger.LogDebug("Unhandled non-object message: {message}", token.ToString());
                return;
            }

            var type = obj["type"]?.Value<string>();
            if (string.IsNullOrWhiteSpace(type))
            {
                PublishUnknown(obj);
                return;
            }

            switch (type)
            {
                case "subscribed":
                    Publish(obj, _streams.SubscribedSubject);
                    break;
                case "unsubscribed":
                    Publish(obj, _streams.UnsubscribedSubject);
                    break;
                case "ok":
                    Publish(obj, _streams.OkSubject);
                    break;
                case "error":
                    Publish(obj, _streams.ErrorSubject);
                    break;
                case "orderbook_snapshot":
                    Publish(obj, _streams.OrderbookSnapshotSubject);
                    break;
                case "orderbook_delta":
                    Publish(obj, _streams.OrderbookDeltaSubject);
                    break;
                case "ticker":
                    Publish(obj, _streams.TickerSubject);
                    break;
                case "trade":
                    Publish(obj, _streams.TradeSubject);
                    break;
                case "market_lifecycle_v2":
                    Publish(obj, _streams.MarketLifecycleSubject);
                    break;
                case "event_lifecycle":
                    Publish(obj, _streams.EventLifecycleSubject);
                    break;
                case "multivariate_market_lifecycle":
                    Publish(obj, _streams.MultivariateMarketLifecycleSubject);
                    break;
                case "multivariate_lookup":
                    Publish(obj, _streams.MultivariateLookupSubject);
                    break;
                case "fill":
                    Publish(obj, _streams.FillSubject);
                    break;
                case "market_position":
                    Publish(obj, _streams.MarketPositionSubject);
                    break;
                case "user_order":
                    Publish(obj, _streams.UserOrderSubject);
                    break;
                case "order_group_updates":
                    Publish(obj, _streams.OrderGroupUpdateSubject);
                    break;
                case "rfq_created":
                    Publish(obj, _streams.RfqCreatedSubject);
                    break;
                case "rfq_deleted":
                    Publish(obj, _streams.RfqDeletedSubject);
                    break;
                case "quote_created":
                    Publish(obj, _streams.QuoteCreatedSubject);
                    break;
                case "quote_accepted":
                    Publish(obj, _streams.QuoteAcceptedSubject);
                    break;
                case "quote_executed":
                    Publish(obj, _streams.QuoteExecutedSubject);
                    break;
                case "cfbenchmarks_value":
                    Publish(obj, _streams.CfBenchmarksValueSubject);
                    break;
                case "cfbenchmarks_value_5hz":
                    Publish(obj, _streams.CfBenchmarksValue5HzSubject);
                    break;
                case "cfbenchmarks_value_indexlist":
                case "cfbenchmarks_value_5hz_indexlist":
                    Publish(obj, _streams.CfBenchmarksIndexListSubject);
                    break;
                case "pyth_value":
                    Publish(obj, _streams.PythValueSubject);
                    break;
                default:
                    _logger.LogDebug("Unhandled message type: {type}", type);
                    PublishUnknown(obj);
                    break;
            }
        }

        private void PublishUnknown(JObject obj)
        {
            var response = obj.ToObject<KalshiRawResponse>(KalshiJsonSerializer.Serializer);
            if (response != null)
            {
                _streams.UnknownMessageSubject.OnNext(response);
            }
        }

        private static void Publish<T>(JToken token, IObserver<T> subject)
        {
            var response = token.ToObject<T>(KalshiJsonSerializer.Serializer);
            if (response != null)
            {
                subject.OnNext(response);
            }
        }
    }
}
