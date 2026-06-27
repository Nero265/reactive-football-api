using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballLineups.Actors;

public class CoordinatorActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Dictionary<int, IActorRef> _fixtureActors = new();

    public CoordinatorActor()
    {
        Receive<UpdateLineup>(msg =>
        {
            var actor = GetOrCreateFixtureActor(msg.FixtureId);
            actor.Tell(msg);
        });

        Receive<GetLineup>(msg =>
        {
            if (_fixtureActors.TryGetValue(msg.FixtureId, out var fixtureActor))
            {
                fixtureActor.Forward(msg);
            }
            else
            {
                Sender.Tell(new LineupResponse(
                    fixtureId: msg.FixtureId,
                    name: "",
                    players: new(),
                    found: false
                ));
            }
        });
    }

    protected override void PreStart()
        => _log.Info("CoordinatorActor started");

    protected override void PostStop()
        => _log.Info("CoordinatorActor stopiran");

    private IActorRef GetOrCreateFixtureActor(int fixtureId)
    {
        if (!_fixtureActors.TryGetValue(fixtureId, out var actor))
        {
            _log.Info($"Kreiram FixtureActor za utakmicu {fixtureId}");
            actor = Context.ActorOf(FixtureActor.Props(), $"fixture-{fixtureId}");
            _fixtureActors[fixtureId] = actor;
        }
        return actor;
    }

    public static Props Props()
        => Akka.Actor.Props.Create<CoordinatorActor>();
}