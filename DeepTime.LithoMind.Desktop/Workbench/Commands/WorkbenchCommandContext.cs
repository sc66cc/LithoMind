namespace DeepTime.LithoMind.Desktop.Workbench.Commands;

/// <summary>
/// Describes the current workbench target for command enablement and execution.
/// </summary>
/// <param name="TargetType">Logical target type, such as Well, Curve, Chart, or Seismic.</param>
/// <param name="TargetId">Optional target identifier within the active project.</param>
/// <param name="Payload">Optional richer payload for command handlers that need the selected object.</param>
public sealed record WorkbenchCommandContext(
    string? TargetType = null,
    string? TargetId = null,
    object? Payload = null);
