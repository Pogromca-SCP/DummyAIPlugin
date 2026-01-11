using DummyAIPlugin.AI;
using LabApi.Features.Console;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;

namespace DummyAIPlugin;

/// <summary>
/// Adds and removes AI controlled dummies and keeps track of active AI dummies.
/// </summary>
/// <param name="plugin">Reference to plugin object for access to config.</param>
public class DummiesManager(DummyAIPlugin plugin)
{
    /// <summary>
    /// Retrieves all active AI dummies.
    /// </summary>
    public IEnumerable<DummyAgent> ActiveDummies => _dummies.Values;

    /// <summary>
    /// Contains reference to plugin object for access to config object.
    /// </summary>
    private readonly DummyAIPlugin _plugin = plugin;

    /// <summary>
    /// Used to keep track of all active AI dummies.
    /// </summary>
    private readonly Dictionary<ReferenceHub, DummyAgent> _dummies = [];

    /// <summary>
    /// Coroutine handle for updates.
    /// </summary>
    private CoroutineHandle _handle;

    /// <summary>
    /// Initializes AI dummies.
    /// </summary>
    public void Init()
    {
        Timing.KillCoroutines(_handle);
        _handle = Timing.RunCoroutine(UpdateDummies());
    }

    /// <summary>
    /// Terminates AI dummies.
    /// </summary>
    public void Terminate()
    {
        Timing.KillCoroutines(_handle);
        UnpossesAllDummies();
    }

    /// <summary>
    /// Performs necessary operations after AI dummy role change.
    /// </summary>
    /// <param name="targetDummy">Dummy which changed the role.</param>
    public void HandleRoleChange(ReferenceHub? targetDummy)
    {
        if (targetDummy is null || !targetDummy.IsDummy || !_dummies.TryGetValue(targetDummy, out var agent))
        {
            return;
        }

        agent.RefreshRole();
    }

    /// <summary>
    /// Spawns a new dummy and attaches an AI controller to it.
    /// </summary>
    /// <param name="role">Role type applied to newly spawned dummy.</param>
    /// <param name="nickname">Optional nickname applied to newly spawned dummy.</param>
    /// <returns><see langword="true"/> if dummy spawned successfully, <see langword="false"/> otherwise.</returns>
    public bool SpawnDummy(RoleTypeId role, string? nickname = null)
    {
        var dummy = nickname is null ? DummyUtils.SpawnDummy() : DummyUtils.SpawnDummy(nickname);

        if (dummy is null)
        {
            Logger.Error("Could not spawn a new dummy instance.");
            return false;
        }

        dummy.roleManager.ServerSetRole(role, RoleChangeReason.LateJoin);
        Posses(dummy);
        Logger.Info($"New AI dummy with role ({role}) spawned successfully!");
        return true;
    }

    /// <summary>
    /// Attaches an AI controller to a non-controlled dummy.
    /// </summary>
    /// <param name="targetDummy">Dummy to posses.</param>
    /// <returns>Result of possessing operation.</returns>
    public PossessionResult PossesDummy(ReferenceHub? targetDummy)
    {
        if (targetDummy is null || !targetDummy.IsDummy)
        {
            return PossessionResult.InvalidTarget;
        }

        if (_dummies.ContainsKey(targetDummy))
        {
            return PossessionResult.AlreadyTaken;
        }

        Posses(targetDummy);
        return PossessionResult.Success;
    }

    /// <summary>
    /// Attaches AI controllers to all non-controlled dummies.
    /// </summary>
    /// <returns>Amount of affected dummies.</returns>
    public int PossesAllDummies()
    {
        var affectedDummies = 0;

        foreach (var player in Player.GetAll(PlayerSearchFlags.DummyNpcs).Select(p => p?.ReferenceHub))
        {
            if (player is not null && player.IsDummy && !_dummies.ContainsKey(player))
            {
                Posses(player);
                ++affectedDummies;
            }
        }

        return affectedDummies;
    }

    /// <summary>
    /// Removes an AI controller from controlled dummy.
    /// </summary>
    /// <param name="targetDummy">Dummy to unposses.</param>
    /// <returns><see langword="true"/> if unpossessed successfully, <see langword="false"/> otherwise.</returns>
    public bool UnpossesDummy(ReferenceHub? targetDummy)
    {
        if (targetDummy is null || !targetDummy.IsDummy)
        {
            return false;
        }

        Unposses(targetDummy);
        return true;
    }

    /// <summary>
    /// Removes AI controllers from all controlled dummies.
    /// </summary>
    /// <returns>Amount of affected dummies.</returns>
    public int UnpossesAllDummies()
    {
        var affectedDummies = 0;

        foreach (var player in Player.GetAll(PlayerSearchFlags.DummyNpcs).Select(p => p?.ReferenceHub))
        {
            if (player is not null && player.IsDummy && _dummies.ContainsKey(player))
            {
                Unposses(player);
                ++affectedDummies;
            }
        }

        DummyAgent.ClearSavedSpectatorHints();
        return affectedDummies;
    }

    /// <summary>
    /// Removes an AI controller and destroys controlled dummy.
    /// </summary>
    /// <param name="targetDummy">Dummy to destroy.</param>
    /// <returns><see langword="true"/> if dummy was destroyed successfully, <see langword="false"/> otherwise.</returns>
    public bool DestroyDummy(ReferenceHub? targetDummy)
    {
        if (targetDummy is null || !targetDummy.IsDummy)
        {
            return false;
        }

        Destroy(targetDummy);
        return true;
    }

    /// <summary>
    /// Removes all AI controller and destroys controlled dummies.
    /// </summary>
    /// <returns>Amount of affected dummies.</returns>
    public int DestroyAllDummies()
    {
        var affectedDummies = 0;

        foreach (var player in Player.GetAll(PlayerSearchFlags.DummyNpcs).Select(p => p?.ReferenceHub))
        {
            if (player is not null && player.IsDummy && _dummies.ContainsKey(player))
            {
                Destroy(player);
                ++affectedDummies;
            }
        }

        DummyAgent.ClearSavedSpectatorHints();
        return affectedDummies;
    }

    /// <summary>
    /// Handles shared possession logic.
    /// </summary>
    /// <param name="target">Dummy to posses.</param>
    private void Posses(ReferenceHub target)
    {
        var agent = new DummyAgent(target);
        agent.Activate();
        _dummies.Add(target, agent);
    }

    /// <summary>
    /// Handles shared unpossession logic.
    /// </summary>
    /// <param name="target">Dummy to unposses.</param>
    private void Unposses(ReferenceHub target)
    {
        if (!_dummies.TryGetValue(target, out var agent))
        {
            return;
        }

        agent.Disable();
        _dummies.Remove(target);
    }

    /// <summary>
    /// Handles shared destroy logic.
    /// </summary>
    /// <param name="target">Dummy to destroy.</param>
    private void Destroy(ReferenceHub target)
    {
        Unposses(target);
        NetworkServer.Destroy(target.gameObject);
        Logger.Info("A dummy was destroyed.");
    }

    /// <summary>
    /// Performs updates for AI dummies.
    /// </summary>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator<float> UpdateDummies()
    {
        while (true)
        {
            var showActionPlan = _plugin.Config?.EnableMindVisualizations ?? false;

            foreach (var agent in _dummies.Values)
            {
                agent.Update(showActionPlan);
            }

            yield return Timing.WaitForOneFrame;
        }
    }
}
