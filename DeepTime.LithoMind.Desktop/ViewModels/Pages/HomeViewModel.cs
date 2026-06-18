using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 快捷面板项
	/// </summary>
	public partial class QuickAccessItem : ObservableObject
	{
		[ObservableProperty]
		private string _title = string.Empty;

		[ObservableProperty]
		private string _description = string.Empty;

		[ObservableProperty]
		private string _icon = "📄";

		[ObservableProperty]
		private string _commandId = string.Empty;
	}

	/// <summary>
	/// 自定义视窗项
	/// </summary>
	public partial class CustomWidgetItem : ObservableObject
	{
		[ObservableProperty]
		private string _title = string.Empty;

		[ObservableProperty]
		private string _widgetType = "Chart"; // Chart, Table, Map, Custom

		[ObservableProperty]
		private bool _isVisible = true;

		[ObservableProperty]
		private int _row;

		[ObservableProperty]
		private int _column;

		[ObservableProperty]
		private int _rowSpan = 1;

		[ObservableProperty]
		private int _columnSpan = 1;
	}

	/// <summary>
	/// 首页 ViewModel
	/// </summary>
	public partial class HomeViewModel : PageViewModelBase
	{
		/// <summary>
		/// 快捷访问面板
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<QuickAccessItem> _quickAccessItems = new();

		/// <summary>
		/// 自定义视窗集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<CustomWidgetItem> _customWidgets = new();

		/// <summary>
		/// 是否显示欢迎信息
		/// </summary>
		[ObservableProperty]
		private bool _showWelcome = true;

		/// <summary>
		/// 是否显示快捷面板
		/// </summary>
		[ObservableProperty]
		private bool _showQuickAccess = true;

		/// <summary>
		/// 是否显示自定义视窗
		/// </summary>
		[ObservableProperty]
		private bool _showCustomWidgets = true;

		/// <summary>
		/// 欢迎消息
		/// </summary>
		[ObservableProperty]
		private string _welcomeMessage = "欢迎使用 LithoMind 地质数据分析平台";

		public HomeViewModel()
		{
			Id = "Home";
			Title = "首页";
			IconKey = "🏠";
			Order = 0;

			InitializeQuickAccessItems();
			InitializeCustomWidgets();
		}

		/// <summary>
		/// 初始化快捷访问项
		/// </summary>
		private void InitializeQuickAccessItems()
		{
			QuickAccessItems = new ObservableCollection<QuickAccessItem>
			{
				new QuickAccessItem
				{
					Title = "新建工程",
					Description = "创建新的地质分析工程",
					Icon = "📁",
					CommandId = "Cmd_NewProject"
				},
				new QuickAccessItem
				{
					Title = "打开工程",
					Description = "打开已有工程",
					Icon = "📂",
					CommandId = "Cmd_OpenProject"
				},
				new QuickAccessItem
				{
					Title = "导入数据",
					Description = "导入测井、地震等数据",
					Icon = "📥",
					CommandId = "Cmd_ImportData"
				},
				new QuickAccessItem
				{
					Title = "数据资源",
					Description = "浏览和管理数据资源",
					Icon = "🗂️",
					CommandId = "Module_DataMgr"
				},
				new QuickAccessItem
				{
					Title = "单井分析",
					Description = "单井相智能分析",
					Icon = "📊",
					CommandId = "Module_SingleWell"
				},
				new QuickAccessItem
				{
					Title = "地震解释",
					Description = "地震相智能分析",
					Icon = "🥓",
					CommandId = "Module_Seismic"
				},
				new QuickAccessItem
				{
					Title = "地层对比",
					Description = "等时地层格架构建",
					Icon = "🧱",
					CommandId = "Module_Strat"
				},
				new QuickAccessItem
				{
					Title = "编图制图",
					Description = "岩相古地理智能编图",
					Icon = "🗺️",
					CommandId = "Module_Mapping"
				}
			};
		}

		/// <summary>
		/// 初始化自定义视窗
		/// </summary>
		private void InitializeCustomWidgets()
		{
			CustomWidgets = new ObservableCollection<CustomWidgetItem>
			{
				new CustomWidgetItem
				{
					Title = "最近打开的工程",
					WidgetType = "List",
					Row = 0,
					Column = 0,
					ColumnSpan = 2
				},
				new CustomWidgetItem
				{
					Title = "工作进度",
					WidgetType = "Chart",
					Row = 1,
					Column = 0
				},
				new CustomWidgetItem
				{
					Title = "数据统计",
					WidgetType = "Table",
					Row = 1,
					Column = 1
				}
			};
		}

		/// <summary>
		/// 添加自定义视窗
		/// </summary>
		[RelayCommand]
		private void AddCustomWidget()
		{
			var newWidget = new CustomWidgetItem
			{
				Title = $"新视窗 {CustomWidgets.Count + 1}",
				WidgetType = "Custom",
				Row = CustomWidgets.Count / 2,
				Column = CustomWidgets.Count % 2
			};
			CustomWidgets.Add(newWidget);
		}

		/// <summary>
		/// 移除自定义视窗
		/// </summary>
		[RelayCommand]
		private void RemoveCustomWidget(CustomWidgetItem widget)
		{
			CustomWidgets.Remove(widget);
		}

		/// <summary>
		/// 切换欢迎信息显示
		/// </summary>
		[RelayCommand]
		private void ToggleWelcome()
		{
			ShowWelcome = !ShowWelcome;
		}

		/// <summary>
		/// 切换快捷面板显示
		/// </summary>
		[RelayCommand]
		private void ToggleQuickAccess()
		{
			ShowQuickAccess = !ShowQuickAccess;
		}

		/// <summary>
		/// 切换自定义视窗显示
		/// </summary>
		[RelayCommand]
		private void ToggleCustomWidgets()
		{
			ShowCustomWidgets = !ShowCustomWidgets;
		}
	}
}
