using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Kalshi.Client.Websocket.Communicator;
using Kalshi.Client.Websocket.Json;
using Kalshi.Client.Websocket.Validations;
using Websocket.Client;

namespace Kalshi.Client.Websocket.Client
{
    /// <summary>
    /// Kalshi websocket client.
    /// Send subscription requests and subscribe to <see cref="Streams"/>.
    /// </summary>
    public class KalshiWebsocketClient : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IKalshiCommunicator _communicator;
        private readonly IDisposable _messageReceivedSubscription;
        private readonly KalshiMessageHandler _messageHandler;

        /// <summary>
        /// Create a Kalshi websocket client.
        /// </summary>
        public KalshiWebsocketClient(IKalshiCommunicator communicator, ILogger<KalshiWebsocketClient>? logger = null)
        {
            KalshiValidations.ValidateInput(communicator, nameof(communicator));

            _communicator = communicator;
            _logger = logger ?? NullLogger<KalshiWebsocketClient>.Instance;
            _messageHandler = new KalshiMessageHandler(Streams, _logger);
            _messageReceivedSubscription = _communicator.MessageReceived.Subscribe(HandleMessage);
        }

        /// <summary>
        /// Provided message streams.
        /// </summary>
        public KalshiClientStreams Streams { get; } = new KalshiClientStreams();

        /// <summary>
        /// Expose logger for this client.
        /// </summary>
        public ILogger Logger => _logger;

        /// <summary>
        /// Cleanup everything.
        /// </summary>
        public void Dispose()
        {
            _messageReceivedSubscription?.Dispose();
        }

        /// <summary>
        /// Serializes request and sends it through websocket communicator.
        /// </summary>
        public bool Send<T>(T request)
        {
            try
            {
                KalshiValidations.ValidateInput(request, nameof(request));

                var serialized = KalshiJsonSerializer.Serialize(request);
                return _communicator.Send(serialized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while sending message '{request}'. Error: {error}", request, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Send a raw text message.
        /// </summary>
        public bool SendRaw(string message)
        {
            try
            {
                KalshiValidations.ValidateNotEmpty(message, nameof(message));
                return _communicator.Send(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while sending raw message '{message}'. Error: {error}", message, e.Message);
                throw;
            }
        }

        private void HandleMessage(ResponseMessage message)
        {
            try
            {
                var formatted = (message.Text ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(formatted))
                {
                    return;
                }

                Streams.RawMessageSubject.OnNext(formatted);
                _messageHandler.HandleMessage(formatted);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while receiving message, error: {error}", e.Message);
            }
        }
    }
}
