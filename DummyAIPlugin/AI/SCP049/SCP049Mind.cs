using DummyAIPlugin.AI.FirstPerson;
using DummyAIPlugin.AI.Senses;
using PlayerRoles.PlayableScps.Scp049;
using System.Linq;

namespace DummyAIPlugin.AI.SCP049;

/// <summary>
/// AI mind for SCP-049 dummies.
/// </summary>
public class SCP049Mind : Mind
{
    /// <summary>
    /// Sight sense for players detection.
    /// </summary>
    public PlayersSightSense PlayersSense { get; }

    /// <summary>
    /// Creates new mind instance.
    /// </summary>
    /// <param name="perception">Perception component used for senses updating.</param>
    /// <param name="role">SCP-049 role reference.</param>
    /// <param name="hub">Target dummy's reference hub.</param>
    public SCP049Mind(Perception perception, Scp049Role role, ReferenceHub hub)
    {
        PlayersSense = new(hub);
        perception.Senses.Add(PlayersSense);

        var factory = new BeliefFactory(Beliefs);
        const string TargetDetected = "TargetDetected";
        const string NoTargets = "NoTargets";

        factory.AddPredicateBelief(TargetDetected, PlayersSense, s => s.ComponentsWithinSight.Any());
        factory.AddPredicateBelief(NoTargets, PlayersSense, s => !s.ComponentsWithinSight.Any());

        Actions.Add(new Action.Builder("Idle", new IdleStrategy(0.01f))
            .AddEffect(Beliefs[TargetDetected])
            .Build());

        Actions.Add(new Action.Builder("Jump", new JumpStrategy(role.FpcModule))
            .AddPrecondition(Beliefs[TargetDetected])
            .AddEffect(Beliefs[NoTargets])
            .Build());

        Goals.Add(new Goal.Builder("IndicateDetection")
            .AddDesiredEffect(Beliefs[NoTargets])
            .Build());

        Goals.Add(new Goal.Builder("DoNothing")
            .AddDesiredEffect(Beliefs[TargetDetected])
            .Build());
    }
}
