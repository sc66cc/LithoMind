using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Workbench;

/// <summary>
/// Bottom status pane matching the prototype: selected object, cursor/depth feedback, resource usage, and API state.
/// </summary>
public sealed class WorkbenchLogsViewModel : PageViewModelBase
{
    public WorkbenchLogsViewModel()
    {
        Id = "WorkbenchLogs";
        Title = "底部状态栏";
        IconKey = "📋";
        Order = 0;
    }

    public string SelectedObjectText { get; } = "选中要素名称 /";

    public string CursorContextText { get; } = "当前鼠标位置的坐标/深度/层位/道号等";

    public string StatusTitle { get; } = "底部状态栏";

    public string MemoryUsageText { get; } = "内存占用";

    public string CpuUsageText { get; } = "CPU占用";

    public string GpuUsageText { get; } = "GPU占用";

    public string ApiConnectionText { get; } = "API连接状态";
}
