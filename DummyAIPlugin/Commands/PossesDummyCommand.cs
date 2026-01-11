using CommandSystem;

namespace DummyAIPlugin.Commands;

/// <summary>
/// Command used to posses dummies.
/// </summary>
/// <param name="dummiesManager">Reference to dummies manager.</param>
public class PossesDummyCommand(DummiesManager dummiesManager) : DummyCommandBase(dummiesManager), ICommand
{
    /// <summary>
    /// Contains command name.
    /// </summary>
    public string Command { get; } = "posses";

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public string[] Aliases { get; } = ["take"];

    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get; } = "Attaches an AI controller to a non-controlled dummy.";
    
    /// <inheritdoc />
    protected override string HandleAllDummiesCommand()
    {
        var count = DummiesManager.PossesAllDummies();
        return $"Done! Found and possessed {count} dummies!";
    }

    /// <inheritdoc />
    protected override bool HandleDummyCommand(ReferenceHub dummy) => DummiesManager.PossesDummy(dummy) == PossessionResult.Success;
}
