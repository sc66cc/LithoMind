using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 搜索结果类型
	/// </summary>
	public enum SearchResultType
	{
		File,      // 文件
		Data,      // 数据
		Layer,     // 图层
		Function,  // 功能/菜单
		Module     // 模块
	}

	/// <summary>
	/// 搜索结果项
	/// </summary>
	public partial class SearchResultItem : ObservableObject
	{
		[ObservableProperty]
		private string _title = string.Empty;

		[ObservableProperty]
		private string _description = string.Empty;

		[ObservableProperty]
		private string _path = string.Empty;

		[ObservableProperty]
		private string _icon = "📄";

		[ObservableProperty]
		private SearchResultType _type;

		[ObservableProperty]
		private string _typeText = string.Empty;

		[ObservableProperty]
		private string _commandId = string.Empty;

		[ObservableProperty]
		private object? _tag;
	}

	/// <summary>
	/// 全局搜索 ViewModel
	/// </summary>
	public partial class GlobalSearchViewModel : ViewModelBase
	{
		/// <summary>
		/// 搜索关键词
		/// </summary>
		[ObservableProperty]
		private string _searchKeyword = string.Empty;

		/// <summary>
		/// 搜索结果
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<SearchResultItem> _searchResults = new();

		/// <summary>
		/// 当前选中的结果
		/// </summary>
		[ObservableProperty]
		private SearchResultItem? _selectedResult;

		/// <summary>
		/// 是否正在搜索
		/// </summary>
		[ObservableProperty]
		private bool _isSearching;

		/// <summary>
		/// 搜索结果数量
		/// </summary>
		[ObservableProperty]
		private string _resultCount = "0 个结果";

		/// <summary>
		/// 是否显示所有类型
		/// </summary>
		[ObservableProperty]
		private bool _showAllTypes = true;

		/// <summary>
		/// 只显示文件
		/// </summary>
		[ObservableProperty]
		private bool _showFilesOnly;

		/// <summary>
		/// 只显示功能
		/// </summary>
		[ObservableProperty]
		private bool _showFunctionsOnly;

		/// <summary>
		/// 只显示图层
		/// </summary>
		[ObservableProperty]
		private bool _showLayersOnly;

		/// <summary>
		/// 搜索结果选择事件
		/// </summary>
		public event Action<SearchResultItem>? ResultSelected;

		public GlobalSearchViewModel()
		{
			Id = "GlobalSearch";
			Title = "全局搜索";
		}

		/// <summary>
		/// 当搜索关键词改变时
		/// </summary>
		partial void OnSearchKeywordChanged(string value)
		{
			PerformSearch();
		}

		/// <summary>
		/// 执行搜索
		/// </summary>
		[RelayCommand]
		private async void PerformSearch()
		{
			IsSearching = true;
			SearchResults.Clear();

			// 模拟搜索延迟
			await System.Threading.Tasks.Task.Delay(100);

			if (string.IsNullOrWhiteSpace(SearchKeyword))
			{
				IsSearching = false;
				ResultCount = "0 个结果";
				return;
			}

			var keyword = SearchKeyword.ToLower();

			// 搜索模块
			if (ShowAllTypes || ShowFunctionsOnly)
			{
				SearchModules(keyword);
			}

			// 搜索功能/菜单
			if (ShowAllTypes || ShowFunctionsOnly)
			{
				SearchFunctions(keyword);
			}

			// 搜索文件
			if (ShowAllTypes || ShowFilesOnly)
			{
				SearchFiles(keyword);
			}

			// 搜索图层
			if (ShowAllTypes || ShowLayersOnly)
			{
				SearchLayers(keyword);
			}

			// 搜索数据
			if (ShowAllTypes || ShowFilesOnly)
			{
				SearchData(keyword);
			}

			IsSearching = false;
			ResultCount = $"{SearchResults.Count} 个结果";
		}

		/// <summary>
		/// 搜索模块
		/// </summary>
		private void SearchModules(string keyword)
		{
			var modules = new[]
			{
				("首页", "🏠", "Module_Home", "快速访问和自定义视窗"),
				("多源数据管理", "📂", "Module_DataMgr", "数据导入、预览和管理"),
				("等时地层格架构建", "🧱", "Module_Strat", "层序地层分析和对比"),
				("单井相智能分析", "📊", "Module_SingleWell", "单井柱状图和岩相推理"),
				("地震相智能分析", "🥓", "Module_Seismic", "地震数据解释和相分析"),
				("岩相古地理智能编图", "🗺️", "Module_Mapping", "制图和空间分析")
			};

			foreach (var (name, icon, cmdId, desc) in modules)
			{
				if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
				{
					SearchResults.Add(new SearchResultItem
					{
						Title = name,
						Description = desc,
						Path = "模块",
						Icon = icon,
						Type = SearchResultType.Module,
						TypeText = "模块",
						CommandId = cmdId
					});
				}
			}
		}

		/// <summary>
		/// 搜索功能/菜单
		/// </summary>
		private void SearchFunctions(string keyword)
		{
			var functions = new[]
			{
				("新建工程", "创建新的地质分析工程", "Cmd_NewProject", "📁"),
				("打开工程", "打开已有工程", "Cmd_OpenProject", "📂"),
				("导入测井数据", "导入 LAS 格式测井曲线", "Cmd_ImportLAS", "📊"),
				("导入地震数据", "导入 SEG-Y 格式地震数据", "Cmd_ImportSeismicRaw", "🥓"),
				("岩相智能推理", "基于测井数据的岩相识别", "Cmd_LithofaciesInference", "🤖"),
				("沉积相智能推理", "基于测井数据的沉积相识别", "Cmd_SediFaciesInference", "🤖"),
				("地震相智能推理", "基于地震数据的地震相识别", "Cmd_SeismicFaciesByTrace", "🤖"),
				("新建单井柱状图", "创建单井综合柱状图", "Cmd_NewWellColumn", "📊"),
				("新建联井剖面", "创建联井对比剖面", "Cmd_CrossSectionFromList", "📐"),
				("层位解释", "地震层位追踪和解释", "Cmd_ImportSeismicHorizon_Seismic", "🎯"),
				("智能制图工具", "边界生成、插值等工具", "Cmd_SmartBoundary", "🎨"),
				("数据标注工具", "测井、地震数据标注", "Cmd_RectAnnotation", "✏️"),
				("导出岩相古地理图", "导出制图结果", "Cmd_ExportPaleoMap", "💾")
			};

			foreach (var (name, desc, cmdId, icon) in functions)
			{
				if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
				    desc.Contains(keyword, StringComparison.OrdinalIgnoreCase))
				{
					SearchResults.Add(new SearchResultItem
					{
						Title = name,
						Description = desc,
						Path = "功能",
						Icon = icon,
						Type = SearchResultType.Function,
						TypeText = "功能",
						CommandId = cmdId
					});
				}
			}
		}

		/// <summary>
		/// 搜索文件
		/// </summary>
		private void SearchFiles(string keyword)
		{
			var files = new[]
			{
				("Well_A1_GR.las", "测井数据/Well_A1_GR.las", "测井曲线文件", "📊"),
				("Well_A2_DEN.las", "测井数据/Well_A2_DEN.las", "测井曲线文件", "📊"),
				("3D_Survey_Area1.sgy", "地震数据/3D_Survey_Area1.sgy", "三维地震数据", "🥓"),
				("构造图.shp", "地质图件/构造图.shp", "构造图矢量数据", "🗺️"),
				("沉积相图.shp", "地质图件/沉积相图.shp", "沉积相分布图", "🗺️"),
				("古地貌图.shp", "地质图件/古地貌图.shp", "古地貌恢复图", "🗺️"),
				("水系图.shp", "地质图件/水系图.shp", "古水系分布图", "🗺️"),
				("项目报告.pdf", "文档资料/项目报告.pdf", "项目总结报告", "📕"),
				("分析测试报告.pdf", "文档资料/分析测试报告.pdf", "实验室测试报告", "📕"),
				("岩心照片.jpg", "岩心资料/岩心照片.jpg", "岩心照片", "🖼️"),
				("薄片分析报告.pdf", "岩心资料/薄片分析报告.pdf", "薄片鉴定报告", "📕")
			};

			foreach (var (name, path, desc, icon) in files)
			{
				if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
				    path.Contains(keyword, StringComparison.OrdinalIgnoreCase))
				{
					SearchResults.Add(new SearchResultItem
					{
						Title = name,
						Description = desc,
						Path = path,
						Icon = icon,
						Type = SearchResultType.File,
						TypeText = "文件",
						CommandId = "Cmd_OpenFile"
					});
				}
			}
		}

		/// <summary>
		/// 搜索图层
		/// </summary>
		private void SearchLayers(string keyword)
		{
			var layers = new[]
			{
				("井位分布图层", "显示井位平面分布", "🎯"),
				("构造等高线图层", "地层构造形态", "📏"),
				("断层分布图层", "断层位置和属性", "⚡"),
				("储层边界图层", "储层空间范围", "🟢"),
				("沉积相分布图层", "沉积相类型分布", "🎨"),
				("砂体厚度图层", "砂体厚度等值线", "📐"),
				("古地貌图层", "沉积前地形地貌", "⛰️"),
				("水系分布图层", "古水系位置", "💧")
			};

			foreach (var (name, desc, icon) in layers)
			{
				if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
				    desc.Contains(keyword, StringComparison.OrdinalIgnoreCase))
				{
					SearchResults.Add(new SearchResultItem
					{
						Title = name,
						Description = desc,
						Path = "图层管理",
						Icon = icon,
						Type = SearchResultType.Layer,
						TypeText = "图层",
						CommandId = "Cmd_ToggleLayer"
					});
				}
			}
		}

		/// <summary>
		/// 搜索数据
		/// </summary>
		private void SearchData(string keyword)
		{
			var dataItems = new[]
			{
				("Well-A1", "研究区 A1 井", "测井数据", "📊"),
				("Well-A2", "研究区 A2 井", "测井数据", "📊"),
				("Well-B1", "研究区 B1 井", "测井数据", "📊"),
				("T1 层位", "目标层位 T1", "层位数据", "🎯"),
				("T2 层位", "目标层位 T2", "层位数据", "🎯"),
				("HST 体系域", "高位体系域", "层序地层", "🧱"),
				("TST 体系域", "湖侵体系域", "层序地层", "🧱"),
				("河道砂", "河道沉积砂体", "沉积相", "🏞️"),
				("三角洲前缘", "三角洲前缘相带", "沉积相", "🏖️")
			};

			foreach (var (name, desc, category, icon) in dataItems)
			{
				if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
				    desc.Contains(keyword, StringComparison.OrdinalIgnoreCase))
				{
					SearchResults.Add(new SearchResultItem
					{
						Title = name,
						Description = desc,
						Path = category,
						Icon = icon,
						Type = SearchResultType.Data,
						TypeText = "数据",
						CommandId = "Cmd_ViewData"
					});
				}
			}
		}

		/// <summary>
		/// 选择搜索结果
		/// </summary>
		[RelayCommand]
		private void SelectResult(SearchResultItem? item)
		{
			if (item != null)
			{
				SelectedResult = item;
				ResultSelected?.Invoke(item);
			}
		}

		/// <summary>
		/// 设置筛选类型 - 全部
		/// </summary>
		[RelayCommand]
		private void ShowAll()
		{
			ShowAllTypes = true;
			ShowFilesOnly = false;
			ShowFunctionsOnly = false;
			ShowLayersOnly = false;
			PerformSearch();
		}

		/// <summary>
		/// 设置筛选类型 - 仅文件
		/// </summary>
		[RelayCommand]
		private void ShowFiles()
		{
			ShowAllTypes = false;
			ShowFilesOnly = true;
			ShowFunctionsOnly = false;
			ShowLayersOnly = false;
			PerformSearch();
		}

		/// <summary>
		/// 设置筛选类型 - 仅功能
		/// </summary>
		[RelayCommand]
		private void ShowFunctions()
		{
			ShowAllTypes = false;
			ShowFilesOnly = false;
			ShowFunctionsOnly = true;
			ShowLayersOnly = false;
			PerformSearch();
		}

		/// <summary>
		/// 设置筛选类型 - 仅图层
		/// </summary>
		[RelayCommand]
		private void ShowLayers()
		{
			ShowAllTypes = false;
			ShowFilesOnly = false;
			ShowFunctionsOnly = false;
			ShowLayersOnly = true;
			PerformSearch();
		}

		/// <summary>
		/// 清空搜索
		/// </summary>
		[RelayCommand]
		private void ClearSearch()
		{
			SearchKeyword = string.Empty;
			SearchResults.Clear();
			SelectedResult = null;
			ResultCount = "0 个结果";
		}
	}
}
