using System;
using System.Net.WebSockets;
using Kalshi.Client.Websocket.Authentication;
using Microsoft.Extensions.Logging;
using Kalshi.Client.Websocket.Communicator;
using Websocket.Client;

namespace Kalshi.Client.Websocket.Websockets
{
    /// <inheritdoc cref="WebsocketClient" />
    public class KalshiWebsocketCommunicator : WebsocketClient, IKalshiCommunicator
    {
        /// <inheritdoc />
        public KalshiWebsocketCommunicator(Uri url, KalshiAuthentication? authentication = null, Func<ClientWebSocket>? clientFactory = null)
            : base(url, () => CreateClient(authentication, clientFactory))
        {
        }

        /// <inheritdoc />
        public KalshiWebsocketCommunicator(Uri url, ILogger<KalshiWebsocketCommunicator> logger, KalshiAuthentication? authentication = null, Func<ClientWebSocket>? clientFactory = null)
            : base(url, logger, () => CreateClient(authentication, clientFactory))
        {
        }

        private static ClientWebSocket CreateClient(KalshiAuthentication? authentication, Func<ClientWebSocket>? clientFactory)
        {
            var socket = clientFactory == null ? new ClientWebSocket() : clientFactory();
            if (authentication == null)
            {
                return socket;
            }

            foreach (var header in authentication.CreateWebsocketHeaders())
            {
                socket.Options.SetRequestHeader(header.Key, header.Value);
            }

            return socket;
        }
    }
}
