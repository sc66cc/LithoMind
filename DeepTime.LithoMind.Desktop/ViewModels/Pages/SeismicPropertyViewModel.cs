using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 层位属性项
	/// </summary>
	public partial class HorizonPropertyItem : ObservableObject
	{
		/// <summary>
		/// 层位名称
		/// </summary>
		[ObservableProperty]
		private string _name = string.Empty;

		/// <summary>
		/// 层位颜色
		/// </summary>
		[ObservableProperty]
		private string _color = "#3498DB";

		/// <summary>
		/// 平均时间（毫秒）
		/// </summary>
		[ObservableProperty]
		private double _averageTime;

		/// <summary>
		/// 最小时间
		/// </summary>
		[ObservableProperty]
		private double _minTime;

		/// <summary>
		/// 最大时间
		/// </summary>
		[ObservableProperty]
		private double _maxTime;

		/// <summary>
		/// 追踪状态
		/// </summary>
		[ObservableProperty]
		private string _status = "已完成";

		/// <summary>
		/// 控制点数
		/// </summary>
		[ObservableProperty]
		private int _controlPointCount;

		/// <summary>
		/// 备注
		/// </summary>
		[ObservableProperty]
		private string _remarks = string.Empty;

		/// <summary>
		/// 是否可见
		/// </summary>
		[ObservableProperty]
		private bool _isVisible = true;
	}

	/// <summary>
	/// 地震属性窗口视图模型
	/// 显示层位信息（Horizon Information）
	/// </summary>
	public partial class SeismicPropertyViewModel : PageViewModelBase
	{
		/// <summary>
		/// 状态颜色转换器
		/// </summary>
		public static IValueConverter StatusToColorConverter { get; } = new StatusColorConverter();

		/// <summary>
		/// 属性标题
		/// </summary>
		[ObservableProperty]
		private string _propertyTitle = "层位信息";

		/// <summary>
		/// 当前选中的地震体
		/// </summary>
		[ObservableProperty]
		private string _currentVolumeName = string.Empty;

		/// <summary>
		/// 是否有数据
		/// </summary>
		[ObservableProperty]
		private bool _hasData;

		/// <summary>
		/// 层位属性集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<HorizonPropertyItem> _horizonProperties = new();

		/// <summary>
		/// 当前选中的层位
		/// </summary>
		[ObservableProperty]
		private HorizonPropertyItem? _selectedHorizon;

		public SeismicPropertyViewModel()
		{
			Id = "SeismicProperty";
			Title = "属性窗口";
			IconKey = "📋";
			Order = 3;

			// 加载示例数据
			LoadSampleData();
		}

		/// <summary>
		/// 加载示例数据
		/// </summary>
		private void LoadSampleData()
		{
			CurrentVolumeName = "主力三维地震体";
			HasData = true;

			HorizonProperties.Clear();

			HorizonProperties.Add(new HorizonPropertyItem
			{
				Name = "T1顶面",
				Color = "#E74C3C",
				AverageTime = 1850,
				MinTime = 1720,
				MaxTime = 1980,
				Status = "已完成",
				ControlPointCount = 2458,
				Remarks = "主力储层顶面，追踪质量良好",
				IsVisible = true
			});

			HorizonProperties.Add(new HorizonPropertyItem
			{
				Name = "T2顶面",
				Color = "#3498DB",
				AverageTime = 2100,
				MinTime = 1950,
				MaxTime = 2250,
				Status = "已完成",
				ControlPointCount = 2312,
				Remarks = "区域标志层",
				IsVisible = true
			});

			HorizonProperties.Add(new HorizonPropertyItem
			{
				Name = "T3顶面",
				Color = "#27AE60",
				AverageTime = 2350,
				MinTime = 2180,
				MaxTime = 2520,
				Status = "追踪中",
				ControlPointCount = 1876,
				Remarks = "断层发育区追踪困难",
				IsVisible = true
			});

			HorizonProperties.Add(new HorizonPropertyItem
			{
				Name = "T4顶面",
				Color = "#9B59B6",
				AverageTime = 2580,
				MinTime = 2420,
				MaxTime = 2740,
				Status = "待审核",
				ControlPointCount = 2156,
				Remarks = "深层目的层",
				IsVisible = false
			});
		}

		/// <summary>
		/// 选择层位
		/// </summary>
		[RelayCommand]
		public void SelectHorizon(HorizonPropertyItem? item)
		{
			if (item != null)
			{
				SelectedHorizon = item;
				PropertyTitle = $"层位信息 - {item.Name}";
			}
		}

		/// <summary>
		/// 切换层位可见性
		/// </summary>
		[RelayCommand]
		public void ToggleHorizonVisibility(HorizonPropertyItem? item)
		{
			if (item != null)
			{
				item.IsVisible = !item.IsVisible;
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
			CurrentVolumeName = string.Empty;
			HorizonProperties.Clear();
			HasData = false;
			PropertyTitle = "层位信息";
			SelectedHorizon = null;
		}

		/// <summary>
		/// 设置当前地震体的层位数据
		/// </summary>
		public void SetSeismicVolumeProperties(string volumeName, ObservableCollection<HorizonPropertyItem> properties)
		{
			CurrentVolumeName = volumeName;
			HorizonProperties = properties;
			HasData = properties.Count > 0;
		}
	}

	/// <summary>
	/// 状态颜色转换器
	/// </summary>
	public class StatusColorConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is string status)
			{
				return status switch
				{
					"已完成" => new SolidColorBrush(Color.Parse("#27AE60")),
					"追踪中" => new SolidColorBrush(Color.Parse("#F39C12")),
					"待审核" => new SolidColorBrush(Color.Parse("#9B59B6")),
					_ => new SolidColorBrush(Color.Parse("#95A5A6"))
				};
			}
			return new SolidColorBrush(Color.Parse("#95A5A6"));
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
