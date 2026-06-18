using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Workbench;

/// <summary>
/// Placeholder for the formal Petrel-like project/input/results explorer.
/// It intentionally starts small so current prototype behavior remains stable while real project-store data is added.
/// </summary>
public sealed class WorkbenchProjectExplorerViewModel : PageViewModelBase
{
    public WorkbenchProjectExplorerViewModel()
    {
        Id = "WorkbenchProjectExplorer";
        Title = "图层与数据目录";
        IconKey = "📁";
        Order = 0;
    }
}
