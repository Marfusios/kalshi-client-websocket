using System;

namespace Kalshi.Client.Websocket
{
    /// <summary>
    /// Kalshi API endpoints.
    /// </summary>
    public static class KalshiValues
    {
        /// <summary>
        /// Production REST API base URL.
        /// </summary>
        public static readonly Uri TradeApiBaseUrl = new Uri("https://external-api.kalshi.com/trade-api/v2/");

        /// <summary>
        /// Demo REST API base URL.
        /// </summary>
        public static readonly Uri DemoTradeApiBaseUrl = new Uri("https://external-api.demo.kalshi.co/trade-api/v2/");

        /// <summary>
        /// Also-supported production REST API base URL.
        /// </summary>
        public static readonly Uri SharedTradeApiBaseUrl = new Uri("https://api.elections.kalshi.com/trade-api/v2/");

        /// <summary>
        /// Also-supported demo REST API base URL.
        /// </summary>
        public static readonly Uri SharedDemoTradeApiBaseUrl = new Uri("https://demo-api.kalshi.co/trade-api/v2/");

        /// <summary>
        /// Production websocket API v2 endpoint.
        /// </summary>
        public static readonly Uri TradeWebsocketApiUrl = new Uri("wss://external-api-ws.kalshi.com/trade-api/ws/v2");

        /// <summary>
        /// Demo websocket API v2 endpoint.
        /// </summary>
        public static readonly Uri DemoTradeWebsocketApiUrl = new Uri("wss://external-api-ws.demo.kalshi.co/trade-api/ws/v2");

        /// <summary>
        /// Also-supported production websocket API v2 endpoint.
        /// </summary>
        public static readonly Uri SharedTradeWebsocketApiUrl = new Uri("wss://api.elections.kalshi.com/trade-api/ws/v2");

        /// <summary>
        /// Also-supported demo websocket API v2 endpoint.
        /// </summary>
        public static readonly Uri SharedDemoTradeWebsocketApiUrl = new Uri("wss://demo-api.kalshi.co/trade-api/ws/v2");

        /// <summary>
        /// Path included in the websocket authentication signature.
        /// </summary>
        public const string WebsocketRequestPath = "/trade-api/ws/v2";
    }
}
