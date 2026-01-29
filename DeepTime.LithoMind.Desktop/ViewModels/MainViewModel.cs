using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.Layouts;
using DeepTime.LithoMind.Desktop.ViewModels.Base;
using DeepTime.LithoMind.Desktop.Views;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;
using LithoMind.Core.Models.UI; // [修复] 确保引用 Core 中的模型

namespace DeepTime.LithoMind.Desktop.ViewModels
{
	public partial class MainViewModel : ViewModelBase
	{
		private readonly LithoMindDockFactory _factory;
		private UiLayoutConfig? _uiConfig;
		private const string LayoutConfigPath = "dock_layout.json";
		
		// 防抖机制：避免快速连续切换
		private System.Threading.CancellationTokenSource? _switchModuleCancellation;
		private string? _currentModuleId;

		/// <summary>
		/// Dock Factory - 用于启用拖拽、停靠、浮动等功能
		/// </summary>
		public new IFactory Factory => _factory;

		// 当前 Dock 布局
		private IRootDock? _layout;
		public IRootDock? Layout
		{
			get => _layout;
			set => SetProperty(ref _layout, value);
		}

		// 全局菜单 (File, Edit...)
		private List<MenuItemModel>? _globalMenus;
		public List<MenuItemModel>? GlobalMenus
		{
			get => _globalMenus;
			set => SetProperty(ref _globalMenus, value);
		}

		// 场景工具栏菜单 (动态变化)
		private List<MenuItemModel>? _currentModuleMenus;
		public List<MenuItemModel>? CurrentModuleMenus
		{
			get => _currentModuleMenus;
			set => SetProperty(ref _currentModuleMenus, value);
		}

		public MainViewModel()
		{
			_factory = new LithoMindDockFactory(this);

			LoadUiConfig();

			// 预创建所有模块的布局（后台执行，提升首次切换速度）
			_ = PreloadLayoutsAsync();

			_ = SwitchModule("Module_DataMgr");
		}

		/// <summary>
		/// 异步预创建所有模块的布局
		/// </summary>
		private async System.Threading.Tasks.Task PreloadLayoutsAsync()
		{
			// 延迟执行，避免阻塞启动
			await System.Threading.Tasks.Task.Delay(100);

			// 在后台线程预创建布局
			await System.Threading.Tasks.Task.Run(() =>
			{
				_factory.PreloadAllLayouts();
			});
		}

		private void LoadUiConfig()
		{
			try
			{
				var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/config/ui_layout.json");
				if (AssetLoader.Exists(uri))
				{
					using var stream = AssetLoader.Open(uri);
					using var reader = new StreamReader(stream);
					var json = reader.ReadToEnd();
		
					var options = new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true,
						Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
					};
						
					_uiConfig = JsonSerializer.Deserialize<UiLayoutConfig>(json, options);
						
					if (_uiConfig != null)
					{
						GlobalMenus = _uiConfig.GlobalMenu;
					}
				}
			}
			catch
			{
				_uiConfig = new UiLayoutConfig();
				GlobalMenus = new List<MenuItemModel>();
			}
		}

		[RelayCommand]
		public async System.Threading.Tasks.Task SwitchModule(string? moduleJsonId)
		{
			if (string.IsNullOrEmpty(moduleJsonId) || _uiConfig == null) return;
			
			// 如果切换到相同模块，直接返回
			if (_currentModuleId == moduleJsonId) return;
		
			// 取消之前的切换操作
			_switchModuleCancellation?.Cancel();
			_switchModuleCancellation = new System.Threading.CancellationTokenSource();
			var cancellationToken = _switchModuleCancellation.Token;
		
			// 直接使用JSON中的模块ID获取菜单
			var moduleMenus = _uiConfig.GetModuleMenus(moduleJsonId);
			CurrentModuleMenus = moduleMenus ?? new List<MenuItemModel>();
		
			// 异步更新 Dock 布局，避免阻塞UI线程
			string factoryId = MapJsonIdToFactoryId(moduleJsonId);
			_currentModuleId = moduleJsonId;
			
			try
			{
				// 切换到地震模块时默认关闭属性窗口，等待标注再显式打开
				if (factoryId == "Seismic")
				{
					_factory.HideSeismicPropertyPane();
				}

				await UpdateDockLayoutAsync(factoryId, cancellationToken);
			}
			catch (System.Threading.Tasks.TaskCanceledException)
			{
				// 切换被取消，这是正常的
			}
		}

		/// <summary>
		/// 异步更新Dock布局，提升UI响应性
		/// 优化：由于布局已缓存，切换速度大幅提升
		/// </summary>
		private async System.Threading.Tasks.Task UpdateDockLayoutAsync(string factoryId, System.Threading.CancellationToken cancellationToken = default)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				// 获取新布局（由于缓存机制，这个操作非常快）
				var newLayout = _factory.CreateLayoutForModule(factoryId);

				if (newLayout != null)
				{
					cancellationToken.ThrowIfCancellationRequested();

					// 初始化并设置布局（全部在UI线程执行，避免跨线程问题）
					_factory.InitLayout(newLayout);

					// 尝试加载保存的布局配置
					if (!TryLoadLayoutFromFile(factoryId))
					{
						// 使用默认布局
						Layout = newLayout;
					}
				}
			}
			catch (System.Threading.Tasks.TaskCanceledException)
			{
				// 切换被取消，这是正常的
				throw;
			}
			catch
			{
				// 布局切换失败时静默处理
			}

			await System.Threading.Tasks.Task.CompletedTask;
		}

		/// <summary>
		/// 尝试从文件加载布局配置
		/// </summary>
		private bool TryLoadLayoutFromFile(string moduleId)
		{
			try
			{
				var layoutFile = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"LithoMind",
					$"{LayoutConfigPath}.{moduleId}"
				);

				if (File.Exists(layoutFile))
				{
					// TODO: 使用 Dock.Serializer 反序列化布局
					// var layoutJson = File.ReadAllText(layoutFile);
					// Layout = Deserialize layout...
					return false; // 暂时返回 false，待实现序列化器
				}
			}
			catch
			{
				// 加载失败时静默处理
			}
			return false;
		}

		/// <summary>
		/// 保存当前布局配置到文件
		/// </summary>
		[RelayCommand]
		public void SaveLayoutToFile()
		{
			if (Layout == null) return;

			try
			{
				var appDataPath = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"LithoMind"
				);

				if (!Directory.Exists(appDataPath))
				{
					Directory.CreateDirectory(appDataPath);
				}

				var layoutFile = Path.Combine(appDataPath, $"{LayoutConfigPath}.{Layout.Title}");

				// TODO: 使用 Dock.Serializer 序列化布局
				// var layoutJson = Serialize(Layout);
				// File.WriteAllText(layoutFile, layoutJson);
			}
			catch
			{
				// 保存失败时静默处理
			}
		}

		/// <summary>
		/// 重置当前模块的布局为默认状态
		/// </summary>
		[RelayCommand]
		public void ResetLayout()
		{
			if (Layout == null) return;

			try
			{
				var currentModuleId = Layout.Title;
				if (Layout.Close.CanExecute(null))
				{
					Layout.Close.Execute(null);
				}

				var newLayout = _factory.CreateLayoutForModule(currentModuleId);
				if (newLayout != null)
				{
					_factory.InitLayout(newLayout);
					Layout = newLayout;
				}
			}
			catch
			{
				// 重置失败时静默处理
			}
		}

		

		private string MapJsonIdToFactoryId(string jsonId)
		{
			if (jsonId.Contains("DataMgr")) return "DataManager";
			if (jsonId.Contains("SingleWell")) return "SingleWell";
			if (jsonId.Contains("Seismic")) return "Seismic";
			if (jsonId.Contains("Strat")) return "Stratigraphy";
			if (jsonId.Contains("Mapping")) return "Mapping";
			return "DataManager";
		}

		[RelayCommand]
		public void ExecuteMenu(string? commandId)
		{
			if (string.IsNullOrWhiteSpace(commandId)) return;
				
			// 根据命令ID执行相应操作
			switch (commandId)
			{
				// 多源数据解析与融合模块命令
				case "Cmd_PrevAll":
					// 预览所有已加载数据 - 切换到工区平面图
					_factory.ActivateDocumentInCurrentLayout("WorkAreaMap");
					break;

				// 单井相智能分析模块命令
				case "Cmd_SectList":
					// 从单井列表拉剖面 - 显示井选择对话框并切换到联井剖面
					_factory.ShowWellSelectionAndActivateCorrelation();
					break;
		
				case "Cmd_WellPreview":
					// 单井综合柱状图 - 切换到单井柱状图标签页
					_factory.ActivateDocumentInCurrentLayout("WellColumn");
					break;
		
				case "Cmd_InferList":
					// 岩相/沉积相智能推理 - 从单井列表选择
					_factory.ShowWellInferenceDialog();
					break;
		
				case "Cmd_InferMap":
					// 岩相/沉积相智能推理 - 从工区平面图选择
					_factory.ShowMapBasedInferenceDialog();
					break;

				// 地震相智能分析模块命令
				case "Cmd_View3D":
					// 三维地震体视图 - 切换到地震体数据标签页
					_factory.ActivateDocumentInCurrentLayout("SeismicBody");
					break;
		
				case "Cmd_View2D":
					// 二维解释剖面视图 - 切换到地震解释剖面标签页
					_factory.ActivateDocumentInCurrentLayout("SeismicInterpretation");
					break;
		
				case "Cmd_InferTrace":
					// 沉积相智能推理 - 根据道号范围选择
					_factory.ShowSeismicTraceInferenceDialog();
					break;
		
				case "Cmd_InferCompare":
					// 沉积相智能推理 - 不同模型结果对比
					_factory.ShowModelComparisonDialog();
					break;

				// 地震数据标注 - 井旁地震相标注
				case "Cmd_WellSeismicFaciesAnnot":
					_factory.ActivateDocumentInCurrentLayout("SeismicInterpretation");
					_factory.StartSeismicAnnotationMode();
					break;

				case "Cmd_AIModelLib":
					// 智能模型库 - 显示智能模型列表窗口
					ShowAIModelLibraryWindow();
					break;

				case "Cmd_RectAnnotation":
					// 按矩形标注 - 启用矩形标注模式
					_factory.EnableRectangleAnnotationMode();
					break;

				case "Cmd_LithofaciesInference":
					// 岩相智能推理 - 切换单井柱状图到推理结果图片 (A5-1-res.jpg)
					_factory.RunLithofaciesInference();
					break;

				case "Cmd_SediFaciesInference":
					// 沉积相智能推理 - 切换联井剖面图到沉积相结果图片 (联井沉积相.jpg)
					_factory.RunSedimentaryFaciesInference();
					break;

				default:
					// 其他命令暂未实现
					break;
			}
		}

		/// <summary>
		/// 显示智能模型库窗口
		/// </summary>
		private void ShowAIModelLibraryWindow()
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
				&& desktop.MainWindow != null)
			{
				var window = new AIModelLibraryWindow();
				window.ShowDialog(desktop.MainWindow);
			}
		}
	}
}