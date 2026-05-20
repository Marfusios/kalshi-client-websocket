using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Kalshi.Client.Websocket;
using Kalshi.Client.Websocket.Authentication;
using Kalshi.Client.Websocket.Client;
using Kalshi.Client.Websocket.Enums;
using Kalshi.Client.Websocket.Files;
using Kalshi.Client.Websocket.Requests;
using Kalshi.Client.Websocket.Websockets;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Kalshi.Client.Websocket.Sample
{
    internal static class Program
    {
        private static readonly ManualResetEvent ExitEvent = new ManualResetEvent(false);

        private static async Task Main()
        {
            var logger = InitLogging();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            Console.WriteLine("|====================|");
            Console.WriteLine("|   KALSHI CLIENT    |");
            Console.WriteLine("|====================|");
            Console.WriteLine();

            var keyId = Environment.GetEnvironmentVariable("KALSHI_API_KEY_ID");
            var privateKeyPath = Environment.GetEnvironmentVariable("KALSHI_PRIVATE_KEY_PATH");
            var marketTicker = Environment.GetEnvironmentVariable("KALSHI_MARKET_TICKER");
            if (!string.IsNullOrWhiteSpace(keyId) &&
                !string.IsNullOrWhiteSpace(privateKeyPath) &&
                !string.IsNullOrWhiteSpace(marketTicker))
            {
                await RunLive(logger, keyId, privateKeyPath, marketTicker);
            }
            else
            {
                await RunReplay();
            }

            Log.CloseAndFlush();
        }

        private static async Task RunLive(SerilogLoggerFactory logger, string keyId, string privateKeyPath, string marketTicker)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(privateKeyPath));
            var auth = KalshiAuthentication.FromRsaPrivateKey(keyId, rsa);
            var url = string.Equals(Environment.GetEnvironmentVariable("KALSHI_USE_DEMO"), "true", StringComparison.OrdinalIgnoreCase)
                ? KalshiValues.DemoTradeWebsocketApiUrl
                : KalshiValues.TradeWebsocketApiUrl;

            using var communicator = new KalshiWebsocketCommunicator(url, auth);
            communicator.Name = "Kalshi-live-1";
            communicator.ReconnectTimeout = TimeSpan.FromSeconds(30);

            using var client = new KalshiWebsocketClient(communicator);
            SubscribeToStreams(client);

            var marketTickers = GetMarketTickers(marketTicker);
            var captureFile = Environment.GetEnvironmentVariable("KALSHI_CAPTURE_FILE");
            var captureSeconds = GetCaptureSeconds();
            using var captureWriter = string.IsNullOrWhiteSpace(captureFile) ? null : CreateCaptureWriter(captureFile);
            object captureLock = new object();
            long orderbookSubscriptionId = 0;
            using var orderbookSubscribed = new ManualResetEvent(false);

            client.Streams.RawMessageStream.Subscribe(message =>
            {
                if (captureWriter == null)
                {
                    return;
                }

                lock (captureLock)
                {
                    captureWriter.WriteLine(message);
                    captureWriter.WriteLine(";;");
                    captureWriter.Flush();
                }
            });

            client.Streams.SubscribedStream.Subscribe(x =>
            {
                if (x.Message?.Channel == KalshiChannel.OrderbookDelta)
                {
                    orderbookSubscriptionId = x.Message.Sid;
                    orderbookSubscribed.Set();
                }
            });

            communicator.ReconnectionHappened.Subscribe(info =>
            {
                Log.Information("Reconnection happened, type: {type}, resubscribing...", info.Type);
                SendLiveSubscriptions(client, marketTickers, captureWriter != null);
            });

            await communicator.Start();
            SendLiveSubscriptions(client, marketTickers, captureWriter != null);

            if (captureWriter != null)
            {
                Log.Information("Capturing raw websocket messages to {file} for {seconds} seconds", captureFile, captureSeconds);
                if (orderbookSubscribed.WaitOne(TimeSpan.FromSeconds(10)) && orderbookSubscriptionId > 0)
                {
                    client.Send(new UpdateSubscriptionRequest(100, new[] { orderbookSubscriptionId }, KalshiSubscriptionAction.GetSnapshot, new[] { marketTickers[0] }));
                }

                client.Send(new ListSubscriptionsRequest(101));
                await Task.Delay(TimeSpan.FromSeconds(captureSeconds));
                return;
            }

            ExitEvent.WaitOne();
        }

        private static void SendLiveSubscriptions(KalshiWebsocketClient client, string[] marketTickers, bool broadCapture)
        {
            client.Send(SubscribeRequest.Orderbook(1, marketTickers[0], sendInitialSnapshot: true));

            if (!broadCapture)
            {
                client.Send(new SubscribeRequest(2, new[] { KalshiChannel.Ticker }, marketTickers: marketTickers));
                client.Send(new SubscribeRequest(3, new[] { KalshiChannel.Trade }, marketTickers: marketTickers));
                return;
            }

            client.Send(new SubscribeRequest(2, new[] { KalshiChannel.Ticker }));
            client.Send(new SubscribeRequest(3, new[] { KalshiChannel.Trade }));
            client.Send(new SubscribeRequest(4, new[] { KalshiChannel.MarketLifecycleV2 }));
            client.Send(new SubscribeRequest(5, new[] { KalshiChannel.MultivariateMarketLifecycle }));
            client.Send(new SubscribeRequest(6, new[] { KalshiChannel.Fill }));
            client.Send(new SubscribeRequest(7, new[] { KalshiChannel.MarketPositions }));
            client.Send(new SubscribeRequest(8, new[] { KalshiChannel.UserOrders }));
            client.Send(new SubscribeRequest(9, new[] { KalshiChannel.OrderGroupUpdates }));
            client.Send(new SubscribeRequest(10, new[] { KalshiChannel.Communications }, shardFactor: 1, shardKey: 0));
        }

        private static string[] GetMarketTickers(string marketTicker)
        {
            var tickers = Environment.GetEnvironmentVariable("KALSHI_MARKET_TICKERS");
            if (string.IsNullOrWhiteSpace(tickers))
            {
                return new[] { marketTicker };
            }

            return tickers.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static int GetCaptureSeconds()
        {
            return int.TryParse(Environment.GetEnvironmentVariable("KALSHI_CAPTURE_SECONDS"), out var seconds) && seconds > 0
                ? seconds
                : 30;
        }

        private static StreamWriter CreateCaptureWriter(string fileName)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return new StreamWriter(fileName, append: false);
        }

        private static async Task RunReplay()
        {
            var baseDir = AppContext.BaseDirectory;
            var fileName = Path.Combine(baseDir, "data", "kalshi_public_replay.txt");
            if (!File.Exists(fileName))
            {
                fileName = Path.Combine(baseDir, "..", "..", "..", "..", "Kalshi.Client.Websocket.Tests.Integration", "data", "kalshi_public_replay.txt");
            }

            using var communicator = new KalshiFileCommunicator
            {
                FileNames = new[] { fileName },
                Delimiter = ";;"
            };
            using var client = new KalshiWebsocketClient(communicator);
            SubscribeToStreams(client);

            Log.Information("KALSHI_API_KEY_ID, KALSHI_PRIVATE_KEY_PATH, or KALSHI_MARKET_TICKER is not set; replaying fixture data.");
            await communicator.Start();
        }

        private static void SubscribeToStreams(KalshiWebsocketClient client)
        {
            client.Streams.SubscribedStream.Subscribe(x =>
                Log.Information("Subscribed sid={sid} channel={channel}", x.Message.Sid, x.Message.Channel));

            client.Streams.OrderbookSnapshotStream.Subscribe(x =>
                Log.Information("Orderbook {ticker}: yes={yes}, no={no}",
                    x.Message.MarketTicker,
                    (x.Message.Yes?.Length ?? 0) + (x.Message.YesDollars?.Length ?? 0),
                    (x.Message.No?.Length ?? 0) + (x.Message.NoDollars?.Length ?? 0)));

            client.Streams.OrderbookDeltaStream.Subscribe(x =>
                Log.Information("Delta {ticker}: {side} {delta} @ {price}",
                    x.Message.MarketTicker,
                    x.Message.Side,
                    x.Message.Delta,
                    x.Message.PriceDollars ?? x.Message.Price));

            client.Streams.TickerStream.Subscribe(x =>
                Log.Information("Ticker {ticker}: bid={bid}, ask={ask}, price={price}",
                    x.Message.MarketTicker,
                    x.Message.YesBidDollars ?? x.Message.YesBid,
                    x.Message.YesAskDollars ?? x.Message.YesAsk,
                    x.Message.PriceDollars ?? x.Message.Price));

            client.Streams.TradeStream.Subscribe(x =>
                Log.Information("Trade {ticker}: {count} @ {price}",
                    x.Message.MarketTicker,
                    x.Message.CountFp ?? x.Message.Count,
                    x.Message.YesPriceDollars ?? x.Message.YesPrice));

            client.Streams.ErrorStream.Subscribe(x =>
                Log.Warning("Error {code}: {message}", x.Message?.Code, x.Message?.Message));
        }

        private static SerilogLoggerFactory InitLogging()
        {
            var executingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? ".";
            var logPath = Path.Combine(executingDir, "logs", "verbose.log");
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .WriteTo.Console(LogEventLevel.Debug, outputTemplate:
                    "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            Log.Logger = logger;
            return new SerilogLoggerFactory(logger);
        }

        private static void CurrentDomainOnProcessExit(object sender, EventArgs eventArgs)
        {
            Log.Warning("Exiting process");
            ExitEvent.Set();
        }

        private static void DefaultOnUnloading(AssemblyLoadContext assemblyLoadContext)
        {
            Log.Warning("Unloading process");
            ExitEvent.Set();
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs consoleCancelEventArgs)
        {
            Log.Warning("Canceling process");
            consoleCancelEventArgs.Cancel = true;
            ExitEvent.Set();
        }
    }
}
