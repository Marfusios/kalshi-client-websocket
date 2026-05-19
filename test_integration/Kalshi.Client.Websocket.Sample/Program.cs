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

            communicator.ReconnectionHappened.Subscribe(info =>
            {
                Log.Information("Reconnection happened, type: {type}, resubscribing...", info.Type);
                client.Send(SubscribeRequest.Orderbook(1, marketTicker, sendInitialSnapshot: true));
                client.Send(SubscribeRequest.Ticker(2, marketTicker));
                client.Send(SubscribeRequest.Trades(3, marketTicker));
            });

            await communicator.Start();
            client.Send(SubscribeRequest.Orderbook(1, marketTicker, sendInitialSnapshot: true));
            client.Send(SubscribeRequest.Ticker(2, marketTicker));
            client.Send(SubscribeRequest.Trades(3, marketTicker));

            ExitEvent.WaitOne();
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
                    x.Message.Count,
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
