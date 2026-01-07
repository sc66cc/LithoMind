using System;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 联井剖面图视图模型
	/// </summary>
	public partial class WellCorrelationViewModel : PageViewModelBase
	{
		/// <summary>
		/// 剖面名称
		/// </summary>
		[ObservableProperty]
		private string _sectionName = "联井剖面-01";

		/// <summary>
		/// 参与联井的井列表
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<CorrelationWell> _wells = new();

		/// <summary>
		/// 剖面图图像
		/// </summary>
		[ObservableProperty]
		private Bitmap? _sectionImage;

		/// <summary>
		/// 是否显示图片
		/// </summary>
		[ObservableProperty]
		private bool _hasImage;

		/// <summary>
		/// 缩放比例
		/// </summary>
		[ObservableProperty]
		private double _zoomLevel = 1.0;

		/// <summary>
		/// 缩放比例文本
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
		/// 显示层位标记
		/// </summary>
		[ObservableProperty]
		private bool _showHorizonMarkers = true;

		/// <summary>
		/// 显示沉积相
		/// </summary>
		[ObservableProperty]
		private bool _showFacies = true;

		/// <summary>
		/// 显示井选择器
		/// </summary>
		[ObservableProperty]
		private bool _showWellSelector;

		/// <summary>
		/// 当前是否显示原始图片（true）还是沉积相推理结果图片（false）
		/// </summary>
		[ObservableProperty]
		private bool _isShowingOriginalImage = true;

		public WellCorrelationViewModel()
		{
			Id = "WellCorrelation";
			Title = "联井剖面图";
			IconKey = "📈";
			Order = 7;

			// 初始化井列表
			InitializeWells();

			// 延迟加载示例图片，避免阻塞UI线程
			Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadSampleImage(),
				Avalonia.Threading.DispatcherPriority.Background);
		}

		/// <summary>
		/// 初始化井列表
		/// </summary>
		private void InitializeWells()
		{
			Wells.Add(new CorrelationWell { Name = "Well-A5-1", X = 0, IsSelected = true });
			Wells.Add(new CorrelationWell { Name = "Well-A6-5", X = 500, IsSelected = true });
			Wells.Add(new CorrelationWell { Name = "Well-A6-1", X = 1200, IsSelected = true });
			Wells.Add(new CorrelationWell { Name = "Well-A7-1", X = 1800, IsSelected = false });
			Wells.Add(new CorrelationWell { Name = "Well-A7-3", X = 2500, IsSelected = true });
		}

		/// <summary>
		/// 加载示例图片
		/// </summary>
		private void LoadSampleImage()
		{
			try
			{
				// 根据当前状态加载原始图片或沉积相推理结果图片
				string imageName = IsShowingOriginalImage ? "联井层序剖面.jpg" : "联井沉积相.jpg";
				var uri = new Uri($"avares://DeepTime.LithoMind.Desktop/Assets/Pics/{imageName}");
				var assets = Avalonia.Platform.AssetLoader.Open(uri);
				SectionImage = new Bitmap(assets);
				HasImage = true;
			}
			catch
			{
				HasImage = false;
			}
		}

		/// <summary>
		/// 执行沉积相智能推理 - 切换到沉积相结果图片
		/// </summary>
		[RelayCommand]
		public void RunSedimentaryFaciesInference()
		{
			// 切换到沉积相推理结果图片
			IsShowingOriginalImage = false;
			LoadSampleImage();
			Title = "联井剖面图 - 沉积相推理结果";
		}

		/// <summary>
		/// 重置为原始图片
		/// </summary>
		[RelayCommand]
		public void ResetToOriginalImage()
		{
			IsShowingOriginalImage = true;
			LoadSampleImage();
			Title = "联井剖面图";
		}

		/// <summary>
		/// 放大
		/// </summary>
		[RelayCommand]
		public void ZoomIn()
		{
			if (ZoomLevel < 5.0)
			{
				ZoomLevel = Math.Min(ZoomLevel * 1.2, 5.0);
				UpdateZoomText();
			}
		}

		/// <summary>
		/// 缩小
		/// </summary>
		[RelayCommand]
		public void ZoomOut()
		{
			if (ZoomLevel > 0.2)
			{
				ZoomLevel = Math.Max(ZoomLevel / 1.2, 0.2);
				UpdateZoomText();
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
			UpdateZoomText();
		}

		/// <summary>
		/// 更新缩放文本
		/// </summary>
		private void UpdateZoomText()
		{
			ZoomLevelText = $"{ZoomLevel * 100:F0}%";
		}

		/// <summary>
		/// 切换井的选中状态
		/// </summary>
		[RelayCommand]
		public void ToggleWell(CorrelationWell? well)
		{
			if (well != null)
			{
				well.IsSelected = !well.IsSelected;
				// 重新加载剖面图
				LoadSampleImage();
			}
		}

		/// <summary>
		/// 全选井
		/// </summary>
		[RelayCommand]
		public void SelectAllWells()
		{
			foreach (var well in Wells)
			{
				well.IsSelected = true;
			}
		}

		/// <summary>
		/// 取消全选
		/// </summary>
		[RelayCommand]
		public void DeselectAllWells()
		{
			foreach (var well in Wells)
			{
				well.IsSelected = false;
			}
		}

		/// <summary>
		/// 取消井选择
		/// </summary>
		[RelayCommand]
		public void CancelWellSelection()
		{
			ShowWellSelector = false;
		}

		/// <summary>
		/// 确认井选择
		/// </summary>
		[RelayCommand]
		public void ConfirmWellSelection()
		{
			ShowWellSelector = false;
			// 重新加载剖面图
			LoadSampleImage();
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
		/// 设置缩放
		/// </summary>
		public void SetZoom(double delta)
		{
			if (delta > 0)
				ZoomIn();
			else
				ZoomOut();
		}
	}

	/// <summary>
	/// 联井信息
	/// </summary>
	public partial class CorrelationWell : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private double _x;

		[ObservableProperty]
		private bool _isSelected = true;
	}
}
