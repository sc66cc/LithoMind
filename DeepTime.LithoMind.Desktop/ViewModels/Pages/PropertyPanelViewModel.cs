using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 属性窗口视图模型
	/// 用于显示JSON格式的数据（岩相、沉积相、地质描述信息等）
	/// </summary>
	public partial class PropertyPanelViewModel : PageViewModelBase
	{
		/// <summary>
		/// 当前显示的属性名称
		/// </summary>
		[ObservableProperty]
		private string _propertyTitle = "属性信息";

		/// <summary>
		/// 当前显示的JSON内容（格式化后的字符串）
		/// </summary>
		[ObservableProperty]
		private string _jsonContent = string.Empty;

		/// <summary>
		/// 当前选中的井名
		/// </summary>
		[ObservableProperty]
		private string _currentWellName = string.Empty;

		/// <summary>
		/// 当前选中的深度段
		/// </summary>
		[ObservableProperty]
		private string _currentDepthRange = string.Empty;

		/// <summary>
		/// 是否有数据显示
		/// </summary>
		[ObservableProperty]
		private bool _hasData;

		/// <summary>
		/// 深度段属性集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<DepthPropertyItem> _depthProperties = new();

		/// <summary>
		/// 预设的岩性选项
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<string> _lithologyOptions = new();

		/// <summary>
		/// 预设的沉积相选项
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<string> _sedimentaryFaciesOptions = new();

		public PropertyPanelViewModel()
		{
			Id = "PropertyPanel";
			Title = "属性窗口";
			IconKey = "📋";
			Order = 5;

			// 初始化预设选项
			InitializeOptions();

			// 加载示例数据
			LoadSampleData();
		}

		/// <summary>
		/// 初始化预设选项
		/// </summary>
		private void InitializeOptions()
		{
			// 岩性预设选项
			LithologyOptions.Add("粗砂岩");
			LithologyOptions.Add("中砂岩");
			LithologyOptions.Add("细砂岩");
			LithologyOptions.Add("粉砂岩");
			LithologyOptions.Add("泥岩");
			LithologyOptions.Add("砂质泥岩");
			LithologyOptions.Add("泥质砂岩");
			LithologyOptions.Add("灰岩");
			LithologyOptions.Add("白云岩");
			LithologyOptions.Add("页岩");
			LithologyOptions.Add("煤层");

			// 沉积相预设选项
			SedimentaryFaciesOptions.Add("河道");
			SedimentaryFaciesOptions.Add("分流河道");
			SedimentaryFaciesOptions.Add("河道边缘");
			SedimentaryFaciesOptions.Add("河口坝");
			SedimentaryFaciesOptions.Add("泛滥平原");
			SedimentaryFaciesOptions.Add("湖泊");
			SedimentaryFaciesOptions.Add("浅湖");
			SedimentaryFaciesOptions.Add("滨岨");
			SedimentaryFaciesOptions.Add("三角洲前缘");
			SedimentaryFaciesOptions.Add("深湖");
		}

		/// <summary>
		/// 加载示例数据
		/// </summary>
		private void LoadSampleData()
		{
			CurrentWellName = "A5-1";
			CurrentDepthRange = "4700m - 5000m";
			HasData = true;

			DepthProperties.Clear();
			
			// 添加示例深度段数据
			DepthProperties.Add(new DepthPropertyItem
			{
				DepthStart = 4700,
				DepthEnd = 4750,
				Lithology = "细砂岩",
				SedimentaryFacies = "河道",
				GeologicalDescription = "灰色细砂岩，分选中等，含少量泥质，见交错层理"
			});

			DepthProperties.Add(new DepthPropertyItem
			{
				DepthStart = 4750,
				DepthEnd = 4820,
				Lithology = "粉砂岩",
				SedimentaryFacies = "河道边缘",
				GeologicalDescription = "浅灰色粉砂岩，含较多泥质，见水平层理"
			});

			DepthProperties.Add(new DepthPropertyItem
			{
				DepthStart = 4820,
				DepthEnd = 4880,
				Lithology = "泥岩",
				SedimentaryFacies = "泛滥平原",
				GeologicalDescription = "深灰色泥岩，质纯，含少量植物碎片"
			});

			DepthProperties.Add(new DepthPropertyItem
			{
				DepthStart = 4880,
				DepthEnd = 4890,
				Lithology = "中砂岩",
				SedimentaryFacies = "分流河道",
				GeologicalDescription = "灰白色中砂岩，分选好，石英为主，见槽状交错层理"
			});

			DepthProperties.Add(new DepthPropertyItem
			{
				DepthStart = 4890,
				DepthEnd = 5000,
				Lithology = "细砂岩夹薄层泥岩",
				SedimentaryFacies = "河口坝",
				GeologicalDescription = "灰色细砂岩与薄层泥岩互层，见波状层理"
			});

			// 生成JSON内容用于显示
			UpdateJsonContent();
		}

		/// <summary>
		/// 更新JSON内容显示
		/// </summary>
		private void UpdateJsonContent()
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};

			var data = new
			{
				WellName = CurrentWellName,
				DepthRange = CurrentDepthRange,
				Properties = DepthProperties
			};

			JsonContent = JsonSerializer.Serialize(data, options);
		}

		/// <summary>
		/// 设置当前井的属性数据
		/// </summary>
		public void SetWellProperties(string wellName, ObservableCollection<DepthPropertyItem> properties)
		{
			CurrentWellName = wellName;
			DepthProperties = properties;
			HasData = properties.Count > 0;

			if (HasData)
			{
				var firstDepth = properties[0].DepthStart;
				var lastDepth = properties[properties.Count - 1].DepthEnd;
				CurrentDepthRange = $"{firstDepth}m - {lastDepth}m";
			}

			UpdateJsonContent();
		}

		/// <summary>
		/// 选择深度段
		/// </summary>
		[RelayCommand]
		public void SelectDepthProperty(DepthPropertyItem? item)
		{
			if (item != null)
			{
				PropertyTitle = $"深度段: {item.DepthStart}m - {item.DepthEnd}m";
			}
		}

		/// <summary>
		/// 刷新数据
		/// </summary>
		[RelayCommand]
		public void RefreshData()
		{
			LoadSampleData();
		}

		/// <summary>
		/// 清除数据
		/// </summary>
		[RelayCommand]
		public void ClearData()
		{
			CurrentWellName = string.Empty;
			CurrentDepthRange = string.Empty;
			DepthProperties.Clear();
			JsonContent = string.Empty;
			HasData = false;
			PropertyTitle = "属性信息";
		}
	}

	/// <summary>
	/// 深度段属性项 - 支持编辑
	/// </summary>
	public partial class DepthPropertyItem : ObservableObject
	{
		/// <summary>
		/// 起始深度（米）
		/// </summary>
		[ObservableProperty]
		private double _depthStart;

		/// <summary>
		/// 终止深度（米）
		/// </summary>
		[ObservableProperty]
		private double _depthEnd;

		/// <summary>
		/// 岩性
		/// </summary>
		[ObservableProperty]
		private string _lithology = string.Empty;

		/// <summary>
		/// 沉积相
		/// </summary>
		[ObservableProperty]
		private string _sedimentaryFacies = string.Empty;

		/// <summary>
		/// 地质描述
		/// </summary>
		[ObservableProperty]
		private string _geologicalDescription = string.Empty;

		/// <summary>
		/// 深度范围显示
		/// </summary>
		public string DepthRangeDisplay => $"{DepthStart:F1}m - {DepthEnd:F1}m";
	}
}
