using DeepTime.LithoMind.Desktop.Layouts;
using DeepTime.LithoMind.Desktop.Workbench.Commands;
using DeepTime.LithoMind.Desktop.Workbench.Docking;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;

var tests = new List<(string Name, Action Test)>
{
    ("command registry registers and executes context-aware command", CommandRegistryRegistersAndExecutesContextAwareCommand),
    ("workbench layout service delegates module layout creation", WorkbenchLayoutServiceDelegatesModuleLayoutCreation),
    ("workbench layout service returns native dock module layout without nested shell", WorkbenchLayoutServiceReturnsNativeDockModuleLayoutWithoutNestedShell),
    ("dock fluent menu resources are localized to Chinese", DockFluentMenuResourcesAreLocalizedToChinese),
    ("workbench native dock regions match prototype labels and proportions", WorkbenchNativeDockRegionsMatchPrototypeLabelsAndProportions),
    ("bottom status pane exposes prototype status fields", BottomStatusPaneExposesPrototypeStatusFields),
    ("module layouts expose panes as freely dockable surfaces", ModuleLayoutsExposePanesAsFreelyDockableSurfaces)
};

var failed = 0;
foreach (var (name, test) in tests)
{
    try
    {
        test();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL {name}: {ex.Message}");
    }
}

if (failed > 0)
{
    Console.Error.WriteLine($"{failed} test(s) failed.");
    Environment.Exit(1);
}

static void CommandRegistryRegistersAndExecutesContextAwareCommand()
{
    var registry = new WorkbenchCommandRegistry();
    var executedWith = string.Empty;
    var context = new WorkbenchCommandContext("Well", "HZ19-1-1A");

    registry.Register(new WorkbenchCommandDefinition(
        id: "OpenWell",
        title: "打开井",
        execute: ctx => executedWith = ctx.TargetId ?? string.Empty,
        canExecute: ctx => ctx.TargetType == "Well"));

    AssertTrue(registry.TryGet("OpenWell", out var command), "registered command should be found");
    AssertTrue(command!.CanExecute(context), "command should execute for Well context");
    AssertFalse(command.CanExecute(new WorkbenchCommandContext("Seismic", "S1")), "command should not execute for Seismic context");

    command.Execute(context);
    AssertEqual("HZ19-1-1A", executedWith, "command should receive execution context");
}

static void WorkbenchLayoutServiceDelegatesModuleLayoutCreation()
{
    var factory = new LithoMindDockFactory(new object());
    var service = new WorkbenchLayoutService(factory);

    var layout = service.CreateModuleLayout("Home");

    AssertEqual("Home", layout.Title, "service should create root dock for requested module");
}


static void WorkbenchLayoutServiceReturnsNativeDockModuleLayoutWithoutNestedShell()
{
    var factory = new LithoMindDockFactory(new object());
    var service = new WorkbenchLayoutService(factory);

    var layout = service.CreateDefaultWorkbenchLayout("SingleWell");

    AssertEqual("WorkbenchRoot", layout.Id, "workbench should use one clean native root dock");
    AssertEqual("SingleWell", layout.Title, "native workbench root should keep the active module title");
    AssertTrue(FindDockable(layout, "WorkbenchLayerDataCatalogPane") is ToolDock, "left layer/data catalog should be a native ToolDock");
    AssertTrue(FindDockable(layout, "WorkbenchDocumentPane") is DocumentDock, "center work area should be a native DocumentDock");
    AssertTrue(FindDockable(layout, "WorkbenchPropertyParameterPane") is ToolDock, "right properties/parameters pane should be a native ToolDock");
    AssertTrue(FindDockable(layout, "WorkbenchBottomStatusPane") is ToolDock, "bottom status pane should be a native ToolDock");
    AssertTrue(FindDockable(layout, "SingleWellMainLayout") is null, "module proportional docks should be flattened and not embedded in the workbench");

    var documentDock = (DocumentDock)FindDockable(layout, "WorkbenchDocumentPane")!;
    AssertTrue(documentDock.VisibleDockables?.All(dockable => dockable is not IDock) == true, "center document dock should contain pages/documents, not nested docks");
    AssertTrue(FindDockable(layout, "WellColumn") is not null, "single-well chart document should be available in the center work area");
    AssertTrue(FindDockable(layout, "WellCorrelation") is not null, "well correlation document should be available in the center work area");
}

static void DockFluentMenuResourcesAreLocalizedToChinese()
{
    var appXaml = File.ReadAllText(Path.Combine("DeepTime.LithoMind.Desktop", "App.axaml"));

    var expectedResources = new Dictionary<string, string>
    {
        ["DocumentTabStripItemFloatString"] = "浮动",
        ["DocumentTabStripItemCloseString"] = "关闭",
        ["DocumentTabStripItemCloseOtherTabsString"] = "关闭其他标签页",
        ["DocumentTabStripItemCloseAllTabsString"] = "关闭所有标签页",
        ["DocumentTabStripItemNewHorizontalDockString"] = "新建横向文档组",
        ["DocumentTabStripItemNewVerticalDockString"] = "新建纵向文档组",
        ["DocumentTabStripItemTabLayoutString"] = "标签布局",
        ["ToolTabStripItemDockString"] = "停靠",
        ["ToolTabStripItemAutoHideString"] = "自动隐藏",
        ["ToolTabStripItemDockAsDocumentString"] = "作为文档标签停靠",
        ["ToolPinItemControlShowString"] = "显示",
        ["DragPreviewControlDockString"] = "停靠"
    };

    foreach (var (key, text) in expectedResources)
    {
        AssertTrue(appXaml.Contains($"x:Key=\"{key}\"", StringComparison.Ordinal), $"App.axaml should define Dock resource key '{key}'");
        AssertTrue(appXaml.Contains($">{text}</x:String>", StringComparison.Ordinal), $"Dock resource '{key}' should use Chinese text '{text}'");
    }
}

static void WorkbenchNativeDockRegionsMatchPrototypeLabelsAndProportions()
{
    var factory = new LithoMindDockFactory(new object());
    var service = new WorkbenchLayoutService(factory);

    var layout = service.CreateDefaultWorkbenchLayout("DataManager");

    var left = (ToolDock)FindDockable(layout, "WorkbenchLayerDataCatalogPane")!;
    var center = (DocumentDock)FindDockable(layout, "WorkbenchDocumentPane")!;
    var right = (ToolDock)FindDockable(layout, "WorkbenchPropertyParameterPane")!;
    var bottom = (ToolDock)FindDockable(layout, "WorkbenchBottomStatusPane")!;

    AssertEqual("图层与数据目录", left.Title, "left dock should match prototype label");
    AssertEqual("数据预览与工作区", center.Title, "center dock should match prototype label");
    AssertEqual("属性展示与参数调整区", right.Title, "right dock should match prototype label");
    AssertEqual("底部状态栏", bottom.Title, "bottom dock should match prototype label");

    AssertEqual(0.13, left.Proportion, "left dock should use a narrow catalog width");
    AssertEqual(0.13, right.Proportion, "right dock should use a narrow property width");
    AssertEqual(0.06, bottom.Proportion, "bottom dock should behave like a status bar");
    AssertTrue(double.IsNaN(center.Proportion), "center work area should take remaining space");
}

static void BottomStatusPaneExposesPrototypeStatusFields()
{
    var viewModel = new DeepTime.LithoMind.Desktop.ViewModels.Workbench.WorkbenchLogsViewModel();

    AssertEqual("选中要素名称 /", viewModel.SelectedObjectText, "status pane should expose selected object field");
    AssertEqual("当前鼠标位置的坐标/深度/层位/道号等", viewModel.CursorContextText, "status pane should expose cursor/depth context field");
    AssertEqual("底部状态栏", viewModel.StatusTitle, "status pane should expose center title field");
    AssertEqual("内存占用", viewModel.MemoryUsageText, "status pane should expose memory field");
    AssertEqual("CPU占用", viewModel.CpuUsageText, "status pane should expose cpu field");
    AssertEqual("GPU占用", viewModel.GpuUsageText, "status pane should expose gpu field");
    AssertEqual("API连接状态", viewModel.ApiConnectionText, "status pane should expose api state field");
}

static void ModuleLayoutsExposePanesAsFreelyDockableSurfaces()
{
    var factory = new LithoMindDockFactory(new object());
    foreach (var moduleId in new[] { "Home", "DataManager", "SingleWell", "Seismic", "Mapping", "Stratigraphy" })
    {
        var layout = factory.CreateLayoutForModule(moduleId);
        var panes = CollectDockables(layout)
            .Where(pane => pane is ToolDock or DocumentDock)
            .ToList();

        AssertTrue(panes.Count > 0, $"module '{moduleId}' should expose dock panes");
        foreach (var pane in panes)
        {
            AssertTrue(pane.CanFloat, $"module '{moduleId}' pane '{pane.Id}' should be floatable");
            AssertTrue(pane.CanPin, $"module '{moduleId}' pane '{pane.Id}' should be pinnable/fixable");
            AssertTrue(pane.CanClose, $"module '{moduleId}' pane '{pane.Id}' should be closeable");

            if (pane is DocumentDock documentDock)
            {
                AssertTrue(documentDock.CanCreateDocument, $"module '{moduleId}' document pane '{pane.Id}' should support split document groups");
            }
        }
    }
}

static List<IDockable> CollectDockables(IDockable? dockable)
{
    var result = new List<IDockable>();
    CollectDockablesRecursive(dockable, result);
    return result;
}

static void CollectDockablesRecursive(IDockable? dockable, List<IDockable> result)
{
    if (dockable == null)
    {
        return;
    }

    result.Add(dockable);

    if (dockable is IDock dock && dock.VisibleDockables != null)
    {
        foreach (var child in dock.VisibleDockables)
        {
            CollectDockablesRecursive(child, result);
        }
    }
}

static IDockable? FindDockable(IDockable? dockable, string id)
{
    if (dockable?.Id == id)
    {
        return dockable;
    }

    if (dockable is IDock dock && dock.VisibleDockables != null)
    {
        foreach (var child in dock.VisibleDockables)
        {
            var found = FindDockable(child, id);
            if (found != null)
            {
                return found;
            }
        }
    }

    return null;
}

static void AssertTrue(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void AssertFalse(bool condition, string message) => AssertTrue(!condition, message);

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{message}. Expected '{expected}', got '{actual}'.");
    }
}
