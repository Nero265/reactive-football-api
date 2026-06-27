# reactive-football-api

A real-time football lineup tracker built with **Rx.NET** and **Akka.NET**, featuring a lightweight HTTP server and SportMonks API integration.

## Architecture

The system consists of three independent layers:

- **Rx.NET layer** — periodically polls the SportMonks API, maps and filters data, and emits messages to Akka actors
- **Akka.NET layer** — actors maintain internal state (player lineups per fixture) and process incoming messages
- **Web server layer** — receives HTTP requests, translates them into actor messages, and returns the current state

The web server never calls the API directly. It only reads state maintained by actors, which is updated independently by the Rx pipeline.

## Tech Stack

- .NET 9
- [Rx.NET](https://github.com/dotnet/reactive) — reactive data pipeline with `NewThreadScheduler` (I/O) and `TaskPoolScheduler` (CPU)
- [Akka.NET](https://getakka.net/) — actor model with `ForkJoinDispatcher` for dedicated thread management
- [SportMonks Football API](https://docs.sportmonks.com/football) — fixture and lineup data
- `HttpListener` — lightweight HTTP server

## How It Works

1. On startup, the Rx pipeline immediately fetches lineups for all configured fixtures
2. Every 30 seconds, the pipeline re-fetches and updates actor state
3. Actors store player data (name, jersey number, birth year, country) per fixture
4. Open your browser and query any tracked fixture:
```
http://localhost:5000/lineup?fixtureId=18535517
```

## Setup

1. Clone the repository
2. Create `appsettings.json` in the `FootballLineups` project folder (see `appsettings.example.json`):

```json
{
  "SportMonks": {
    "ApiToken": "YOUR_API_TOKEN_HERE",
    "FixtureIds": [ 18535517, 18535605 ]
  }
}
```

3. Get a free API token at [sportmonks.com](https://www.sportmonks.com)
4. Run the project:
```
dotnet run
```

## Project Structure

```
FootballLineups/
├── Models/
│   ├── PlayerInfo.cs        # Player data model
│   └── FixtureData.cs       # Fixture data model
├── Actors/
│   ├── Messages.cs          # Akka message definitions
│   ├── FixtureActor.cs      # Maintains state per fixture
│   └── CoordinatorActor.cs  # Routes messages to fixture actors
├── Rx/
│   └── SportMonksService.cs # Rx pipeline — API polling and data mapping
├── Server/
│   └── WebServer.cs         # HTTP server
├── Program.cs               # Entry point
└── appsettings.json         # Configuration (not committed)
```

## Multithreading

| Component | Scheduler / Dispatcher | Purpose |
|-----------|----------------------|---------|
| API calls | `NewThreadScheduler` | Isolates slow I/O from ThreadPool |
| Data processing | `TaskPoolScheduler` | Efficient CPU-bound work on ThreadPool |
| Akka actors | `ForkJoinDispatcher` (4 threads) | Dedicated threads separate from Rx |
