using System;
using System.Collections.Generic;
using DeepTime.LithoMind.Desktop.Layouts;
using DeepTime.LithoMind.Desktop.ViewModels.Workbench;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;

namespace DeepTime.LithoMind.Desktop.Workbench.Docking;

/// <summary>
/// Builds the main workspace directly with Dock.Avalonia native dock primitives.
/// The workbench is intentionally a clean RootDock -> ProportionalDock -> ToolDock/DocumentDock tree:
/// left catalog, center document workspace, right properties/parameters, and a bottom status/log pane.
/// Existing prototype module layouts are only used as a source of page view-models; their nested docks are not embedded.
/// </summary>
public sealed class WorkbenchLayoutService
{
    private readonly LithoMindDockFactory _factory;

    public WorkbenchLayoutService(LithoMindDockFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IRootDock CreateModuleLayout(string moduleId)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
        {
            moduleId = "Home";
        }

        return _factory.CreateLayoutForModule(moduleId);
    }

    public IRootDock CreateDefaultWorkbenchLayout(string moduleId)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
        {
            moduleId = "Home";
        }

        var moduleRoot = CreateModuleLayout(moduleId);
        var moduleParts = NativeWorkbenchModuleParts.From(moduleRoot);

        var projectExplorerVM = new WorkbenchProjectExplorerViewModel();
        var propertiesVM = new WorkbenchPropertiesViewModel();
        var logsVM = new WorkbenchLogsViewModel();

        var leftItems = moduleParts.LeftTools.Count > 0
            ? moduleParts.LeftTools
            : new List<IDockable> { projectExplorerVM };

        var documentItems = moduleParts.Documents.Count > 0
            ? moduleParts.Documents
            : new List<IDockable> { moduleRoot.ActiveDockable ?? moduleRoot.DefaultDockable ?? projectExplorerVM };

        var rightItems = moduleParts.RightTools.Count > 0
            ? moduleParts.RightTools
            : new List<IDockable> { propertiesVM };

        var bottomItems = moduleParts.BottomTools.Count > 0
            ? moduleParts.BottomTools
            : new List<IDockable> { logsVM };

        var catalogDock = new ToolDock
        {
            Id = "WorkbenchLayerDataCatalogPane",
            Title = "图层与数据目录",
            Proportion = 0.13,
            Alignment = Alignment.Left,
            ActiveDockable = leftItems[0],
            VisibleDockables = leftItems,
            GripMode = GripMode.Visible,
            CanFloat = true,
            CanPin = true,
            CanClose = true,
            IsCollapsable = true
        };

        var documentDock = new DocumentDock
        {
            Id = "WorkbenchDocumentPane",
            Title = "数据预览与工作区",
            Proportion = double.NaN,
            ActiveDockable = documentItems[0],
            VisibleDockables = documentItems,
            CanFloat = true,
            CanPin = true,
            CanClose = true,
            CanCreateDocument = true,
            IsCollapsable = false
        };

        var propertiesDock = new ToolDock
        {
            Id = "WorkbenchPropertyParameterPane",
            Title = "属性展示与参数调整区",
            Proportion = 0.13,
            Alignment = Alignment.Right,
            ActiveDockable = rightItems[0],
            VisibleDockables = rightItems,
            GripMode = GripMode.Visible,
            CanFloat = true,
            CanPin = true,
            CanClose = true,
            IsCollapsable = true
        };

        var mainRow = new ProportionalDock
        {
            Id = "WorkbenchMainWorkspaceRow",
            Orientation = Orientation.Horizontal,
            Proportion = double.NaN,
            VisibleDockables = new List<IDockable>
            {
                catalogDock,
                new ProportionalDockSplitter { Id = "WorkbenchLeftSplitter", Title = "Splitter" },
                documentDock,
                new ProportionalDockSplitter { Id = "WorkbenchRightSplitter", Title = "Splitter" },
                propertiesDock
            }
        };

        var statusDock = new ToolDock
        {
            Id = "WorkbenchBottomStatusPane",
            Title = "底部状态栏",
            Proportion = 0.06,
            Alignment = Alignment.Bottom,
            ActiveDockable = bottomItems[0],
            VisibleDockables = bottomItems,
            GripMode = GripMode.Visible,
            CanFloat = true,
            CanPin = true,
            CanClose = true,
            IsCollapsable = true
        };

        var shell = new ProportionalDock
        {
            Id = "WorkbenchNativeShell",
            Orientation = Orientation.Vertical,
            VisibleDockables = new List<IDockable>
            {
                mainRow,
                new ProportionalDockSplitter { Id = "WorkbenchBottomSplitter", Title = "Splitter" },
                statusDock
            }
        };

        return new RootDock
        {
            Id = "WorkbenchRoot",
            Title = moduleId,
            IsCollapsable = false,
            ActiveDockable = shell,
            DefaultDockable = shell,
            VisibleDockables = new List<IDockable> { shell },
            CanFloat = true,
            CanPin = true,
            CanClose = true
        };
    }

    public void InitializeLayout(IRootDock layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        _factory.InitLayout(layout);
    }

    private sealed class NativeWorkbenchModuleParts
    {
        public List<IDockable> LeftTools { get; } = new();
        public List<IDockable> Documents { get; } = new();
        public List<IDockable> RightTools { get; } = new();
        public List<IDockable> BottomTools { get; } = new();

        private readonly HashSet<string> _seen = new(StringComparer.Ordinal);

        public static NativeWorkbenchModuleParts From(IDockable moduleRoot)
        {
            var parts = new NativeWorkbenchModuleParts();
            parts.Collect(moduleRoot);
            return parts;
        }

        private void Collect(IDockable dockable)
        {
            switch (dockable)
            {
                case DocumentDock documentDock:
                    AddChildren(documentDock.VisibleDockables, Documents);
                    break;

                case ToolDock toolDock:
                    AddToolChildren(toolDock);
                    break;
            }

            if (dockable is IDock dock && dock.VisibleDockables != null)
            {
                foreach (var child in dock.VisibleDockables)
                {
                    if (child != null)
                    {
                        Collect(child);
                    }
                }
            }
        }

        private void AddToolChildren(ToolDock toolDock)
        {
            var target = toolDock.Alignment switch
            {
                Alignment.Right => RightTools,
                Alignment.Bottom => BottomTools,
                _ => LeftTools
            };

            AddChildren(toolDock.VisibleDockables, target);
        }

        private void AddChildren(IEnumerable<IDockable>? children, List<IDockable> target)
        {
            if (children == null)
            {
                return;
            }

            foreach (var child in children)
            {
                if (child == null || child is IDock)
                {
                    continue;
                }

                var key = child.Id ?? child.Title ?? child.GetType().FullName ?? Guid.NewGuid().ToString("N");
                if (_seen.Add(key))
                {
                    child.CanFloat = true;
                    child.CanPin = true;
                    child.CanClose = true;
                    target.Add(child);
                }
            }
        }
    }
}
