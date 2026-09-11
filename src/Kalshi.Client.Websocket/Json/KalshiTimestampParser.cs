using System;
using System.Globalization;

namespace Kalshi.Client.Websocket.Json
{
    /// <summary>
    /// Parses Kalshi timestamp fields that arrive either as unix seconds or as RFC3339 strings.
    /// </summary>
    internal static class KalshiTimestampParser
    {
        /// <summary>
        /// Unix seconds for a raw "ts" value (unix seconds or RFC3339).
        /// </summary>
        public static long? ToUnixSeconds(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
            {
                return seconds;
            }

            return TryParseRfc3339(raw, out var parsed) ? parsed.ToUnixTimeSeconds() : (long?)null;
        }

        /// <summary>
        /// Most precise available time: RFC3339 string (sub-millisecond), then unix milliseconds, then unix seconds.
        /// </summary>
        public static DateTime? ToDateTime(string? raw, long? milliseconds)
        {
            long? seconds = null;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedSeconds))
                {
                    seconds = parsedSeconds;
                }
                else if (TryParseRfc3339(raw, out var parsed))
                {
                    return parsed.UtcDateTime;
                }
            }

            if (milliseconds.HasValue && milliseconds.Value > 0)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds.Value).UtcDateTime;
            }

            if (seconds.HasValue && seconds.Value > 0)
            {
                return DateTimeOffset.FromUnixTimeSeconds(seconds.Value).UtcDateTime;
            }

            return null;
        }

        private static bool TryParseRfc3339(string raw, out DateTimeOffset parsed)
        {
            return DateTimeOffset.TryParse(
                raw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out parsed);
        }
    }
}
