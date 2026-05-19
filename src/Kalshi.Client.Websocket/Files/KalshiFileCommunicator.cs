using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Kalshi.Client.Websocket.Communicator;
using Websocket.Client;

namespace Kalshi.Client.Websocket.Files
{
    /// <summary>
    /// Communicator that loads raw websocket data from files and streams it through the client.
    /// </summary>
    public class KalshiFileCommunicator : IKalshiCommunicator
    {
        private readonly Subject<ResponseMessage> _messageReceivedSubject = new Subject<ResponseMessage>();

        /// <inheritdoc />
        public IObservable<ResponseMessage> MessageReceived => _messageReceivedSubject.AsObservable();

        /// <inheritdoc />
        public IObservable<ReconnectionInfo> ReconnectionHappened => Observable.Empty<ReconnectionInfo>();

        /// <inheritdoc />
        public IObservable<DisconnectionInfo> DisconnectionHappened => Observable.Empty<DisconnectionInfo>();

        /// <inheritdoc />
        public TimeSpan ConnectTimeout { get; set; }

        /// <inheritdoc />
        public TimeSpan? ReconnectTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <inheritdoc />
        public TimeSpan? ErrorReconnectTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <inheritdoc />
        public TimeSpan? LostReconnectTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <inheritdoc />
        public string Name { get; set; } = "KalshiFile";

        /// <inheritdoc />
        public bool IsStarted { get; private set; }

        /// <inheritdoc />
        public bool IsRunning { get; private set; }

        /// <inheritdoc />
        public bool TextSenderRunning { get; set; }

        /// <inheritdoc />
        public bool BinarySenderRunning { get; set; }

        /// <inheritdoc />
        public bool IsInsideLock { get; set; }

        /// <inheritdoc />
        public bool IsReconnectionEnabled { get; set; }

        /// <inheritdoc />
        public ClientWebSocket NativeClient { get; } = new ClientWebSocket();

        /// <inheritdoc />
        public bool IsStreamDisposedAutomatically { get; set; }

        /// <inheritdoc />
        public Encoding MessageEncoding { get; set; } = Encoding.UTF8;

        /// <inheritdoc />
        public bool IsTextMessageConversionEnabled { get; set; }

        /// <summary>
        /// Files with raw websocket messages.
        /// </summary>
        public string[] FileNames { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Delimiter between messages in the raw files.
        /// </summary>
        public string Delimiter { get; set; } = ";;";

        /// <summary>
        /// Encoding used by source files.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <inheritdoc />
        public Uri Url { get; set; } = KalshiValues.TradeWebsocketApiUrl;

        /// <inheritdoc />
        public void Dispose()
        {
            _messageReceivedSubject.Dispose();
            NativeClient.Dispose();
        }

        /// <inheritdoc />
        public Task Start()
        {
            IsStarted = true;
            IsRunning = true;
            StartStreaming();
            IsRunning = false;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StartOrFail()
        {
            return Start();
        }

        /// <inheritdoc />
        public Task<bool> Stop(WebSocketCloseStatus status, string statusDescription)
        {
            IsRunning = false;
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> StopOrFail(WebSocketCloseStatus status, string statusDescription)
        {
            return Stop(status, statusDescription);
        }

        /// <inheritdoc />
        public bool Send(string message)
        {
            return true;
        }

        /// <inheritdoc />
        public bool Send(byte[] message)
        {
            return true;
        }

        /// <inheritdoc />
        public bool Send(ArraySegment<byte> message)
        {
            return true;
        }

        /// <inheritdoc />
        public bool Send(ReadOnlySequence<byte> message)
        {
            return true;
        }

        /// <inheritdoc />
        public Task SendInstant(string message)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SendInstant(byte[] message)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool SendAsText(byte[] message)
        {
            return true;
        }

        /// <inheritdoc />
        public bool SendAsText(ArraySegment<byte> message)
        {
            return true;
        }

        /// <inheritdoc />
        public bool SendAsText(ReadOnlySequence<byte> message)
        {
            return true;
        }

        /// <inheritdoc />
        public Task Reconnect()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ReconnectOrFail()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void StreamFakeMessage(ResponseMessage message)
        {
            _messageReceivedSubject.OnNext(message);
        }

        private void StartStreaming()
        {
            if (FileNames == null || FileNames.Length == 0)
            {
                throw new InvalidOperationException("FileNames are not set, provide at least one path to raw data");
            }

            if (string.IsNullOrEmpty(Delimiter))
            {
                throw new InvalidOperationException("Delimiter is not set");
            }

            foreach (var fileName in FileNames)
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var stream = new StreamReader(fs, Encoding))
                {
                    var message = ReadByDelimiter(stream, Delimiter);
                    while (message != null)
                    {
                        _messageReceivedSubject.OnNext(ResponseMessage.TextMessage(message));
                        message = ReadByDelimiter(stream, Delimiter);
                    }
                }
            }
        }

        private static string ReadByDelimiter(StreamReader sr, string delimiter)
        {
            var line = new StringBuilder();
            var matchIndex = 0;

            while (sr.Peek() >= 0)
            {
                var nextChar = (char)sr.Read();
                line.Append(nextChar);

                if (nextChar == delimiter[matchIndex])
                {
                    if (matchIndex == delimiter.Length - 1)
                    {
                        return line.ToString().Substring(0, line.Length - delimiter.Length);
                    }

                    matchIndex++;
                    continue;
                }

                matchIndex = 0;
            }

            return line.Length == 0 ? null : line.ToString();
        }
    }
}
