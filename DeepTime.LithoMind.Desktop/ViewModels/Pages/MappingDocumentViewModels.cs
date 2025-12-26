using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 百分比转透明度转换器 (0-100 -> 0-1)
	/// </summary>
	public class PercentToOpacityConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is int percent)
				return percent / 100.0;
			return 1.0;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is double opacity)
				return (int)(opacity * 100);
			return 100;
		}
	}

	/// <summary>
	/// 布尔值转展开/收起图标转换器
	/// </summary>
	public class BoolToExpandIconConverter : IValueConverter
	{
		public static readonly BoolToExpandIconConverter Instance = new();

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is bool isExpanded)
				return isExpanded ? "▼" : "▲";
			return "▼";
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return value?.ToString() == "▼";
		}
	}
	/// <summary>
	/// 图例项
	/// </summary>
	public partial class LegendItem : ObservableObject
	{
		[ObservableProperty]
		private string _label = string.Empty;

		[ObservableProperty]
		private string _color = "#3498DB";

		[ObservableProperty]
		private double _minValue;

		[ObservableProperty]
		private double _maxValue;

		[ObservableProperty]
		private string _unit = "m";
	}

	/// <summary>
	/// 制图文档基类 - 支持缩放和拖拽的地图显示
	/// </summary>
	public abstract partial class MappingDocumentViewModelBase : PageViewModelBase
	{
		/// <summary>
		/// 地图图片
		/// </summary>
		[ObservableProperty]
		private Bitmap? _mapImage;

		/// <summary>
		/// 是否有图片
		/// </summary>
		[ObservableProperty]
		private bool _hasImage;

		/// <summary>
		/// 缩放级别
		/// </summary>
		[ObservableProperty]
		private double _zoomLevel = 1.0;

		/// <summary>
		/// 缩放级别文本
		/// </summary>
		[ObservableProperty]
		private string _zoomLevelText = "100%";

		/// <summary>
		/// 平移偏移X
		/// </summary>
		[ObservableProperty]
		private double _panOffsetX;

		/// <summary>
		/// 平移偏移Y
		/// </summary>
		[ObservableProperty]
		private double _panOffsetY;

		/// <summary>
		/// 图表标题
		/// </summary>
		[ObservableProperty]
		private string _chartTitle = string.Empty;

		/// <summary>
		/// 图表副标题
		/// </summary>
		[ObservableProperty]
		private string _chartSubtitle = string.Empty;

		/// <summary>
		/// 比例尺信息
		/// </summary>
		[ObservableProperty]
		private string _scaleInfo = "1:50000";

		/// <summary>
		/// 坐标信息
		/// </summary>
		[ObservableProperty]
		private string _coordinateInfo = "X: 0.00, Y: 0.00";

		/// <summary>
		/// 图例标题
		/// </summary>
		[ObservableProperty]
		private string _legendTitle = "图例";

		/// <summary>
		/// 图例是否展开
		/// </summary>
		[ObservableProperty]
		private bool _isLegendExpanded = true;

		/// <summary>
		/// 图例透明度 (0-100)
		/// </summary>
		[ObservableProperty]
		private int _legendOpacity = 90;

		/// <summary>
		/// 图例位置X
		/// </summary>
		[ObservableProperty]
		private double _legendPositionX = 20;

		/// <summary>
		/// 图例位置Y
		/// </summary>
		[ObservableProperty]
		private double _legendPositionY = 20;

		/// <summary>
		/// 图例项集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<LegendItem> _legendItems = new();

		protected MappingDocumentViewModelBase()
		{
			LoadMapImage();
			LoadLegendItems();
		}

		/// <summary>
		/// 加载地图图片（子类重写）
		/// </summary>
		protected abstract void LoadMapImage();

		/// <summary>
		/// 加载图例项（子类可重写）
		/// </summary>
		protected virtual void LoadLegendItems()
		{
			// 默认图例项
		}

		/// <summary>
		/// 切换图例展开/收起
		/// </summary>
		[RelayCommand]
		public void ToggleLegend()
		{
			IsLegendExpanded = !IsLegendExpanded;
		}

		/// <summary>
		/// 当缩放级别改变时更新文本
		/// </summary>
		partial void OnZoomLevelChanged(double value)
		{
			ZoomLevelText = $"{(int)(value * 100)}%";
		}

		/// <summary>
		/// 放大
		/// </summary>
		[RelayCommand]
		public void ZoomIn()
		{
			if (ZoomLevel < 5.0)
			{
				ZoomLevel = Math.Round(ZoomLevel * 1.2, 2);
			}
		}

		/// <summary>
		/// 缩小
		/// </summary>
		[RelayCommand]
		public void ZoomOut()
		{
			if (ZoomLevel > 0.1)
			{
				ZoomLevel = Math.Round(ZoomLevel / 1.2, 2);
			}
		}

		/// <summary>
		/// 重置缩放
		/// </summary>
		[RelayCommand]
		public void ResetZoom()
		{
			ZoomLevel = 1.0;
			PanOffsetX = 0;
			PanOffsetY = 0;
		}

		/// <summary>
		/// 滚轮缩放
		/// </summary>
		public void SetZoom(double delta)
		{
			if (delta > 0)
			{
				ZoomIn();
			}
			else
			{
				ZoomOut();
			}
		}

		/// <summary>
		/// 应用平移
		/// </summary>
		public void ApplyPan(double deltaX, double deltaY)
		{
			PanOffsetX += deltaX;
			PanOffsetY += deltaY;
		}

		/// <summary>
		/// 更新坐标信息
		/// </summary>
		public void UpdateCoordinate(double x, double y)
		{
			CoordinateInfo = $"X: {x:F2}, Y: {y:F2}";
		}
	}

	/// <summary>
	/// 砂体等厚图 ViewModel
	/// </summary>
	public partial class SandBodyThicknessViewModel : MappingDocumentViewModelBase
	{
		public SandBodyThicknessViewModel()
		{
			Id = "SandBodyThickness";
			Title = "砂体等厚图";
			IconKey = "📊";
			Order = 1;
			ChartTitle = "单一因素分析 - 砂体等厚图";
			ChartSubtitle = "XX区块 T1层段";
			LegendTitle = "砂体厚度(米)";
		}

		protected override void LoadMapImage()
		{
			try
			{
				var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/Pics/砂体等厚图.jpg");
				var assets = Avalonia.Platform.AssetLoader.Open(uri);
				MapImage = new Bitmap(assets);
				HasImage = true;
			}
			catch
			{
				HasImage = false;
			}
		}

		protected override void LoadLegendItems()
		{
			LegendItems.Add(new LegendItem { Label = ">20m", Color = "#8B0000", MinValue = 20, MaxValue = 100, Unit = "m" });
			LegendItems.Add(new LegendItem { Label = "15-20m", Color = "#FF4500", MinValue = 15, MaxValue = 20, Unit = "m" });
			LegendItems.Add(new LegendItem { Label = "10-15m", Color = "#FFA500", MinValue = 10, MaxValue = 15, Unit = "m" });
			LegendItems.Add(new LegendItem { Label = "5-10m", Color = "#FFD700", MinValue = 5, MaxValue = 10, Unit = "m" });
			LegendItems.Add(new LegendItem { Label = "2-5m", Color = "#90EE90", MinValue = 2, MaxValue = 5, Unit = "m" });
			LegendItems.Add(new LegendItem { Label = "<2m", Color = "#ADD8E6", MinValue = 0, MaxValue = 2, Unit = "m" });
		}
	}

	/// <summary>
	/// 砂地比图 ViewModel
	/// </summary>
	public partial class SandRatioViewModel : MappingDocumentViewModelBase
	{
		public SandRatioViewModel()
		{
			Id = "SandRatio";
			Title = "砂地比图";
			IconKey = "🟤";
			Order = 2;
			ChartTitle = "单一因素分析 - 砂地比分布图";
			ChartSubtitle = "XX区块 T1层段";
			LegendTitle = "砂地比(%)";
		}

		protected override void LoadMapImage()
		{
			try
			{
				var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/Pics/砂地比图.png");
				var assets = Avalonia.Platform.AssetLoader.Open(uri);
				MapImage = new Bitmap(assets);
				HasImage = true;
			}
			catch
			{
				HasImage = false;
			}
		}

		protected override void LoadLegendItems()
		{
			LegendItems.Add(new LegendItem { Label = ">80%", Color = "#8B4513", MinValue = 80, MaxValue = 100, Unit = "%" });
			LegendItems.Add(new LegendItem { Label = "60-80%", Color = "#CD853F", MinValue = 60, MaxValue = 80, Unit = "%" });
			LegendItems.Add(new LegendItem { Label = "40-60%", Color = "#DEB887", MinValue = 40, MaxValue = 60, Unit = "%" });
			LegendItems.Add(new LegendItem { Label = "20-40%", Color = "#F5DEB3", MinValue = 20, MaxValue = 40, Unit = "%" });
			LegendItems.Add(new LegendItem { Label = "<20%", Color = "#FFFACD", MinValue = 0, MaxValue = 20, Unit = "%" });
		}
	}

	/// <summary>
	/// 碳酸盐岩含量图 ViewModel
	/// </summary>
	public partial class CarbonateContentViewModel : MappingDocumentViewModelBase
	{
		public CarbonateContentViewModel()
		{
			Id = "CarbonateContent";
			Title = "碳酸盐岩含量";
			IconKey = "⚫";
			Order = 3;
			ChartTitle = "单一因素分析 - 碳酸盐岩含量分布图";
			ChartSubtitle = "XX区块 T1层段";
			LegendTitle = "碳酸盐岩含量(%)";
		}

		protected override void LoadMapImage()
		{
			try
			{
				var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/Pics/砂体等厚图.jpg");
				var assets = Avalonia.Platform.AssetLoader.Open(uri);
				MapImage = new Bitmap(assets);
				HasImage = true;
			}
			catch
			{
				HasImage = false;
			}
		}

		protected override void LoadLegendItems()
		{
			LegendItems.Add(new LegendItem { Label = ">50%", Color = "#4169E1", MinValue = 50, MaxValue = 100, Unit = "%" });
			LegendItems.Add(new LegendItem { Label = "30-50%", Color = "#6495ED", MinValue = 30, MaxValue = 50, Unit = "%" });
			LegendItems.Add(new LegendItem { Label = "15-30%", Color = "#87CEEB", MinValue = 15, MaxValue = 30, Unit = "%" });
			LegendItems.Add(new LegendItem { Label = "5-15%", Color = "#B0E0E6", MinValue = 5, MaxValue = 15, Unit = "%" });
			LegendItems.Add(new LegendItem { Label = "<5%", Color = "#E0FFFF", MinValue = 0, MaxValue = 5, Unit = "%" });
		}
	}

	/// <summary>
	/// 岩相古地理图 ViewModel
	/// </summary>
	public partial class LithofaciesPaleogeographyViewModel : MappingDocumentViewModelBase
	{
		public LithofaciesPaleogeographyViewModel()
		{
			Id = "LithofaciesPaleogeography";
			Title = "岩相古地理图";
			IconKey = "🗺️";
			Order = 4;
			ChartTitle = "岩相古地理综合分析图";
			ChartSubtitle = "XX区块 T1层段";
			LegendTitle = "沉积相";
		}

		protected override void LoadMapImage()
		{
			try
			{
				var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/Pics/重建结果.jpg");
				var assets = Avalonia.Platform.AssetLoader.Open(uri);
				MapImage = new Bitmap(assets);
				HasImage = true;
			}
			catch
			{
				HasImage = false;
			}
		}

		protected override void LoadLegendItems()
		{
			LegendItems.Add(new LegendItem { Label = "三角洲前缘", Color = "#FFD700", MinValue = 0, MaxValue = 0 });
			LegendItems.Add(new LegendItem { Label = "水下分流河道", Color = "#FFA500", MinValue = 0, MaxValue = 0 });
			LegendItems.Add(new LegendItem { Label = "浅海沙坡", Color = "#90EE90", MinValue = 0, MaxValue = 0 });
			LegendItems.Add(new LegendItem { Label = "浅海泥坡", Color = "#87CEEB", MinValue = 0, MaxValue = 0 });
			LegendItems.Add(new LegendItem { Label = "半深海", Color = "#4169E1", MinValue = 0, MaxValue = 0 });
			LegendItems.Add(new LegendItem { Label = "深海", Color = "#191970", MinValue = 0, MaxValue = 0 });
		}
	}
}
