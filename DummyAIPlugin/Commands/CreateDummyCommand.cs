using CommandSystem;
using PlayerRoles;
using System;

namespace DummyAIPlugin.Commands;

/// <summary>
/// Command used to create dummies.
/// </summary>
/// <param name="dummiesManager">Reference to dummies manager.</param>
public class CreateDummyCommand(DummiesManager dummiesManager) : ICommand, IUsageProvider
{
    /// <summary>
    /// Contains command name.
    /// </summary>
    public string Command { get; } = "create";

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public string[] Aliases { get; } = ["spawn", "new", "make"];

    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get; } = "Spawns a dummy and attaches an AI controller to it.";

    /// <summary>
    /// Defines command usage prompts.
    /// </summary>
    public string[] Usage { get; } = ["%role%", "Nickname (Optional)"];

    /// <summary>
    /// Contains a reference to dummies manager.
    /// </summary>
    private readonly DummiesManager _dummiesManager = dummiesManager;

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true"/> if command executed successfully, <see langword="false"/> otherwise.</returns>
    public bool Execute(ArraySegment<string?> arguments, ICommandSender? sender, out string response)
    {
        var problem = DummyAIParentCommand.CheckPerms(sender);

        if (problem is not null)
        {
            response = problem;
            return false;
        }

        if (arguments.Count < 1)
        {
            response = $"Please specify a valid argument.\nUsage: {this.DisplayCommandUsage()}";
            return false;
        }

        if (!Enum.TryParse<RoleTypeId>(arguments.At(0), true, out var resultRole))
        {
            response = "Invalid role ID / name.";
            return false;
        }

        var isSuccess = arguments.Count > 1 ? _dummiesManager.SpawnDummy(resultRole, arguments.At(1)) : _dummiesManager.SpawnDummy(resultRole);
        response = isSuccess ? "Done! AI controlled dummy was spawned!" : "Error: Dummy couldn't be spawned!";
        return isSuccess;
    }
}
