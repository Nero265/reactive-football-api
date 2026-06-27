using Akka.Actor;
using FootballLineups.Actors;
using FootballLineups.Rx;
using FootballLineups.Server;
using Microsoft.Extensions.Configuration;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Football Lineups - Rx.NET + Akka.NET ===\n");

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var apiToken = config["SportMonks:ApiToken"];

if (string.IsNullOrEmpty(apiToken))
{
    Console.WriteLine("[Main] Greska: API token nije pronadjen u appsetings.json!");
    return;
}

var fixtureIds = config.GetSection("SportMonks:FixtureIds")
                       .GetChildren()
                       .Select(x => int.Parse(x.Value!))
                       .ToList();

if (fixtureIds.Count == 0)
{
    Console.WriteLine("[Main] Greska: nisu pronadjeni fixture ID-evi u appsettings.json!");
    return;
}

Console.WriteLine($"[Main] Pracenje {fixtureIds.Count} utakmica: {string.Join(", ", fixtureIds)}");

using var system = ActorSystem.Create("football-system");
var coordinator = system.ActorOf(CoordinatorActor.Props(), "coordinator");

Console.WriteLine("[Main] Akka sistem pokrenut.");

var service = new SportMonksService(apiToken, coordinator);
var subscription = service
    .GetFixturesWithLineups(fixtureIds)
    .Subscribe(
        onNext: f => Console.WriteLine($"[Rx] Emitovana utakmica: {f.Name}"),
        onError: ex => Console.WriteLine($"[Rx] Greska: {ex.Message}"),
        onCompleted: () => Console.WriteLine("[Rx] Pipeline zavrsen.")
    );

Console.WriteLine("[Main] Rx pipeline pokrenut.");

// Pokretanje web servera
var server = new WebServer("http://localhost:5000/", coordinator);

Console.WriteLine("[Main] Pokreni browser i idi na:");
Console.WriteLine("http://localhost:5000/lineup?fixtureId=18535517");
Console.WriteLine("\nPritisni Enter za gašenje...\n");

// Pokretanje servera i cekanje na Enter istovremeno
await Task.WhenAny(
    server.StartAsync(),
    Task.Run(() => Console.ReadLine())
);

// Cleanup
subscription.Dispose();
Console.WriteLine("[Main] Ugaseno.");