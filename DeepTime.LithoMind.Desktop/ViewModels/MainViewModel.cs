using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.Layouts;
using DeepTime.LithoMind.Desktop.ViewModels.Base;
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

			// 默认启动进入"数据管理"
			SwitchModule("Module_DataMgr");
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
						PropertyNameCaseInsensitive = true // 允许 JSON key 大小写不敏感
					};

					_uiConfig = JsonSerializer.Deserialize<UiLayoutConfig>(json, options);

					if (_uiConfig != null)
					{
						// [修复] 使用 PascalCase 属性 GlobalMenu
						GlobalMenus = CleanMenus(_uiConfig.GlobalMenu);
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"配置加载失败: {ex.Message}");
				_uiConfig = new UiLayoutConfig();
			}
		}

		[RelayCommand]
		public void SwitchModule(string? moduleJsonId)
		{
			if (string.IsNullOrEmpty(moduleJsonId) || _uiConfig == null) return;

			// [修复] 适配 Dictionary 结构的 ContextToolbars
			// 如果 JSON 里的 ID 是 "Module_DataMgr"，我们需要在字典里查找对应的菜单列表
			// 注意：这里取决于你的 JSON 结构和 Core 模型是否一致。
			// 如果 Core 模型定义 ContextToolbars 是 Dictionary，我们尝试用 Key 获取。

			if (_uiConfig.ContextToolbars.TryGetValue(moduleJsonId, out var items))
			{
				CurrentModuleMenus = CleanMenus(items);
			}
			else
			{
				// 如果字典里找不到，或者你其实还在用 List 结构但 Core 没改过来，这里做一个容错
				// 暂时给个空列表，防止报错
				CurrentModuleMenus = new List<MenuItemModel>();
			}

			// 更新 Dock 布局
			string factoryId = MapJsonIdToFactoryId(moduleJsonId);
			UpdateDockLayout(factoryId);
		}

		private void UpdateDockLayout(string factoryId)
		{
			try
			{
				if (Layout != null && Layout.Close.CanExecute(null))
				{
					Layout.Close.Execute(null);
				}

				var newLayout = _factory.CreateLayoutForModule(factoryId);
				if (newLayout != null)
				{
					_factory.InitLayout(newLayout);
					Layout = newLayout;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Dock布局切换错误: {ex.Message}");
			}
		}

		// [修复] 属性名改为 PascalCase (Icon, Children)
		private List<MenuItemModel> CleanMenus(List<MenuItemModel>? menus)
		{
			if (menus == null) return new List<MenuItemModel>();
			foreach (var item in menus)
			{
				item.Icon = null; // 强制清空 Icon
				if (item.Children != null)
				{
					CleanMenus(item.Children); // 递归清理
				}
			}
			return menus;
		}

		private string MapJsonIdToFactoryId(string jsonId)
		{
			if (jsonId.Contains("DataMgr")) return "DataManager";
			if (jsonId.Contains("SingleWell")) return "SingleWell";
			if (jsonId.Contains("Seismic")) return "Seismic";
			if (jsonId.Contains("Strat")) return "Stratigraphy";
			if (jsonId.Contains("Fusion")) return "Fusion";
			if (jsonId.Contains("Mapping")) return "Mapping";
			return "DataManager";
		}

		[RelayCommand]
		public void ExecuteMenu(string commandId)
		{
			System.Diagnostics.Debug.WriteLine($"执行命令: {commandId}");
		}
	}
}