using CommandSystem;

namespace DummyAIPlugin.Commands;

/// <summary>
/// Command used to unposses dummies.
/// </summary>
/// <param name="dummiesManager">Reference to dummies manager.</param>
public class UnpossesDummyCommand(DummiesManager dummiesManager) : DummyCommandBase(dummiesManager), ICommand
{
    /// <summary>
    /// Contains command name.
    /// </summary>
    public string Command { get; } = "unposses";

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public string[] Aliases { get; } = ["free"];

    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get; } = "Removes an AI controller from controlled dummy.";
    
    /// <inheritdoc />
    protected override string HandleAllDummiesCommand()
    {
        var count = DummiesManager.UnpossesAllDummies();
        return $"Done! Found and unpossessed {count} dummies!";
    }

    /// <inheritdoc />
    protected override bool HandleDummyCommand(ReferenceHub dummy) => DummiesManager.UnpossesDummy(dummy);
}
