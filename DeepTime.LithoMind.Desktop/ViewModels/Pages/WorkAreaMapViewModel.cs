using System;
using System.IO;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 工区平面图视图模型
	/// 支持图像显示、缩放、拖拽和图层控制功能
	/// </summary>
	public partial class WorkAreaMapViewModel : PageViewModelBase
	{
		/// <summary>
		/// 工区平面图图像
		/// </summary>
		[ObservableProperty]
		private Bitmap? _mapImage;

		/// <summary>
		/// 当前缩放比例（1.0 = 100%）
		/// </summary>
		[ObservableProperty]
		private double _zoomLevel = 1.0;

		/// <summary>
		/// 缩放比例显示文本
		/// </summary>
		[ObservableProperty]
		private string _zoomLevelText = "100%";

		/// <summary>
		/// 平移偏移量 X
		/// </summary>
		[ObservableProperty]
		private double _panOffsetX;

		/// <summary>
		/// 平移偏移量 Y
		/// </summary>
		[ObservableProperty]
		private double _panOffsetY;

		/// <summary>
		/// 是否正在加载
		/// </summary>
		[ObservableProperty]
		private bool _isLoading;

		/// <summary>
		/// 错误信息
		/// </summary>
		[ObservableProperty]
		private string _errorMessage = string.Empty;

		/// <summary>
		/// 图像是否加载成功
		/// </summary>
		[ObservableProperty]
		private bool _isImageLoaded;

		/// <summary>
		/// 图像原始宽度
		/// </summary>
		[ObservableProperty]
		private int _imageWidth;

		/// <summary>
		/// 图像原始高度
		/// </summary>
		[ObservableProperty]
		private int _imageHeight;

		/// <summary>
		/// 缩放最小值
		/// </summary>
		private const double MinZoom = 0.1;

		/// <summary>
		/// 缩放最大值
		/// </summary>
		private const double MaxZoom = 10.0;

		/// <summary>
		/// 缩放步进值
		/// </summary>
		private const double ZoomStep = 0.1;

		/// <summary>
		/// 图层可见性字典 - 键为节点路径，值为是否可见
		/// </summary>
		private readonly Dictionary<string, bool> _layerVisibility = new();

		public WorkAreaMapViewModel()
		{
			Id = "WorkAreaMap";
			Title = "工区平面图";
			IconKey = "🗺️";
			Order = 4;

			// 加载工区平面图
			LoadWorkAreaMap();
		}

		/// <summary>
		/// 加载工区平面图 - 使用Avalonia资源加载方式
		/// </summary>
		private void LoadWorkAreaMap()
		{
			IsLoading = true;
			ErrorMessage = string.Empty;

			try
			{
				// 使用Avalonia的嵌入资源加载方式
				var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/Pics/工区平面图.png");
				var assets = Avalonia.Platform.AssetLoader.Open(uri);
				MapImage = new Bitmap(assets);
				ImageWidth = MapImage.PixelSize.Width;
				ImageHeight = MapImage.PixelSize.Height;
				IsImageLoaded = true;
			}
			catch (Exception ex)
			{
				ErrorMessage = $"加载工区平面图失败: {ex.Message}";
				IsImageLoaded = false;
			}
			finally
			{
				IsLoading = false;
			}
		}

		/// <summary>
		/// 放大图像
		/// </summary>
		[RelayCommand]
		public void ZoomIn()
		{
			if (ZoomLevel < MaxZoom)
			{
				ZoomLevel = Math.Min(ZoomLevel + ZoomStep, MaxZoom);
				UpdateZoomLevelText();
			}
		}

		/// <summary>
		/// 缩小图像
		/// </summary>
		[RelayCommand]
		public void ZoomOut()
		{
			if (ZoomLevel > MinZoom)
			{
				ZoomLevel = Math.Max(ZoomLevel - ZoomStep, MinZoom);
				UpdateZoomLevelText();
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
			UpdateZoomLevelText();
		}

		/// <summary>
		/// 适应窗口大小
		/// </summary>
		[RelayCommand]
		public void FitToWindow()
		{
			// 重置平移
			PanOffsetX = 0;
			PanOffsetY = 0;
			// 缩放会由View层根据实际容器大小计算
			ZoomLevel = 1.0;
			UpdateZoomLevelText();
		}

		/// <summary>
		/// 设置缩放比例（由鼠标滚轮调用）
		/// </summary>
		public void SetZoomLevel(double delta, Point mousePosition)
		{
			var oldZoom = ZoomLevel;
			var newZoom = delta > 0 
				? Math.Min(ZoomLevel * 1.1, MaxZoom) 
				: Math.Max(ZoomLevel / 1.1, MinZoom);

			// 计算以鼠标位置为中心的缩放偏移调整
			if (Math.Abs(oldZoom - newZoom) > 0.001)
			{
				var zoomRatio = newZoom / oldZoom;
				PanOffsetX = mousePosition.X - (mousePosition.X - PanOffsetX) * zoomRatio;
				PanOffsetY = mousePosition.Y - (mousePosition.Y - PanOffsetY) * zoomRatio;
			}

			ZoomLevel = newZoom;
			UpdateZoomLevelText();
		}

		/// <summary>
		/// 应用平移偏移
		/// </summary>
		public void ApplyPanOffset(double deltaX, double deltaY)
		{
			PanOffsetX += deltaX;
			PanOffsetY += deltaY;
		}

		/// <summary>
		/// 更新缩放显示文本
		/// </summary>
		private void UpdateZoomLevelText()
		{
			ZoomLevelText = $"{ZoomLevel * 100:F0}%";
		}

		/// <summary>
		/// 设置图层可见性
		/// </summary>
		public void SetLayerVisibility(string layerPath, bool isVisible)
		{
			_layerVisibility[layerPath] = isVisible;
			
			// TODO: 在实际应用中，这里会触发图层的显示/隐藏
			// 当前原型阶段仅记录状态
			OnLayerVisibilityChanged(layerPath, isVisible);
		}

		/// <summary>
		/// 获取图层可见性
		/// </summary>
		public bool GetLayerVisibility(string layerPath)
		{
			return _layerVisibility.TryGetValue(layerPath, out var visible) ? visible : true;
		}

		/// <summary>
		/// 图层可见性变化事件
		/// </summary>
		public event Action<string, bool>? LayerVisibilityChanged;

		/// <summary>
		/// 触发图层可见性变化事件
		/// </summary>
		protected virtual void OnLayerVisibilityChanged(string layerPath, bool isVisible)
		{
			LayerVisibilityChanged?.Invoke(layerPath, isVisible);
		}

		/// <summary>
		/// 刷新工区平面图
		/// </summary>
		[RelayCommand]
		public void RefreshMap()
		{
			LoadWorkAreaMap();
		}
	}
}
