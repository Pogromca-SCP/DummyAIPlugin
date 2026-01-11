using CommandSystem;
using NorthwoodLib.Pools;
using System;

namespace DummyAIPlugin.Commands;

/// <summary>
/// Main command provided by the plugin.
/// </summary>
public class DummyAIParentCommand : ParentCommand, IUsageProvider
{
    /// <summary>
    /// Permission required by this command and any of its subcommands.
    /// </summary>
    public const PlayerPermissions AIManagementPermissions = PlayerPermissions.ForceclassWithoutRestrictions;

    /// <summary>
    /// Checks if command sender has required permissions for AI management.
    /// </summary>
    /// <param name="sender">Sender to verify.</param>
    /// <returns>Error message if sender does not have permissions or <see langword="null"/> otherwise.</returns>
    public static string? CheckPerms(ICommandSender? sender)
    {
        if (sender is null)
        {
            return "Command sender is null.";
        }
        
        if (!sender.CheckPermission(AIManagementPermissions, out var response))
        {
            return response;
        }

        return null;
    }

    /// <summary>
    /// Contains command name.
    /// </summary>
    public override string Command { get; } = "dummyai";

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public override string[]? Aliases => null;

    /// <summary>
    /// Contains command description.
    /// </summary>
    public override string Description { get; } =
        "Provides subcommands for managing AI controlled dummmies. Displays plugin status if no sub-command is provided.";

    /// <summary>
    /// Defines command usage prompts.
    /// </summary>
    public string[] Usage { get; } = ["spawn/take/free/kill",  "%player%"];

    /// <summary>
    /// Contains a reference to dummies manager.
    /// </summary>
    private readonly DummiesManager _dummiesManager;

    /// <summary>
    /// Initializes the command.
    /// </summary>
    /// <param name="dummiesManager">Reference to dummies manager.</param>
    public DummyAIParentCommand(DummiesManager dummiesManager)
    {
        _dummiesManager = dummiesManager;
        LoadGeneratedCommands();
    }

    /// <summary>
    /// Loads subcommands.
    /// </summary>
    public override void LoadGeneratedCommands()
    {
        RegisterCommand(new CreateDummyCommand(_dummiesManager));
        RegisterCommand(new PossesDummyCommand(_dummiesManager));
        RegisterCommand(new UnpossesDummyCommand(_dummiesManager));
        RegisterCommand(new DestroyDummyCommand(_dummiesManager));
    }

    /// <summary>
    /// Executes the parent command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true"/> if command executed successfully, <see langword="false"/> otherwise.</returns>
    protected override bool ExecuteParent(ArraySegment<string?> arguments, ICommandSender? sender, out string response)
    {
        var problem = CheckPerms(sender);

        if (problem is not null)
        {
            response = problem;
            return false;
        }

        var sb = StringBuilderPool.Shared.Rent("Currently active AI dummies:\n");

        foreach (var agent in _dummiesManager.ActiveDummies)
        {
            sb.Append("- ").Append(agent.ToString()).Append('\n');
        }

        response = StringBuilderPool.Shared.ToStringReturn(sb);
        return true;
    }
}
