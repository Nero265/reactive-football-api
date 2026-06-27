using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballLineups.Actors
{
    public class FixtureActor : ReceiveActor    
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private Models.FixtureData _state = new();

        public FixtureActor()
        {
            Receive<UpdateLineup>(msg =>
            {
                _state.FixtureId = msg.FixtureId;
                _state.Name = msg.Name;
                _state.Players = msg.Players;
                _log.Info($"Azurirano stanje za utakmicu {msg.Name} " +
                          $"— {msg.Players.Count} igraca");
            });

            Receive<GetLineup>(msg =>
            {
                Sender.Tell(new LineupResponse(
                    fixtureId: _state.FixtureId,
                    name: _state.Name,
                    players: _state.Players,
                    found: _state.Players.Count > 0
                ));
            });
        }

        protected override void PreStart()
            => _log.Info($"FixtureActor startet za utakmicu {Self.Path.Name}");

        protected override void PostStop()
            => _log.Info($"FixtureActor stopiran za utakmicu {Self.Path.Name}");

        public static Props Props()
            => Akka.Actor.Props.Create<FixtureActor>();
    }
}
