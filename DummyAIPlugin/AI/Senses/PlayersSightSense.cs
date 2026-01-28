using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DummyAIPlugin.AI.Senses;

/// <summary>
/// A sight sense specialized in players detection.
/// </summary>
public class PlayersSightSense : SightSense<ReferenceHub>
{
    /// <summary>
    /// Contains detected enemies.
    /// </summary>
    public IEnumerable<ReferenceHub> EnemiesWithinSight { get; }

    /// <summary>
    /// Contains detected teammates.
    /// </summary>
    public IEnumerable<ReferenceHub> TeammatesWithinSight { get; }

    /// <inheritdoc />
    protected override LayerMask LayerMask { get; } = LayerMask.GetMask(Perception.HitboxLayer);

    /// <summary>
    /// Contains the faction target dummy belongs to.
    /// </summary>
    private readonly Faction _faction;

    /// <summary>
    /// Initializes new players sight sense instance.
    /// </summary>
    /// <param name="dummy">Target dummy's reference hub.</param>
    public PlayersSightSense(ReferenceHub dummy) : base(dummy)
    {
        _faction = dummy.GetFaction();
        EnemiesWithinSight = ComponentsWithinSight.Where(h => h.GetFaction() != _faction && h.GetFaction() != Faction.Unclassified);
        TeammatesWithinSight = ComponentsWithinSight.Where(o => o.GetFaction() == _faction);
    }
}
