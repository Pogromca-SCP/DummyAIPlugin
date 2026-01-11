using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using System.Linq;

namespace DummyAIPlugin.Events;

/// <summary>
/// Custom event handler for custom events.
/// </summary>
/// <param name="plugin">Reference to plugin object for access to config.</param>
/// <param name="dummiesManager">Reference to dummies manager.</param>
public class AIEventsHandler(DummyAIPlugin plugin, DummiesManager dummiesManager) : CustomEventsHandler
{
    /// <summary>
    /// Attempts to spawn a dummy AI if the selected role is not present.
    /// </summary>
    /// <param name="manager">Manager used to spawn AI dummy.</param>
    /// <param name="role">Role to apply for newly spawned dummy.</param>
    private static void SpawnIfNotPresent(DummiesManager manager, RoleTypeId role)
    {
        if (!Player.GetAll().Any(p => p.Role == role))
        {
            manager.SpawnDummy(role, role.ToString());
        }
    }

    /// <summary>
    /// Contains reference to plugin object for access to config object.
    /// </summary>
    private readonly DummyAIPlugin _plugin = plugin;

    /// <summary>
    /// Contains a reference to dummies manager.
    /// </summary>
    private readonly DummiesManager _dummiesManager = dummiesManager;

    /// <inheritdoc />
    public override void OnServerRoundStarted()
    {
        var config = _plugin.Config;

        if (config is null)
        {
            Logger.Warn("Cannot spawn AI dummies on round start because config object is missing.");
            return;
        }

        Timing.CallDelayed(1.0f, () =>
        {
            if (config.SpawnScp049IfNotPresentOnStart)
            {
                SpawnIfNotPresent(_dummiesManager, RoleTypeId.Scp049);
            }
        });
    }

    /// <inheritdoc />
    public override void OnServerRoundRestarted() => _dummiesManager.UnpossesAllDummies();

    /// <inheritdoc />
    public override void OnPlayerLeft(PlayerLeftEventArgs ev) => _dummiesManager.UnpossesDummy(ev.Player.ReferenceHub);

    /// <inheritdoc />
    public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev) => _dummiesManager.HandleRoleChange(ev.Player.ReferenceHub);
}
