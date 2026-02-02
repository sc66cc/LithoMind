# LithoMind 岩相古地理数智化重建系统

## 项目简介

LithoMind 是一个基于 **Avalonia UI** 的跨平台地质数据分析和可视化桌面应用程序。该系统旨在帮助地质工作者进行岩相古地理重建、地层对比、单井与地震数据分析以及智能编图等工作。

**核心特性：**
- 跨平台支持（Windows、macOS、Linux）
- 模块化架构，支持多种地质分析场景
- 可停靠窗口系统（类似 Visual Studio / VSCode）
- **JSON 驱动的动态菜单系统**（重点特性）
- 智能推理功能（岩相、沉积相）
- 数据标注工具与可视化

---

## 技术架构

### 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| .NET | 8.0 | 运行时框架 |
| Avalonia UI | 11.3.8 | 跨平台 UI 框架 |
| Dock.Avalonia | 11.3.8 | 可停靠窗口系统 |
| CommunityToolkit.Mvvm | 8.4.0 | MVVM 辅助库 |
| System.Text.Json | 内置 | JSON 序列化/反序列化 |

### 三层架构

```
┌───────────────────────────────────────────────────┐
│         DeepTime.LithoMind.Desktop                │  表示层
│  (Views, ViewModels, Layouts, Assets)             │
└───────────────┬───────────────────────────────────┘
                │ 依赖
┌───────────────▼───────────────────────────────────┐
│         LithoMind.Infrastructure                   │  基础设施层
│  (Services 实现, 数据访问)                          │
└───────────────┬───────────────────────────────────┘
                │ 依赖
┌───────────────▼───────────────────────────────────┐
│         LithoMind.Core                             │  核心层
│  (Models, Interfaces, 业务逻辑)                     │
└───────────────────────────────────────────────────┘
```

#### 1. LithoMind.Core（核心层）

**职责：** 定义领域模型和服务接口，不依赖任何具体实现。

**关键组件：**
- `Models/UI/MenuItemModel.cs` - 菜单项模型
- `Models/UI/UiLayoutConfig.cs` - UI 布局配置模型
- `Models/UI/MenuItemType.cs` - 菜单项类型枚举
- `Services/IUiConfigService.cs` - UI 配置服务接口

#### 2. LithoMind.Infrastructure（基础设施层）

**职责：** 实现 Core 层定义的接口，提供具体的服务实现。

**关键组件：**
- `Services/JsonUiConfigService.cs` - JSON 配置加载服务
- `Services/CommandRegistry.cs` - 命令注册表

#### 3. DeepTime.LithoMind.Desktop（表示层）

**职责：** Avalonia UI 桌面应用程序，包含所有视图和 ViewModel。

**关键组件：**
- `Views/MainWindow.axaml` - 主窗口视图
- `ViewModels/MainViewModel.cs` - 主窗口 ViewModel
- `Layouts/LithoMindDockFactory.cs` - Dock 布局工厂
- `Assets/config/ui_layout.json` - UI 菜单配置文件

---

## 🌟 JSON 动态菜单系统（核心特性）

### 系统概述

LithoMind 采用 **JSON 驱动的动态菜单系统**，将 UI 配置与代码解耦，实现了：
- 无需重新编译即可调整菜单结构
- 支持多级子菜单、分隔符、命令绑定
- 全局菜单与模块上下文菜单的统一管理
- 快捷键支持

### 配置文件结构

配置文件位于：`DeepTime.LithoMind.Desktop/Assets/config/ui_layout.json`

```json
{
  "version": "2.0",
  "comment": "Generated from FreeMind XML",
  "globalMenu": [
    {
      "header": "工程与文件",
      "type": "SubMenu",
      "id": "GenID_FileMenu",
      "children": [
        {
          "header": "打开工程",
          "commandId": "Cmd_OpenProject",
          "id": "GenID_OpenProject"
        },
        {
          "header": "新建工程",
          "type": "SubMenu",
          "children": [
            {
              "header": "导入井坐标",
              "commandId": "Cmd_ImportWellCoord",
              "id": "GenID_ImportWellCoord"
            }
          ]
        },
        {
          "type": "Separator"
        },
        {
          "header": "保存工程",
          "commandId": "Cmd_SaveProject",
          "inputGesture": "Ctrl+S"
        }
      ]
    }
  ],
  "contextToolbars": {
    "Module_DataMgr": [ /* 数据管理模块菜单 */ ],
    "Module_SingleWell": [ /* 单井分析模块菜单 */ ],
    "Module_Seismic": [ /* 地震分析模块菜单 */ ],
    "Module_Strat": [ /* 地层对比模块菜单 */ ],
    "Module_Mapping": [ /* 编图制图模块菜单 */ ]
  }
}
```

### 数据模型

#### MenuItemModel（菜单项模型）

```csharp
public class MenuItemModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; }               // 菜单项唯一标识

    [JsonPropertyName("header")]
    public string Header { get; set; }           // 显示文本

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }            // 图标路径

    [JsonPropertyName("commandId")]
    public string? CommandId { get; set; }       // 命令ID

    [JsonPropertyName("inputGesture")]
    public string? InputGesture { get; set; }    // 快捷键（如 "Ctrl+S"）

    [JsonPropertyName("type")]
    public MenuItemType Type { get; set; }       // 类型：Button/SubMenu/Separator

    [JsonPropertyName("children")]
    public List<MenuItemModel>? Children { get; set; }  // 子菜单项（支持递归）

    [JsonIgnore]
    public ICommand? Command { get; set; }       // 绑定的命令对象
}
```

#### UiLayoutConfig（UI 布局配置）

```csharp
public class UiLayoutConfig
{
    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("globalMenu")]
    public List<MenuItemModel> GlobalMenu { get; set; }  // 全局菜单（始终显示）

    [JsonPropertyName("contextToolbars")]
    public Dictionary<string, List<MenuItemModel>> ContextToolbars { get; set; }  // 模块菜单

    // 根据模块ID获取对应菜单
    public List<MenuItemModel>? GetModuleMenus(string moduleId) { ... }

    // 递归收集所有 CommandId
    public HashSet<string> GetAllCommandIds() { ... }
}
```

### 加载流程

#### 1. 配置文件加载（JsonUiConfigService）

位置：`LithoMind.Infrastructure/Services/JsonUiConfigService.cs:25`

```csharp
public async Task<UiLayoutConfig?> LoadConfigAsync()
{
    var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                 @"Assets\config\ui_layout.json");

    using var stream = File.OpenRead(fullPath);
    return await JsonSerializer.DeserializeAsync<UiLayoutConfig>(stream, _jsonOptions);
}
```

**关键配置：**
```csharp
private static readonly JsonSerializerOptions _jsonOptions = new()
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    Converters = { new JsonStringEnumConverter() }  // 支持字符串转枚举
};
```

#### 2. ViewModel 初始化（MainViewModel）

位置：`DeepTime.LithoMind.Desktop/ViewModels/MainViewModel.cs:87`

```csharp
private void LoadUiConfig()
{
    var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/config/ui_layout.json");
    using var stream = AssetLoader.Open(uri);
    using var reader = new StreamReader(stream);
    var json = reader.ReadToEnd();

    _uiConfig = JsonSerializer.Deserialize<UiLayoutConfig>(json, options);

    if (_uiConfig != null)
    {
        GlobalMenus = _uiConfig.GlobalMenu;  // 设置全局菜单
    }
}
```

#### 3. 模块切换与菜单更新

位置：`DeepTime.LithoMind.Desktop/ViewModels/MainViewModel.cs:119`

```csharp
[RelayCommand]
public async Task SwitchModule(string? moduleJsonId)
{
    if (_currentModuleId == moduleJsonId) return;

    // 获取模块对应的上下文菜单
    var moduleMenus = _uiConfig.GetModuleMenus(moduleJsonId);
    CurrentModuleMenus = moduleMenus ?? new List<MenuItemModel>();

    // 更新 Dock 布局
    string factoryId = MapJsonIdToFactoryId(moduleJsonId);
    await UpdateDockLayoutAsync(factoryId, cancellationToken);
}
```

### 视图绑定

#### MainWindow.axaml 菜单渲染

位置：`DeepTime.LithoMind.Desktop/Views/MainWindow.axaml:17`

**数据模板（支持递归渲染子菜单）：**
```xml
<Window.DataTemplates>
    <TreeDataTemplate DataType="models:MenuItemModel" ItemsSource="{Binding Children}">
        <TextBlock Text="{Binding Header}" VerticalAlignment="Center"/>
    </TreeDataTemplate>
</Window.DataTemplates>
```

**菜单项样式（自动绑定命令）：**
```xml
<Style Selector="MenuItem" x:DataType="models:MenuItemModel">
    <Setter Property="Header" Value="{Binding Header}" />
    <Setter Property="ItemsSource" Value="{Binding Children}" />
    <Setter Property="Command"
            Value="{Binding $parent[Window].((vm:MainViewModel)DataContext).ExecuteMenuCommand}" />
    <Setter Property="CommandParameter" Value="{Binding CommandId}" />
    <Setter Property="Classes.separator" Value="{Binding IsSeparator}" />
</Style>
```

**全局菜单与模块菜单布局：**
```xml
<Grid ColumnDefinitions="Auto,Auto,*,Auto">
    <!-- 全局菜单 (始终显示) -->
    <Menu Grid.Column="0" ItemsSource="{Binding GlobalMenus}" />

    <!-- 分隔线 -->
    <Border Grid.Column="1" Width="1" Background="#D0D0D0" />

    <!-- 模块菜单 (动态切换) -->
    <Menu Grid.Column="2" ItemsSource="{Binding CurrentModuleMenus}" />
</Grid>
```

### 命令分发

位置：`DeepTime.LithoMind.Desktop/ViewModels/MainViewModel.cs:299`

```csharp
[RelayCommand]
public void ExecuteMenu(string? commandId)
{
    if (string.IsNullOrWhiteSpace(commandId)) return;

    switch (commandId)
    {
        case "Cmd_OpenProject":
            // 打开工程
            break;

        case "Cmd_WellPreview":
            // 激活单井柱状图
            _factory.ActivateDocumentInCurrentLayout("WellColumn");
            break;

        case "Cmd_LithofaciesInference":
            // 岩相智能推理
            _factory.RunLithofaciesInference();
            break;

        // ... 更多命令
    }
}
```

### 菜单系统特点总结

✅ **配置驱动**：菜单结构完全由 JSON 配置，无需修改代码
✅ **递归渲染**：支持任意深度的子菜单
✅ **类型安全**：使用强类型模型和枚举
✅ **模块化**：全局菜单 + 模块上下文菜单分离
✅ **命令绑定**：通过 CommandId 统一分发
✅ **快捷键支持**：通过 InputGesture 属性配置
✅ **分隔符支持**：通过 Type=Separator 实现
✅ **异常容错**：配置加载失败时提供降级菜单

---

## 模块系统

LithoMind 支持 6 个主要功能模块，每个模块有独立的 Dock 布局和上下文菜单：

| 模块 ID | 显示名称 | 功能描述 | 布局结构 |
|---------|---------|---------|---------|
| `Module_Home` | 首页 | 欢迎页面和快速导航 | 单栏布局 |
| `Module_DataMgr` | 多源数据解析与融合 | 数据导入、管理、预览 | 三栏：本地文件 \| 工程目录 \| 预览区 |
| `Module_Strat` | 地层智能对比 | 联井地层对比分析 | 单栏：联井剖面 |
| `Module_SingleWell` | 单井相智能分析 | 单井柱状图、测井解释 | 三栏：工程目录 \| 井数据 \| 属性面板 |
| `Module_Seismic` | 地震相智能分析 | 地震数据解释与标注 | 三栏：地震目录 \| 地震剖面 \| 属性面板 |
| `Module_Mapping` | 岩相古地理智能编图 | 编图制图与可视化 | 三栏：图层管理 \| 制图区域 \| 工具栏 |

### 模块 ID 映射

JSON 配置中的模块 ID 与 Factory 内部 ID 的映射关系：

```csharp
// MainViewModel.cs:288
private string MapJsonIdToFactoryId(string jsonId)
{
    if (jsonId.Contains("Home")) return "Home";
    if (jsonId.Contains("DataMgr")) return "DataManager";
    if (jsonId.Contains("SingleWell")) return "SingleWell";
    if (jsonId.Contains("Seismic")) return "Seismic";
    if (jsonId.Contains("Strat")) return "Stratigraphy";
    if (jsonId.Contains("Mapping")) return "Mapping";
    return "Home";
}
```

---

## Dock 布局系统

### LithoMindDockFactory

位置：`DeepTime.LithoMind.Desktop/Layouts/LithoMindDockFactory.cs`

**核心职责：**
- 为每个模块创建独立的 Dock 布局
- 缓存 ViewModel 实例，避免重复创建
- 提供文档激活、对话框显示等辅助方法

**缓存机制：**
```csharp
// 两级字典：模块ID -> (ViewModel ID -> ViewModel 实例)
private Dictionary<string, Dictionary<string, IDockable>> _viewModelCache;

private T GetOrCreateViewModel<T>(string moduleId, string viewModelId)
    where T : IDockable, new()
{
    if (!_viewModelCache.ContainsKey(moduleId))
        _viewModelCache[moduleId] = new Dictionary<string, IDockable>();

    if (!_viewModelCache[moduleId].ContainsKey(viewModelId))
        _viewModelCache[moduleId][viewModelId] = new T { Id = viewModelId };

    return (T)_viewModelCache[moduleId][viewModelId];
}
```

**布局预加载：**
```csharp
// 在后台线程预创建所有模块布局，提升首次切换速度
public void PreloadAllLayouts()
{
    var moduleIds = new[] { "Home", "DataManager", "SingleWell", "Seismic", "Stratigraphy", "Mapping" };
    foreach (var id in moduleIds)
    {
        CreateLayoutForModule(id);
    }
}
```

---

## MVVM 模式

### ViewModel 基类

```csharp
public class ViewModelBase : ObservableObject, IDockable
{
    // Dock 系统所需属性
    public string Id { get; set; }
    public string Title { get; set; }
    public object? Context { get; set; }
    public IDockable? Owner { get; set; }
    public IFactory? Factory { get; set; }
}
```

### 命令绑定

使用 `CommunityToolkit.Mvvm` 的 `[RelayCommand]` 特性：

```csharp
[RelayCommand]
public async Task SwitchModule(string? moduleId)
{
    // 自动生成 SwitchModuleCommand 属性
}
```

### 属性通知

使用 `[ObservableProperty]` 自动生成属性通知：

```csharp
[ObservableProperty]
private List<MenuItemModel>? _globalMenus;
// 自动生成 GlobalMenus 属性，带 INotifyPropertyChanged
```

---

## 构建与运行

### 前置要求

- .NET 8.0 SDK
- Rider / Visual Studio 2022 / VS Code

### 构建项目

```bash
# 克隆仓库
cd /Users/user/RiderProjects/LithoMind

# 构建项目
dotnet build

# 构建 Release 版本
dotnet build -c Release
```

### 运行应用程序

```bash
# 调试模式运行
dotnet run --project DeepTime.LithoMind.Desktop

# Release 模式运行
dotnet run --project DeepTime.LithoMind.Desktop -c Release
```

### 开发调试

在 Debug 模式下，Avalonia.Diagnostics 已启用：
- 按 **F12** 打开 Avalonia DevTools
- 可以实时检查控件树、属性、样式

---

## 项目结构

```
LithoMind/
├── DeepTime.LithoMind.Desktop/           # 表示层
│   ├── Assets/
│   │   └── config/
│   │       └── ui_layout.json           # 🌟 菜单配置文件
│   ├── Layouts/
│   │   └── LithoMindDockFactory.cs      # Dock 布局工厂
│   ├── ViewModels/
│   │   ├── Base/
│   │   │   └── ViewModelBase.cs
│   │   ├── Pages/                       # 各模块 ViewModel
│   │   │   ├── DataManagerViewModel.cs
│   │   │   ├── SingleWellViewModel.cs
│   │   │   ├── SeismicViewModel.cs
│   │   │   └── ...
│   │   └── MainViewModel.cs             # 🌟 主 ViewModel
│   ├── Views/
│   │   ├── MainWindow.axaml             # 🌟 主窗口视图
│   │   ├── DataManagerView.axaml
│   │   └── ...
│   ├── Program.cs                       # 入口点
│   └── DeepTime.LithoMind.Desktop.csproj
│
├── LithoMind.Infrastructure/             # 基础设施层
│   ├── Services/
│   │   ├── JsonUiConfigService.cs       # 🌟 JSON 配置加载
│   │   └── CommandRegistry.cs
│   └── LithoMind.Infrastructure.csproj
│
├── LithoMind.Core/                      # 核心层
│   ├── Models/
│   │   └── UI/
│   │       ├── MenuItemModel.cs         # 🌟 菜单项模型
│   │       ├── UiLayoutConfig.cs        # 🌟 配置模型
│   │       └── MenuItemType.cs          # 菜单类型枚举
│   ├── Services/
│   │   └── IUiConfigService.cs
│   └── LithoMind.Core.csproj
│
├── CLAUDE.md                            # Claude Code 项目指引
└── PROJECT_README.md                    # 本文档
```

---

## 开发指南

### 添加新菜单项

1. **编辑 JSON 配置文件**

   位置：`DeepTime.LithoMind.Desktop/Assets/config/ui_layout.json`

   ```json
   {
     "header": "新功能",
     "commandId": "Cmd_NewFeature",
     "id": "GenID_NewFeature",
     "inputGesture": "Ctrl+N"
   }
   ```

2. **在 MainViewModel 中添加命令处理**

   位置：`MainViewModel.cs:299` ExecuteMenu 方法

   ```csharp
   case "Cmd_NewFeature":
       // 实现新功能逻辑
       break;
   ```

3. **无需重启**，配置文件在下次启动时自动生效

### 添加新模块

1. **创建 ViewModel**

   ```csharp
   public class NewModuleViewModel : ViewModelBase
   {
       public NewModuleViewModel()
       {
           Id = "NewModule";
           Title = "新模块";
       }
   }
   ```

2. **创建 View**

   ```xml
   <UserControl xmlns="https://github.com/avaloniaui"
                x:Class="DeepTime.LithoMind.Desktop.Views.NewModuleView">
       <!-- 内容 -->
   </UserControl>
   ```

3. **在 LithoMindDockFactory 中添加布局**

   ```csharp
   private IRootDock CreateNewModuleLayout()
   {
       var vm = GetOrCreateViewModel<NewModuleViewModel>("NewModule", "NewModule");
       // 创建布局...
   }
   ```

4. **在 ui_layout.json 中添加模块菜单**

   ```json
   "contextToolbars": {
       "Module_NewModule": [ /* 菜单项 */ ]
   }
   ```

### 调试技巧

1. **查看菜单绑定**

   在 MainViewModel 构造函数设置断点，检查 `GlobalMenus` 和 `CurrentModuleMenus`

2. **检查命令执行**

   在 `ExecuteMenu` 方法设置断点，观察 `commandId` 参数

3. **验证 JSON 配置**

   使用在线 JSON 校验工具检查配置文件格式

---

## 性能优化

### 已实现的优化

✅ **ViewModel 缓存**
- 模块切换时复用 ViewModel 实例，避免重复创建

✅ **布局预加载**
- 在后台线程预创建所有模块布局，提升首次切换速度

✅ **异步加载**
- 布局切换使用 `async/await`，避免阻塞 UI 线程

✅ **防抖机制**
- 使用 `CancellationToken` 避免快速连续切换导致的资源浪费

### 建议的优化方向

⚠️ **虚拟化列表**
- 对于大量数据的列表（如井列表），使用 `VirtualizingStackPanel`

⚠️ **延迟加载**
- 仅在模块激活时加载数据，而非启动时全部加载

⚠️ **图像缓存**
- 对于岩心照片、地震剖面等图像，实现缓存机制

---

## 常见问题

### Q1: 菜单不显示怎么办？

**排查步骤：**
1. 检查 `ui_layout.json` 是否正确复制到输出目录
2. 查看项目文件中是否设置 `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`
3. 在 `LoadUiConfig` 方法设置断点，检查是否成功加载

### Q2: 命令执行没有响应？

**排查步骤：**
1. 检查 JSON 配置中的 `commandId` 是否正确
2. 在 `ExecuteMenu` 方法中添加对应的 case 分支
3. 确认命令参数绑定是否正确：`CommandParameter="{Binding CommandId}"`

### Q3: 如何添加快捷键？

在 JSON 配置中添加 `inputGesture` 属性：
```json
{
  "header": "保存",
  "commandId": "Cmd_Save",
  "inputGesture": "Ctrl+S"
}
```

注意：Avalonia 支持的快捷键格式为 `Ctrl+Key`, `Shift+Key`, `Alt+Key` 等。

### Q4: 如何调试 Dock 布局问题？

1. 按 **F12** 打开 Avalonia DevTools
2. 在 "Visual Tree" 中查找 `DockControl`
3. 检查 `Layout` 属性是否正确绑定
4. 查看 `Factory.InitLayout()` 是否被调用

---

## 未来规划

- [ ] 实现 Dock 布局序列化（保存/恢复用户自定义布局）
- [ ] 添加插件系统，支持第三方扩展
- [ ] 国际化支持（多语言菜单）
- [ ] 主题切换（浅色/深色模式）
- [ ] 命令搜索功能（类似 VSCode 的 Command Palette）
- [ ] 菜单项权限控制（基于角色的访问控制）

---

## 贡献指南

欢迎贡献代码、报告问题或提出建议！

### 提交代码

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 提交 Pull Request

### 报告问题

请在 GitHub Issues 中报告问题，并提供：
- 操作系统和版本
- .NET 版本
- 详细的复现步骤
- 错误日志（如有）

---

## 许可证

本项目采用 MIT 许可证。详情请参阅 LICENSE 文件。

---

## 致谢

- [Avalonia UI](https://github.com/AvaloniaUI/Avalonia) - 跨平台 UI 框架
- [Dock](https://github.com/wieslawsoltes/Dock) - 可停靠窗口系统
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM 辅助库

---

**最后更新：** 2026-02-02
**文档版本：** 1.0
