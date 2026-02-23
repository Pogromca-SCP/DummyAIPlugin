using DummyAIPlugin.Commands;
using DummyAIPlugin.Events;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using System;

namespace DummyAIPlugin;

/// <summary>
/// Defines plugin functionality.
/// </summary>
public class DummyAIPlugin : Plugin<Config>
{
    /// <summary>
    /// Contains plugin name to display.
    /// </summary>
    public const string PluginName = "DummyAIPlugin";

    /// <summary>
    /// Contains current plugin version.
    /// </summary>
    public const string PluginVersion = "0.1.0";

    /// <summary>
    /// Contains plugin description.
    /// </summary>
    public const string PluginDescription = "AI controlled dummy entities plugin.";

    /// <summary>
    /// Contains plugin author.
    /// </summary>
    public const string PluginAuthor = "Adam Szerszenowicz and Grzegorz Ptaszyński";

    /// <inheritdoc />
    public override string Name { get; } = PluginName;

    /// <inheritdoc />
    public override string Description { get; } = PluginDescription;

    /// <inheritdoc />
    public override string Author { get; } = PluginAuthor;

    /// <inheritdoc />
    public override Version Version { get; } = new(PluginVersion);

    /// <inheritdoc />
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    /// <summary>
    /// Contains a reference to dummies manager.
    /// </summary>
    private DummiesManager? _dummiesManager = null;

    /// <summary>
    /// Contains a reference to events handler.
    /// </summary>
    private AIEventsHandler? _eventsHandler = null;

    /// <summary>
    /// Contains a reference to main plugin command.
    /// </summary>
    private DummyAIParentCommand? _parentCommand = null;

    /// <inheritdoc />
    public override void Enable()
    {
        if (_dummiesManager is not null)
        {
            return;
        }

        Logger.Info("Enabling DummyAI...");
        _dummiesManager = new(this);
        _dummiesManager.Init();
        _eventsHandler = new(this, _dummiesManager);
        CustomHandlersManager.RegisterEventsHandler(_eventsHandler);
        _parentCommand = new(_dummiesManager);
        Server.RemoteAdminCommandHandler.RegisterCommand(_parentCommand);
        Server.GameConsoleCommandHandler.RegisterCommand(_parentCommand);
        Logger.Info("DummyAI is enabled.");
    }

    /// <inheritdoc />
    public override void Disable()
    {
        if (_dummiesManager is null)
        {
            return;
        }

        Logger.Info("Disabling DummyAI...");
        Server.GameConsoleCommandHandler.UnregisterCommand(_parentCommand);
        Server.RemoteAdminCommandHandler.UnregisterCommand(_parentCommand);
        _parentCommand = null;
        CustomHandlersManager.UnregisterEventsHandler(_eventsHandler!);
        _eventsHandler = null;
        _dummiesManager.Terminate();
        _dummiesManager = null;
        Logger.Info("DummyAI is disabled.");
    }
}
