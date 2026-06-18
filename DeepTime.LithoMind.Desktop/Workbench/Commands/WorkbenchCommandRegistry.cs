using System;
using System.Collections.Generic;

namespace DeepTime.LithoMind.Desktop.Workbench.Commands;

/// <summary>
/// Central registry for workbench commands. New formal-development features should register here
/// instead of expanding MainViewModel switch statements.
/// </summary>
public sealed class WorkbenchCommandRegistry
{
    private readonly Dictionary<string, WorkbenchCommandDefinition> _commands = new(StringComparer.Ordinal);

    public IReadOnlyCollection<WorkbenchCommandDefinition> Commands => _commands.Values;

    public void Register(WorkbenchCommandDefinition command)
    {
        ArgumentNullException.ThrowIfNull(command);
        _commands[command.Id] = command;
    }

    public bool TryGet(string id, out WorkbenchCommandDefinition? command)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            command = null;
            return false;
        }

        return _commands.TryGetValue(id, out command);
    }

    public bool Execute(string id, WorkbenchCommandContext context)
    {
        if (!TryGet(id, out var command) || command is null || !command.CanExecute(context))
        {
            return false;
        }

        command.Execute(context);
        return true;
    }
}
