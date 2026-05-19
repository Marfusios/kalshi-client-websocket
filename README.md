![Logo](https://raw.githubusercontent.com/Marfusios/kalshi-client-websocket/master/kalshi-logo.png)
# Kalshi websocket API client

[![NuGet version](https://img.shields.io/nuget/v/Kalshi.Client.Websocket?style=flat-square)](https://www.nuget.org/packages/Kalshi.Client.Websocket)
[![Nuget downloads](https://img.shields.io/nuget/dt/Kalshi.Client.Websocket?style=flat-square)](https://www.nuget.org/packages/Kalshi.Client.Websocket)
[![CI build](https://img.shields.io/github/check-runs/marfusios/kalshi-client-websocket/master?style=flat-square&label=build)](https://github.com/Marfusios/kalshi-client-websocket/actions/workflows/dotnet-core.yml)

This is a C# implementation of the Kalshi websocket API v2:

- [Websocket overview](https://docs.kalshi.com/websockets)
- [Websocket connection](https://docs.kalshi.com/websockets/websocket-connection)
- [Websocket channels](https://docs.kalshi.com/websockets/orderbook-updates)
- [API environments](https://docs.kalshi.com/getting_started/api_environments)
- [API keys and signing](https://docs.kalshi.com/getting_started/api_keys)

Kalshi requires authenticated websocket connection headers for public and private websocket channels. The client defaults to Kalshi's dedicated Trade API websocket hosts:

- production: `wss://external-api-ws.kalshi.com/trade-api/ws/v2`
- demo: `wss://external-api-ws.demo.kalshi.co/trade-api/ws/v2`

The older shared hosts are exposed as `KalshiValues.SharedTradeWebsocketApiUrl` and `KalshiValues.SharedDemoTradeWebsocketApiUrl`.

### License

Apache License 2.0

### Features

- installation via NuGet ([Kalshi.Client.Websocket](https://www.nuget.org/packages/Kalshi.Client.Websocket))
- targets `netstandard2.0`, `netstandard2.1`, `net6.0`, `net7.0`, `net8.0`, `net9.0`, `net10.0`
- built on [Websocket.Client 5.5.0](https://www.nuget.org/packages/Websocket.Client/5.5.0)
- RSA-PSS authentication header helper for Kalshi websocket handshakes
- typed subscribe, unsubscribe, update-subscription, and list-subscriptions requests
- typed streams for public market data and private account channels
- decimal/integer/string-enum parsing for websocket values
- file communicator for replay/backtesting and data collection pipelines
- unit and integration tests, including replay data seeded from public Kalshi REST market/orderbook data

### Usage

Connect and subscribe to public market data:

```csharp
using var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText("kalshi-private-key.pem"));

var auth = KalshiAuthentication.FromRsaPrivateKey("api-key-id", rsa);
using var communicator = new KalshiWebsocketCommunicator(KalshiValues.TradeWebsocketApiUrl, auth);
using var client = new KalshiWebsocketClient(communicator);

client.Streams.OrderbookSnapshotStream.Subscribe(snapshot =>
{
    Console.WriteLine($"Book {snapshot.Message.MarketTicker}: yes={snapshot.Message.Yes?.Length ?? 0}, no={snapshot.Message.No?.Length ?? 0}");
});

client.Streams.TickerStream.Subscribe(ticker =>
{
    Console.WriteLine($"Ticker {ticker.Message.MarketTicker}: bid={ticker.Message.YesBid}, ask={ticker.Message.YesAsk}");
});

await communicator.Start();

client.Send(SubscribeRequest.Orderbook(1, "KXEXAMPLE-YES", sendInitialSnapshot: true));
client.Send(SubscribeRequest.Ticker(2, "KXEXAMPLE-YES"));
client.Send(SubscribeRequest.Trades(3, "KXEXAMPLE-YES"));
```

Update and unsubscribe:

```csharp
client.Send(new UpdateSubscriptionRequest(
    id: 4,
    subscriptionId: 2,
    action: KalshiSubscriptionAction.AddMarkets,
    marketTickers: new[] { "KXEXAMPLE-NO" }));

client.Send(new UpdateSubscriptionRequest(
    id: 5,
    subscriptionIds: new[] { 2L },
    action: KalshiSubscriptionAction.GetSnapshot,
    marketTickers: new[] { "KXEXAMPLE-YES" }));

client.Send(new ListSubscriptionsRequest(6));
client.Send(new UnsubscribeRequest(7, new[] { 2L, 3L }));
```

Run the sample:

```powershell
$env:KALSHI_API_KEY_ID = "your-key-id"
$env:KALSHI_PRIVATE_KEY_PATH = "C:\keys\kalshi-private-key.pem"
$env:KALSHI_MARKET_TICKER = "KXEXAMPLE-YES"
dotnet run --project test_integration/Kalshi.Client.Websocket.Sample
```

If the environment variables are not set, the sample replays the included public fixture.

More examples:

- integration tests ([link](test_integration/Kalshi.Client.Websocket.Tests.Integration))
- console sample ([link](test_integration/Kalshi.Client.Websocket.Sample/Program.cs))

### API Coverage

#### Commands

| Command | Covered |
|---------|:-------:|
| `subscribe` | yes |
| `unsubscribe` | yes |
| `update_subscription` | yes |
| `list_subscriptions` | yes |

#### Public market data

| Type | Covered |
|------|:-------:|
| `orderbook_snapshot` | yes |
| `orderbook_delta` | yes |
| `ticker` | yes |
| `trade` | yes |
| `market_lifecycle_v2` | yes |
| `event_lifecycle` | yes |
| `multivariate_market_lifecycle` | yes |
| `multivariate_lookup` | yes |

#### Private/account data

| Type | Covered |
|------|:-------:|
| `fill` | yes |
| `market_position` | yes |
| `user_order` | yes |
| `order_group_updates` | yes |
| `rfq_created` | yes |
| `rfq_deleted` | yes |
| `quote_created` | yes |
| `quote_accepted` | yes |
| `quote_executed` | yes |

### Reconnecting

There is a built-in reconnection which invokes after 1 minute (default) of not receiving any messages from the server. It is possible to configure that timeout via `communicator.ReconnectTimeout`. There is also a stream `ReconnectionHappened` which sends information about the reconnection type.

You need to resubscribe after reconnection happens. Subscribe to `communicator.ReconnectionHappened` and send the same subscription request again from that handler.

### Backtesting

The library is prepared for backtesting and data collection. The dependency between `Client` and `Communicator` is via abstraction `IKalshiCommunicator`. There are two communicator implementations:

- `KalshiWebsocketCommunicator` - realtime communication with Kalshi websocket API v2
- `KalshiFileCommunicator` - simulated communication, raw data are loaded from files and streamed

Usage:

```csharp
var communicator = new KalshiFileCommunicator
{
    FileNames = new[] { "data/kalshi_public_replay.txt" },
    Delimiter = ";;"
};

using var client = new KalshiWebsocketClient(communicator);
client.Streams.OrderbookSnapshotStream.Subscribe(snapshot =>
{
    // do something with snapshots
});

await communicator.Start();
```

### Multi-threading

Observables from Reactive Extensions are single threaded by default. Your code inside subscriptions is called synchronously as soon as a message comes from the websocket API. If your subscription handler takes a long time, it blocks the receiving method, buffers messages, and can eventually lose messages.

Handle messages on another thread when your processing is expensive:

```csharp
client
    .Streams
    .OrderbookDeltaStream
    .ObserveOn(TaskPoolScheduler.Default)
    .Subscribe(delta => { /* process */ });
```

If you need parallel processing while preserving order for a stream, use synchronization:

```csharp
private static readonly object Gate = new object();

client
    .Streams
    .OrderbookDeltaStream
    .ObserveOn(TaskPoolScheduler.Default)
    .Synchronize(Gate)
    .Subscribe(delta => { /* process */ });
```

**Pull Requests are welcome!**
