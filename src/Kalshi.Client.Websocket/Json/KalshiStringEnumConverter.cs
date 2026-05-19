using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Kalshi.Client.Websocket.Json
{
    /// <summary>
    /// Converts Kalshi string enum values while tolerating casing and future unknown values.
    /// </summary>
    public sealed class KalshiStringEnumConverter<TEnum> : JsonConverter where TEnum : struct
    {
        private static readonly Dictionary<string, TEnum> ReadMap = CreateReadMap();
        private static readonly Dictionary<TEnum, string> WriteMap = CreateWriteMap();

        public override bool CanConvert(Type objectType)
        {
            var type = Nullable.GetUnderlyingType(objectType) ?? objectType;
            return type == typeof(TEnum);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var nullable = Nullable.GetUnderlyingType(objectType) != null;
            if (reader.TokenType == JsonToken.Null)
            {
                return nullable ? null : (object)default(TEnum);
            }

            if (reader.TokenType == JsonToken.Integer)
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture));
            }

            var value = Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(value))
            {
                return nullable ? null : (object)default(TEnum);
            }

            var normalized = Normalize(value.Trim());
            if (ReadMap.TryGetValue(normalized, out var mapped))
            {
                return mapped;
            }

            if (Enum.TryParse(normalized, ignoreCase: true, result: out TEnum parsed))
            {
                return parsed;
            }

            return default(TEnum);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var enumValue = (TEnum)value;
            if (WriteMap.TryGetValue(enumValue, out var mapped))
            {
                writer.WriteValue(mapped);
                return;
            }

            writer.WriteValue(enumValue.ToString());
        }

        private static Dictionary<string, TEnum> CreateReadMap()
        {
            var map = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var enumValue = (TEnum)field.GetValue(null);
                map[field.Name] = enumValue;

                var enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
                if (!string.IsNullOrWhiteSpace(enumMember?.Value))
                {
                    map[enumMember.Value] = enumValue;
                }
            }

            return map;
        }

        private static Dictionary<TEnum, string> CreateWriteMap()
        {
            var map = new Dictionary<TEnum, string>();
            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var enumValue = (TEnum)field.GetValue(null);
                var enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
                map[enumValue] = string.IsNullOrWhiteSpace(enumMember?.Value) ? field.Name : enumMember.Value;
            }

            return map;
        }

        private static string Normalize(string value) => value;
    }
}
