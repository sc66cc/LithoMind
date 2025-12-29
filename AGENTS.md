# Repository Guidelines（仓库贡献指南）

## 项目结构与模块划分

- `LithoMind.slnx`：解决方案入口（共 3 个项目）。
- `DeepTime.LithoMind.Desktop/`：桌面端 Avalonia UI（MVVM + Dock.Avalonia）。
  - `Views/` + `ViewModels/`：视图与视图模型（如 `*View.axaml`、`*ViewModel.cs`）。
  - `Assets/`：资源文件；运行时配置位于 `Assets/config/`（重点关注 `ui_layout.json`）。
  - `Layouts/`、`Models/`：布局定义与 UI 模型。
- `LithoMind.Core/`：核心领域模型与服务契约（尽量保持无 UI 依赖）。
- `LithoMind.Infrastructure/`：基础设施实现（配置/服务等），由 Desktop 引用。

不要手改生成目录：`bin/`、`obj/`、`.vs/`。

## 构建、运行与开发命令

前置条件：.NET SDK 8.x。

- `dotnet restore`：还原 NuGet 依赖。
- `dotnet build LithoMind.slnx -c Debug`：编译全部项目。
- `dotnet run --project DeepTime.LithoMind.Desktop`：本地运行桌面端。
- `dotnet publish DeepTime.LithoMind.Desktop -c Release`：生成发布产物。
- `dotnet clean`：清理构建输出（`bin/`/`obj/`）。

## 代码风格与命名约定

- C#：4 空格缩进；已启用 `Nullable`，避免不安全的空引用用法；公共边界（public API）优先明确类型与可空性。
- 命名：类型/成员用 `PascalCase`，局部变量/参数用 `camelCase`，接口以 `I` 开头（如 `IUiConfigService`）。
- Avalonia：保持 View 与 ViewModel 成对命名与结构一致（`Views/FooView.axaml` ↔ `ViewModels/FooViewModel.cs`）。

如需格式化，优先使用 `dotnet format`，并尽量控制变更范围（仓库当前未通过 `.editorconfig` 强制统一格式）。

## 测试指南

当前仓库暂无测试项目。如需补充测试：

- 新建 `*.Tests` 项目（建议 xUnit），测试文件命名为 `*Tests.cs`。
- 使用 `dotnet test` 运行；优先覆盖 `LithoMind.Core/` 的业务行为（基础设施依赖用 mock/stub 隔离）。

## 提交与 Pull Request 规范

- Commit：保持简短、聚焦、可读（常见为动词开头；也可能使用 `Refactor:` 等前缀），避免一次提交混入无关改动。
- PR：写清变更说明与验证方式（运行过的命令）；涉及 `DeepTime.LithoMind.Desktop/` UI 变更请附截图/动图。
- 变更 `Assets/config/ui_layout.json` 时请在 PR 中显式说明（格式稳定性与兼容性优先）。
