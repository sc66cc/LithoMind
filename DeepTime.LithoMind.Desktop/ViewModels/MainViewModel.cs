using CommunityToolkit.Mvvm.ComponentModel;
using DeepTime.LithoMind.Desktop.ViewModels.Base;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;
using LithoMind.Core.Models.UI;
using LithoMind.Core.Services;
using LithoMind.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeepTime.LithoMind.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
	private readonly IUiConfigService _uiConfigService;
	private readonly CommandRegistry _cmdRegistry;
	private UiLayoutConfig? _cachedConfig;

	public ObservableCollection<PageViewModelBase> Pages { get; } = new();
	public ObservableCollection<MenuItemModel> MenuItems { get; } = new();
	public ObservableCollection<MenuItemModel> ToolbarItems { get; } = new();

	[ObservableProperty]
	private PageViewModelBase _currentPage;

	public MainViewModel(IUiConfigService uiConfigService, CommandRegistry cmdRegistry)
	{
		_uiConfigService = uiConfigService;
		_cmdRegistry = cmdRegistry;

		InitializePages();
		_ = LoadUiAsync();
	}

	private void InitializePages()
	{
		Pages.Add(new DataManagerViewModel());
		Pages.Add(new SingleWellViewModel());
		Pages.Add(new StratigraphyViewModel());
		Pages.Add(new SeismicViewModel());
		Pages.Add(new FusionViewModel());
		Pages.Add(new MappingViewModel());
		CurrentPage = Pages.First();
	}

	private async Task LoadUiAsync()
	{
		_cachedConfig = await _uiConfigService.LoadConfigAsync();
		if (_cachedConfig is null) return;

		MenuItems.Clear();
		foreach (var item in _cachedConfig.GlobalMenu)
		{
			BindCommandRecursively(item);
			MenuItems.Add(item);
		}

		RefreshToolbar(CurrentPage);
	}

	partial void OnCurrentPageChanged(PageViewModelBase value)
	{
		RefreshToolbar(value);
	}

	private void RefreshToolbar(PageViewModelBase page)
	{
		if (_cachedConfig is null || page is null) return;

		ToolbarItems.Clear();

		// 1. 加载当前页面特有的工具栏
		if (_cachedConfig.ContextToolbars.TryGetValue(page.GetType().Name, out var contextTools) && contextTools.Any())
		{
			foreach (var item in contextTools) AddToolbarItem(item);

			// 如果有上下文工具，且有全局工具，则加分割线
			if (_cachedConfig.GlobalToolbar.Any())
			{
				ToolbarItems.Add(new MenuItemModel { Type = MenuItemType.Separator });
			}
		}

		// 2. 加载全局工具栏
		foreach (var item in _cachedConfig.GlobalToolbar) AddToolbarItem(item);
	}

	private void AddToolbarItem(MenuItemModel item)
	{
		BindCommandRecursively(item);
		ToolbarItems.Add(item);
	}

	private void BindCommandRecursively(MenuItemModel item)
	{
		if (!string.IsNullOrEmpty(item.CommandId))
		{
			// 从注册表中查找命令并绑定
			item.Command = _cmdRegistry.Get(item.CommandId);
		}

		if (item.Children.Any())
		{
			foreach (var child in item.Children) BindCommandRecursively(child);
		}
	}
}