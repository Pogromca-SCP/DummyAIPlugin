using CommandSystem;

namespace DummyAIPlugin.Commands;

/// <summary>
/// Command used to destroy dummies.
/// </summary>
/// <param name="dummiesManager">Reference to dummies manager.</param>
public class DestroyDummyCommand(DummiesManager dummiesManager) : DummyCommandBase(dummiesManager), ICommand
{
    /// <summary>
    /// Contains command name.
    /// </summary>
    public string Command { get; } = "destroy";

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public string[] Aliases { get; } = ["kill", "remove", "delete"];

    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get; } = "Removes an AI controller and destroys controlled dummy.";
    
    /// <inheritdoc />
    protected override string HandleAllDummiesCommand()
    {
        var count = DummiesManager.DestroyAllDummies();
        return $"Done! Found and destroyed {count} dummies!";
    }

    /// <inheritdoc />
    protected override bool HandleDummyCommand(ReferenceHub dummy) => DummiesManager.DestroyDummy(dummy);
}
