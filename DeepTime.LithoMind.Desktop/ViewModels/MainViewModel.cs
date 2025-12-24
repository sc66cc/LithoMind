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
		public void SwitchModule(string? moduleJsonId)
		{
			if (string.IsNullOrEmpty(moduleJsonId) || _uiConfig == null) return;
		
			// 直接使用JSON中的模块ID获取菜单
			var moduleMenus = _uiConfig.GetModuleMenus(moduleJsonId);
			CurrentModuleMenus = moduleMenus ?? new List<MenuItemModel>();
		
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
			catch
			{
				// 布局切换失败时静默处理
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
			// TODO: 实现命令执行逻辑
		}
	}
}