using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kalshi.Client.Websocket.Json;

namespace Kalshi.Client.Websocket.Responses.MarketData
{
    /// <summary>
    /// CF Benchmarks index value update (cfbenchmarks_value and cfbenchmarks_value_5hz channels).
    /// </summary>
    public class CfBenchmarksValueResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public CfBenchmarksValueMessage Message { get; set; }
    }

    /// <summary>
    /// CF Benchmarks index value payload.
    /// </summary>
    public class CfBenchmarksValueMessage
    {
        /// <summary>
        /// CF Benchmarks index id, e.g. "BRTI" or "ETHUSD_RTI".
        /// </summary>
        [JsonProperty("index_id")]
        public string IndexId { get; set; }

        /// <summary>
        /// When Kalshi received the upstream frame (unix milliseconds).
        /// </summary>
        [JsonProperty("received_at")]
        public long? ReceivedAt { get; set; }

        /// <summary>
        /// The raw CF Benchmarks JSON frame, as a string. Use <see cref="ParseFrame"/> to read it.
        /// </summary>
        [JsonProperty("data")]
        public string Data { get; set; }

        /// <summary>
        /// Trailing 60 second average of the index value.
        /// </summary>
        [JsonProperty("avg_60s_data")]
        public CfBenchmarksWindowedAverage Average60s { get; set; }

        /// <summary>
        /// Trailing 60 second average as used for the 15 minute markets (final minute before the quarter hour).
        /// </summary>
        [JsonProperty("last_60s_windowed_average_15min")]
        public CfBenchmarksWindowedAverage Average60s15Min { get; set; }

        /// <summary>
        /// Parsed index value, present on the 5Hz channel.
        /// </summary>
        [JsonProperty("value")]
        public decimal? Value { get; set; }

        /// <summary>
        /// Parsed index time (unix milliseconds), present on the 5Hz channel.
        /// </summary>
        [JsonProperty("time")]
        public long? Time { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }

        /// <summary>
        /// Kalshi receive time.
        /// </summary>
        [JsonIgnore]
        public DateTime? ReceivedTime => KalshiTimestampParser.ToDateTime(null, ReceivedAt);

        /// <summary>
        /// Parses the raw CF Benchmarks frame carried in <see cref="Data"/>.
        /// </summary>
        public CfBenchmarksFrame ParseFrame()
        {
            return string.IsNullOrWhiteSpace(Data) ? null : KalshiJsonSerializer.Deserialize<CfBenchmarksFrame>(Data);
        }
    }

    /// <summary>
    /// Windowed average metadata for a CF Benchmarks index value.
    /// </summary>
    public class CfBenchmarksWindowedAverage
    {
        [JsonProperty("value")]
        public decimal? Value { get; set; }

        /// <summary>
        /// Number of ticks counted in the window.
        /// </summary>
        [JsonProperty("window_size")]
        public int? WindowSize { get; set; }

        /// <summary>
        /// Window start boundary (unix milliseconds).
        /// </summary>
        [JsonProperty("window_start_ts_ms")]
        public long? WindowStartMilliseconds { get; set; }

        /// <summary>
        /// Window end boundary, exclusive (unix milliseconds).
        /// </summary>
        [JsonProperty("window_end_ts_exclusive")]
        public long? WindowEndExclusiveMilliseconds { get; set; }
    }

    /// <summary>
    /// Raw CF Benchmarks frame, e.g. {"type":"value","time":1789123105000,"id":"BRTI","value":"77002.30"}.
    /// </summary>
    public class CfBenchmarksFrame
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Source timestamp (unix milliseconds).
        /// </summary>
        [JsonProperty("time")]
        public long? Time { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("value")]
        public decimal? Value { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }

        [JsonIgnore]
        public DateTime? SourceTime => KalshiTimestampParser.ToDateTime(null, Time);
    }

    /// <summary>
    /// List of index ids available on a CF Benchmarks channel (response to the indexlist action).
    /// </summary>
    public class CfBenchmarksIndexListResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public CfBenchmarksIndexListMessage Message { get; set; }
    }

    /// <summary>
    /// Index list payload.
    /// </summary>
    public class CfBenchmarksIndexListMessage
    {
        [JsonProperty("index_ids")]
        public string[] IndexIds { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    /// <summary>
    /// Pyth price update (pyth_value channel), kept as a raw payload.
    /// </summary>
    public class PythValueResponse : KalshiResponse
    {
        [JsonProperty("msg")]
        public JObject Message { get; set; }
    }
}
