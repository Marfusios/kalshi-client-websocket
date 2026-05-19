using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kalshi.Client.Websocket.Enums;
using Kalshi.Client.Websocket.Json;

namespace Kalshi.Client.Websocket.Responses.MarketData
{
    /// <summary>
    /// Full orderbook snapshot response.
    /// </summary>
    public class OrderbookSnapshotResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public OrderbookSnapshotMessage Message { get; set; }
    }

    /// <summary>
    /// Incremental orderbook delta response.
    /// </summary>
    public class OrderbookDeltaResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public OrderbookDeltaMessage Message { get; set; }
    }

    /// <summary>
    /// Full orderbook snapshot payload.
    /// </summary>
    public class OrderbookSnapshotMessage
    {
        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("market_id")]
        public string MarketId { get; set; }

        [JsonProperty("yes")]
        [JsonConverter(typeof(KalshiOrderbookLevelsConverter))]
        public OrderbookLevel[] Yes { get; set; }

        [JsonProperty("no")]
        [JsonConverter(typeof(KalshiOrderbookLevelsConverter))]
        public OrderbookLevel[] No { get; set; }

        [JsonProperty("yes_dollars")]
        [JsonConverter(typeof(KalshiOrderbookLevelsConverter))]
        public OrderbookLevel[] YesDollars { get; set; }

        [JsonProperty("no_dollars")]
        [JsonConverter(typeof(KalshiOrderbookLevelsConverter))]
        public OrderbookLevel[] NoDollars { get; set; }
    }

    /// <summary>
    /// Incremental orderbook delta payload.
    /// </summary>
    public class OrderbookDeltaMessage
    {
        [JsonProperty("market_ticker")]
        public string MarketTicker { get; set; }

        [JsonProperty("market_id")]
        public string MarketId { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("price_dollars")]
        public decimal? PriceDollars { get; set; }

        [JsonProperty("delta")]
        public decimal Delta { get; set; }

        [JsonProperty("side")]
        public KalshiSide Side { get; set; }
    }

    /// <summary>
    /// Orderbook price level.
    /// </summary>
    public class OrderbookLevel
    {
        public OrderbookLevel()
        {
        }

        public OrderbookLevel(decimal price, decimal quantity)
        {
            Price = price;
            Quantity = quantity;
        }

        /// <summary>
        /// Price in cents or dollars, depending on the source field.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Available quantity.
        /// </summary>
        public decimal Quantity { get; set; }
    }

    internal sealed class KalshiOrderbookLevelsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(OrderbookLevel[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var array = JArray.Load(reader);
            if (array.Count == 2 && !(array[0] is JArray))
            {
                return new[]
                {
                    new OrderbookLevel(
                        array[0].ToObject<decimal>(KalshiJsonSerializer.Serializer),
                        array[1].ToObject<decimal>(KalshiJsonSerializer.Serializer))
                };
            }

            var levels = new OrderbookLevel[array.Count];
            for (var i = 0; i < array.Count; i++)
            {
                var token = array[i];
                if (token is JArray tuple && tuple.Count >= 2)
                {
                    levels[i] = new OrderbookLevel(tuple[0].ToObject<decimal>(KalshiJsonSerializer.Serializer), tuple[1].ToObject<decimal>(KalshiJsonSerializer.Serializer));
                    continue;
                }

                levels[i] = token.ToObject<OrderbookLevel>(KalshiJsonSerializer.Serializer);
            }

            return levels;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var levels = (OrderbookLevel[])value;
            writer.WriteStartArray();
            foreach (var level in levels)
            {
                writer.WriteStartArray();
                writer.WriteValue(level.Price);
                writer.WriteValue(level.Quantity);
                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}
