using System;
namespace DeepTime.LithoMind.Desktop.Workbench.Commands;

/// <summary>
/// Metadata and handlers for a command that can be surfaced from menus, toolbars, context menus, or shortcuts.
/// </summary>
public sealed class WorkbenchCommandDefinition
{
    private readonly Action<WorkbenchCommandContext> _execute;
    private readonly Func<WorkbenchCommandContext, bool>? _canExecute;

    public WorkbenchCommandDefinition(
        string id,
        string title,
        Action<WorkbenchCommandContext> execute,
        Func<WorkbenchCommandContext, bool>? canExecute = null,
        string? icon = null,
        string? shortcut = null,
        string? contextType = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Command id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Command title cannot be empty.", nameof(title));
        }

        Id = id;
        Title = title;
        Icon = icon;
        Shortcut = shortcut;
        ContextType = contextType;
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public string Id { get; }

    public string Title { get; }

    public string? Icon { get; }

    public string? Shortcut { get; }

    public string? ContextType { get; }

    public bool CanExecute(WorkbenchCommandContext context)
    {
        return _canExecute?.Invoke(context) ?? true;
    }

    public void Execute(WorkbenchCommandContext context)
    {
        if (!CanExecute(context))
        {
            return;
        }

        _execute(context);
    }
}
