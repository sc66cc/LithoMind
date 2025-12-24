using System;
using System.IO;
using System.Linq; 
using System.Text.Json;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.Layouts;
using DeepTime.LithoMind.Desktop.Models;
using DeepTime.LithoMind.Desktop.ViewModels.Base;
using Avalonia.Platform;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace DeepTime.LithoMind.Desktop.ViewModels
{
	public partial class MainViewModel : ViewModelBase
	{
		private readonly LithoMindDockFactory _factory; // 改为具体类型以便调用自定义方法
		private UiLayoutConfig _uiConfig;
		//
		private List<MenuItemModel>? _globalMenus;
		public List<MenuItemModel>? GlobalMenus
		{
			get => _globalMenus;
			set => SetProperty(ref _globalMenus, value);
		}

		[ObservableProperty]
		private IRootDock? _layout;
		[ObservableProperty]
		private List<MenuItemModel> _currentModuleMenus;
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
					using (var stream = AssetLoader.Open(uri))
					using (var reader = new StreamReader(stream))
					{
						var json = reader.ReadToEnd();
						_uiConfig = JsonSerializer.Deserialize<UiLayoutConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
						if (_uiConfig?.globalMenu != null)
						{
							GlobalMenus = _uiConfig.globalMenu;
						}
					}
				}
			}
			catch (Exception ex)
			{
				// 如果读不到文件，初始化一个空的防止报错
				_uiConfig = new UiLayoutConfig { moduleToolbars = new List<ModuleToolbar>() };
				System.Diagnostics.Debug.WriteLine("Error loading config: " + ex.Message);
			}
		}

		[RelayCommand]
		public void SwitchModule(string moduleJsonId)
		{
			if (_uiConfig == null) return;

			// 1. 查找 JSON 中对应的菜单配置
			var targetToolbar = _uiConfig.moduleToolbars.FirstOrDefault(t => t.id == moduleJsonId);

			if (targetToolbar != null)
			{
				CurrentModuleMenus = targetToolbar.items;

				string simpleId = moduleJsonId.Replace("Module_", "").Replace("DataMgr", "DataManager");

				var newLayout = _factory.CreateLayoutForModule(simpleId);
				_factory.InitLayout(newLayout);
				Layout = newLayout;
			}
		}
		[RelayCommand]
		public void ExecuteMenu(string commandId)
		{
			System.Diagnostics.Debug.WriteLine($"触发菜单命令: {commandId}");
			// 未来这里可以对接 CommandRegistry
		}
	}
}