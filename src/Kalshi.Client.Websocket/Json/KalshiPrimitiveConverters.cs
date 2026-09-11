using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Kalshi.Client.Websocket.Json
{
    /// <summary>
    /// Converts Kalshi decimals that may arrive as JSON numbers, quoted numbers, empty strings, or nulls.
    /// </summary>
    internal sealed class KalshiDecimalConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var type = Nullable.GetUnderlyingType(objectType) ?? objectType;
            return type == typeof(decimal);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var nullable = Nullable.GetUnderlyingType(objectType) != null;
            if (reader.TokenType == JsonToken.Null)
            {
                return nullable ? null : (object)0m;
            }

            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToDecimal(reader.Value, CultureInfo.InvariantCulture);
            }

            var value = Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(value))
            {
                return nullable ? null : (object)0m;
            }

            return decimal.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue((decimal)value);
        }
    }

    /// <summary>
    /// Converts Kalshi integer values that may arrive as JSON numbers, quoted numbers,
    /// or RFC3339 timestamps (e.g. orderbook_delta "ts"), which are converted to unix seconds.
    /// </summary>
    internal sealed class KalshiLongConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var type = Nullable.GetUnderlyingType(objectType) ?? objectType;
            return type == typeof(long);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var nullable = Nullable.GetUnderlyingType(objectType) != null;
            if (reader.TokenType == JsonToken.Null)
            {
                return nullable ? null : (object)0L;
            }

            if (reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture);
            }

            if (reader.TokenType == JsonToken.Date)
            {
                return ToUnixSeconds(reader.Value);
            }

            var value = Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(value))
            {
                return nullable ? null : (object)0L;
            }

            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            if (DateTimeOffset.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var timestamp))
            {
                return timestamp.ToUnixTimeSeconds();
            }

            throw new JsonSerializationException($"Cannot convert '{value}' to a Kalshi integer value");
        }

        private static long ToUnixSeconds(object value)
        {
            switch (value)
            {
                case DateTimeOffset offset:
                    return offset.ToUnixTimeSeconds();
                case DateTime date:
                    return new DateTimeOffset(date.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                        : date.ToUniversalTime()).ToUnixTimeSeconds();
                default:
                    throw new JsonSerializationException($"Cannot convert '{value}' to a Kalshi integer value");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue((long)value);
        }
    }

    /// <summary>
    /// Converts Kalshi int values that may arrive as JSON numbers or quoted numbers.
    /// </summary>
    internal sealed class KalshiIntConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var type = Nullable.GetUnderlyingType(objectType) ?? objectType;
            return type == typeof(int);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var nullable = Nullable.GetUnderlyingType(objectType) != null;
            if (reader.TokenType == JsonToken.Null)
            {
                return nullable ? null : (object)0;
            }

            if (reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture);
            }

            var value = Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(value))
            {
                return nullable ? null : (object)0;
            }

            return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue((int)value);
        }
    }

    /// <summary>
    /// Converts DateTime fields while tolerating empty strings and Unix timestamps.
    /// </summary>
    internal sealed class KalshiDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var type = Nullable.GetUnderlyingType(objectType) ?? objectType;
            return type == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var nullable = Nullable.GetUnderlyingType(objectType) != null;
            if (reader.TokenType == JsonToken.Null)
            {
                return nullable ? null : (object)default(DateTime);
            }

            if (reader.TokenType == JsonToken.Date && reader.Value is DateTime date)
            {
                return date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
            }

            var value = Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(value))
            {
                return nullable ? null : (object)default(DateTime);
            }

            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unix))
            {
                return FromUnixTimestamp(unix);
            }

            return DateTime.Parse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(((DateTime)value).ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        }

        private static DateTime FromUnixTimestamp(long value)
        {
            return Math.Abs(value) >= 100000000000
                ? DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime
                : DateTimeOffset.FromUnixTimeSeconds(value).UtcDateTime;
        }
    }
}
