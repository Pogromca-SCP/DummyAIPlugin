using DummyAIPlugin.AI.SCP049;
using Hints;
using LabApi.Features.Console;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.Spectating;
using System.Collections.Generic;
using System.Linq;

namespace DummyAIPlugin.AI;

/// <summary>
/// Target dummy wrapper with mind, perception and other AI components attached.
/// </summary>
/// <param name="hub">Target dummy.</param>
public class DummyAgent(ReferenceHub hub)
{
    /// <summary>
    /// Clears all previously saved text messages.
    /// </summary>
    public static void ClearSavedSpectatorHints() => _spectatorHintTexts.Clear();

    /// <summary>
    /// Stores text messages previously sent to spectators.
    /// </summary>
    private static readonly Dictionary<ReferenceHub, string> _spectatorHintTexts = [];

    /// <summary>
    /// Retrieves all players spectating this dummy.
    /// </summary>
    public IEnumerable<ReferenceHub> Spectators =>
        ReferenceHub.AllHubs.Where(h => h.roleManager.CurrentRole is SpectatorRole sp && sp.SyncedSpectatedNetId == Hub.netId);

    /// <summary>
    /// Reference to wrapped dummy reference hub.
    /// </summary>
    public ReferenceHub Hub { get; } = hub;

    /// <summary>
    /// Contains current role assigned to this dummy.
    /// </summary>
    public RoleTypeId CurrentRole { get; private set; } = RoleTypeId.None;

    /// <summary>
    /// Contains AI mind currently used by this dummy.
    /// </summary>
    public Mind? Mind { get; private set; } = null;

    /// <summary>
    /// Contains AI perception currently used by this dummy.
    /// </summary>
    public Perception? Perception { get; private set; } = null;

    /// <inheritdoc />
    public override string ToString() => $"({Hub.Network_playerId.Value}) {Hub.nicknameSync.DisplayName} playing as {CurrentRole}";

    /// <summary>
    /// Performs dummy AI restart if assigned role was changed.
    /// </summary>
    public void RefreshRole()
    {
        if (Hub.roleManager.CurrentRole.RoleTypeId == CurrentRole)
        {
            return;
        }

        Activate();
    }

    /// <summary>
    /// Activates or restarts the dummy.
    /// </summary>
    public void Activate()
    {
        Disable();
        var roleObj = Hub.roleManager.CurrentRole;
        CurrentRole = roleObj.RoleTypeId;

        switch (CurrentRole)
        {
            case RoleTypeId.ClassD:
            case RoleTypeId.Scientist:
            case RoleTypeId.FacilityGuard:
            case RoleTypeId.ChaosConscript:
            case RoleTypeId.ChaosRifleman:
            case RoleTypeId.ChaosMarauder:
            case RoleTypeId.ChaosRepressor:
            case RoleTypeId.ChaosFlamingo:
            case RoleTypeId.NtfPrivate:
            case RoleTypeId.NtfSergeant:
            case RoleTypeId.NtfSpecialist:
            case RoleTypeId.NtfCaptain:
            case RoleTypeId.NtfFlamingo:
            case RoleTypeId.Flamingo:
            case RoleTypeId.AlphaFlamingo:
            case RoleTypeId.Scp0492:
            case RoleTypeId.ZombieFlamingo:
            case RoleTypeId.Scp079:
            case RoleTypeId.Scp096:
            case RoleTypeId.Scp106:
            case RoleTypeId.Scp173:
            case RoleTypeId.Scp939:
            case RoleTypeId.Scp3114:
                Logger.Warn($"AI dummy agent is possesing role '{CurrentRole}' which is not supported yet.");
                break;
            case RoleTypeId.Scp049:
                if (roleObj is Scp049Role doc)
                {
                    Perception = new(Hub);
                    Mind = new SCP049Mind(Perception, doc, Hub);
                }
                else
                {
                    Logger.Warn($"AI dummy agent failed to posses role '{CurrentRole}' due to unexpected role class.");
                }

                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Disables the dummy.
    /// </summary>
    public void Disable()
    {
        Mind?.Terminate();
        Mind = null;
        Perception?.Terminate();
        Perception = null;
        CurrentRole = RoleTypeId.None;
    }

    /// <summary>
    /// Performs AI update.
    /// </summary>
    /// <param name="showActionPlan">Whether or not the agent should display current action plan to spectators.</param>
    public void Update(bool showActionPlan)
    {
        Perception?.Update();

        if (Mind is null)
        {
            return;
        }

        Mind.Update();
        
        if (showActionPlan)
        {
            DisplayActionPlan();
        }
    }

    /// <summary>
    /// Sends an action plan visualization text message to all spectators.
    /// </summary>
    private void DisplayActionPlan()
    {
        var sb = StringBuilderPool.Shared.Rent("<size=14><align=left>");
        Mind!.DisplayActionPlan(sb);
        SendTextHintToSpectators(StringBuilderPool.Shared.ToStringReturn(sb), 10.0f);
    }

    /// <summary>
    /// Sends a text message to display for all spectators.
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <param name="duration">Duration of displayed message.</param>
    private void SendTextHintToSpectators(string message, float duration)
    {
        HintParameter[] param = [new StringHintParameter(string.Empty)];

        foreach (var spectator in Spectators)
        {
            if (!_spectatorHintTexts.TryGetValue(spectator, out var prevHintText))
            {
                prevHintText = string.Empty;
                _spectatorHintTexts.Add(spectator, prevHintText);
            }

            if (prevHintText == message)
            {
                continue;
            }

            spectator.hints.Show(new TextHint(message, param, null, duration));
            _spectatorHintTexts[spectator] = message;
        }
    }
}
