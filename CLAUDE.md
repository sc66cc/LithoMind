# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

LithoMind 是一个基于 Avalonia UI 的跨平台桌面应用程序，用于地质数据分析和可视化。项目采用 .NET 8.0 和 MVVM 架构模式。

## 构建和运行

### 构建项目
```bash
dotnet build
```

### 运行应用程序
```bash
dotnet run --project DeepTime.LithoMind.Desktop
```

### 构建 Release 版本
```bash
dotnet build -c Release
```

## 项目架构

### 三层架构

1. **LithoMind.Core** - 核心层
   - 包含领域模型、接口定义
   - 位置：`LithoMind.Core/`
   - 主要内容：
     - `Models/UI/` - UI 配置模型（MenuItemModel, UiLayoutConfig）
     - `Services/` - 服务接口（IUiConfigService）

2. **LithoMind.Infrastructure** - 基础设施层
   - 实现 Core 层定义的接口
   - 位置：`LithoMind.Infrastructure/`
   - 依赖：CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection
   - 主要内容：
     - `Services/` - 服务实现（JsonUiConfigService, CommandRegistry）

3. **DeepTime.LithoMind.Desktop** - 表示层
   - Avalonia UI 桌面应用程序
   - 位置：`DeepTime.LithoMind.Desktop/`
   - 依赖：Avalonia 11.3.8, Dock.Avalonia 11.3.8, CommunityToolkit.Mvvm 8.4.0

### Dock 布局系统

项目使用 Dock.Avalonia 实现类似 VSCode 的可停靠窗口系统：

- **LithoMindDockFactory** (`Layouts/LithoMindDockFactory.cs`)
  - 核心工厂类，负责创建和管理不同模块的布局
  - 实现 ViewModel 缓存机制，避免重复创建
  - 支持五种模块布局：
    - `DataManager` - 数据资源管理（三栏布局：本地文件、工程目录、预览区）
    - `SingleWell` - 井综合分析（三栏布局：工程目录、井数据显示、属性窗口）
    - `Seismic` - 地震综合分析（三栏布局：地震目录、地震数据显示、属性窗口）
    - `Mapping` - 编图制图（三栏布局：图层管理、制图区域、工具栏）
    - `Stratigraphy` - 地层对比（单栏布局：联井层序剖面）

### MVVM 模式

- **ViewModels** (`ViewModels/`)
  - `MainViewModel` - 主窗口 ViewModel，管理模块切换和菜单系统
  - `ViewModels/Pages/` - 各个页面的 ViewModel
  - 使用 CommunityToolkit.Mvvm 的 `[RelayCommand]` 和 `[ObservableProperty]` 特性

- **Views** (`Views/`)
  - 所有视图使用 `.axaml` 文件（Avalonia XAML）
  - 主窗口：`MainWindow.axaml`
  - 各模块视图：`DataManagerView.axaml`, `SingleWellView.axaml`, `SeismicView.axaml` 等

### UI 配置系统

- **配置文件** (`Assets/config/ui_layout.json`)
  - 定义全局菜单（File, Edit, View, Help）
  - 定义各模块的上下文工具栏菜单
  - 使用 JSON 格式，支持嵌套子菜单和分隔符
  - 菜单项包含：id, header, icon, commandId, inputGesture 等属性

- **模块 ID 映射**
  - JSON 配置中的模块 ID：`Module_DataMgr`, `Module_SingleWell`, `Module_Seismic`, `Module_Strat`, `Module_Mapping`
  - Factory 中的模块 ID：`DataManager`, `SingleWell`, `Seismic`, `Stratigraphy`, `Mapping`
  - 映射逻辑在 `MainViewModel.MapJsonIdToFactoryId()` 中

### 依赖注入

当前项目未使用 DI 容器，直接在 `App.axaml.cs` 中实例化 `MainViewModel`。如需添加 DI：
- 已引用 `Microsoft.Extensions.DependencyInjection`
- 可在 `App.OnFrameworkInitializationCompleted()` 中配置服务容器

## 关键实现细节

### 模块切换机制

1. 用户点击菜单触发 `MainViewModel.SwitchModule(moduleJsonId)`
2. 使用防抖机制（CancellationToken）避免快速连续切换
3. 异步更新 Dock 布局，避免阻塞 UI 线程
4. 更新当前模块的上下文菜单

### ViewModel 缓存

`LithoMindDockFactory` 使用 `Dictionary<string, Dictionary<string, IDockable>>` 缓存 ViewModel：
- 外层 Key：模块 ID
- 内层 Key：ViewModel ID
- 避免模块切换时重复创建 ViewModel，提升性能

### 事件连接

各模块的 ViewModel 之间通过事件进行通信：
- `LocalFilesViewModel.FileSelected` → `FilePreviewViewModel.PreviewLocalFileAsync()`
- `ProjectFilesViewModel.FileSelected` → `FilePreviewViewModel.PreviewFileAsync()`
- `WellProjectTreeViewModel.WellSelected` → `WellColumnViewModel.LoadWellData()`
- `WellColumnViewModel.InferenceCompleted` → `PropertyPanelViewModel.SetInferenceResults()`

### 命令执行

`MainViewModel.ExecuteMenu(commandId)` 根据命令 ID 执行相应操作：
- 使用 switch 语句分发命令
- 通过 `LithoMindDockFactory` 的辅助方法激活文档或显示对话框
- 示例命令：`Cmd_PrevAll`, `Cmd_SectList`, `Cmd_WellPreview`, `Cmd_View3D` 等

## 开发注意事项

### 添加新的 View/ViewModel

1. 在 `ViewModels/Pages/` 创建 ViewModel，继承自 `ViewModelBase`
2. 在 `Views/` 创建对应的 `.axaml` 和 `.axaml.cs` 文件
3. 在 `LithoMindDockFactory` 的相应模块布局方法中添加 ViewModel 创建逻辑
4. 使用 `GetOrCreateViewModel()` 方法确保 ViewModel 被缓存

### 修改 UI 布局配置

1. 编辑 `Assets/config/ui_layout.json`
2. 在 `contextToolbars` 中添加或修改模块菜单
3. 在 `MainViewModel.ExecuteMenu()` 中添加命令处理逻辑
4. 确保 `ui_layout.json` 的 `CopyToOutputDirectory` 设置为 `PreserveNewest`

### 调试 Dock 布局

- 在 Debug 模式下，Avalonia.Diagnostics 已启用（F12 打开开发者工具）
- 检查 `LithoMindDockFactory.InitLayout()` 确保 DockState 正确初始化
- 使用 `ActivateDocumentInCurrentLayout()` 方法激活特定文档标签页

### 性能优化

- ViewModel 缓存已实现，避免重复创建
- 布局切换使用异步操作，避免阻塞 UI 线程
- 使用 `Task.Run()` 在后台线程执行耗时操作
- 使用 `Dispatcher.UIThread.InvokeAsync()` 在 UI 线程更新界面

## 文件路径约定

- 配置文件：`Assets/config/`
- 资源文件：`Assets/` (使用 `avares://` URI 访问)
- 用户数据：`%AppData%/LithoMind/` (Windows)
- 布局配置：`%AppData%/LithoMind/dock_layout.json.{ModuleId}`
