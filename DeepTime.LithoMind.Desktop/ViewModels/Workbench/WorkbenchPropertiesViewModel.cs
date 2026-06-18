using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Workbench;

/// <summary>
/// Placeholder for context-sensitive properties, style, and QC issue panels.
/// </summary>
public sealed class WorkbenchPropertiesViewModel : PageViewModelBase
{
    public WorkbenchPropertiesViewModel()
    {
        Id = "WorkbenchProperties";
        Title = "属性展示与参数调整区";
        IconKey = "⚙️";
        Order = 0;
    }
}
