using Akka.Actor;
using FootballLineups.Actors;
using FootballLineups.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballLineups.Rx
{
    public class SportMonksService
    {
        private readonly string _apiToken;
        private readonly IActorRef _coordinator;
        private readonly HttpClient _httpClient = new();

        public SportMonksService(string apiToken, IActorRef coordinator)
        {
            _apiToken = apiToken;
            _coordinator = coordinator;
        }

        public IObservable<FixtureData> GetFixturesWithLineups(List<int> fixtureIds)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(30))
                .StartWith(0)
                .SelectMany(_ => fixtureIds.ToObservable())
                // NewThreadScheduler — svaki API poziv ide na novu nit
                // idealno za spore I/O operacije koje ne troše CPU
                .SelectMany(id => FetchFixture(id)
                    .SubscribeOn(NewThreadScheduler.Default))
                .Where(f => f.Players.Count > 0)
                .Do(f => _coordinator.Tell(new UpdateLineup(f.FixtureId, f.Name, f.Players)))
                // TaskPoolScheduler — obrada rezultata ide na ThreadPool
                // idealno za kratkotrajne CPU operacije
                .ObserveOn(TaskPoolScheduler.Default);
        }

        private IObservable<FixtureData> FetchFixture(int fixtureId)
        {
            return Observable.FromAsync(async () =>
            {
                var url = $"https://api.sportmonks.com/v3/football/fixtures/{fixtureId}" +
                          $"?api_token={_apiToken}&include=lineups.player.country";

                Console.WriteLine($"[Rx] Povlacim podatke za utakmicu {fixtureId}...");

                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);
                var data = json["data"];

                if (data == null) return new FixtureData { FixtureId = fixtureId };

                var fixture = new FixtureData
                {
                    FixtureId = fixtureId,
                    Name = data["name"]?.ToString() ?? ""
                };

                var lineups = data["lineups"] as JArray;
                if (lineups == null) return fixture;

                fixture.Players = lineups.Select(p => new PlayerInfo
                {
                    PlayerId = p["player_id"]?.Value<int>() ?? 0,
                    PlayerName = p["player_name"]?.ToString() ?? "",
                    JerseyNumber = p["jersey_number"]?.Value<int>() ?? 0,
                    TeamId = p["team_id"]?.Value<int>() ?? 0,
                    FirstName = p["player"]?["firstname"]?.ToString() ?? "",
                    LastName = p["player"]?["lastname"]?.ToString() ?? "",
                    BirthYear = p["player"]?["date_of_birth"] != null
                        ? DateTime.TryParse(p["player"]?["date_of_birth"]?.ToString(), out var dob)
                            ? dob.Year
                            : (int?)null
                        : null,
                    Country = p["player"]?["country"]?["name"]?.ToString() ?? ""

                }).ToList();

                Console.WriteLine($"[Rx] Utakmica '{fixture.Name}' - {fixture.Players.Count} igraca ucitano.");

                return fixture;
            });
        }
    }
}
