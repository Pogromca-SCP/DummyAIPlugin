using CommandSystem;
using System;
using Utils;

namespace DummyAIPlugin.Commands;

/// <summary>
/// Base class for dummy commands.
/// </summary>
/// <param name="dummiesManager">Reference to dummies manager.</param>
public abstract class DummyCommandBase(DummiesManager dummiesManager) : IUsageProvider
{
    /// <summary>
    /// Defines command usage prompts.
    /// </summary>
    public string[] Usage { get; } = ["%player%"];

    /// <summary>
    /// Contains a reference to dummies manager.
    /// </summary>
    protected DummiesManager DummiesManager { get; } = dummiesManager;

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

        if ("all".Equals(arguments.At(0), StringComparison.OrdinalIgnoreCase))
        {
            response = HandleAllDummiesCommand();
            return true;
        }

        var list = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out _);

        if (list is null)
        {
            response = "An unexpected problem has occurred during PlayerId or name array processing.";
            return false;
        }

        var count = 0;

        foreach (var hub in list)
        {
            if (hub is not null && hub.IsDummy && HandleDummyCommand(hub))
            {
                ++count;
            }
        }

        response = $"Done! The request affected {count} dumm" + (count == 1 ? "y!" : "ies!");
        return true;
    }

    /// <summary>
    /// Handles the dummy command functionality for all active dummies.
    /// </summary>
    /// <returns>Response to display in sender's console.</returns>
    protected abstract string HandleAllDummiesCommand();

    /// <summary>
    /// Handles the dummy command functionality.
    /// </summary>
    /// <param name="dummy">Found dummy.</param>
    /// <returns>Whether or not the operation was successful.</returns>
    protected abstract bool HandleDummyCommand(ReferenceHub dummy);
}
