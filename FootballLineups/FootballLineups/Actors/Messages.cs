using FootballLineups.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballLineups.Actors
{
    // Rx salje ovo CoordinatorActor-u kada povuce podatke sa API-ja
    public sealed class UpdateLineup
    {
        public int FixtureId { get; }
        public string Name { get; }
        public List<PlayerInfo> Players { get; }

        public UpdateLineup(int fixtureId, string name, List<PlayerInfo> players)
        {
            FixtureId = fixtureId;
            Name = name;
            Players = players;
        }
    }

    //web server  salje ovo kada korisnik trazi podatke za utkamicu
    public sealed class GetLineup
    {
        public int FixtureId { get; }
        public GetLineup(int fixtureId) => FixtureId = fixtureId;
    }

    // Aktor salje ovo nazad web serveru kao odgovor
    public sealed class LineupResponse
    {
        public int FixtureId { get; }
        public string Name { get; }
        public List<PlayerInfo> Players { get; }
        public bool Found { get; }

        public LineupResponse(int fixtureId, string name, List<PlayerInfo> players, bool found)
        {
            FixtureId = fixtureId;
            Name = name;
            Players = players;
            Found = found;
        }
    }
}
