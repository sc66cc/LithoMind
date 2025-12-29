using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 地震体数据视图模型
	/// 显示SeismicBody.png并支持缩放
	/// </summary>
	public partial class SeismicBodyViewModel : PageViewModelBase
	{
		/// <summary>
		/// 地震体名称
		/// </summary>
		[ObservableProperty]
		private string _volumeName = "主力三维地震体";

		/// <summary>
		/// 地震体图像
		/// </summary>
		[ObservableProperty]
		private Bitmap? _seismicImage;

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
		/// 主测线范围
		/// </summary>
		[ObservableProperty]
		private string _inlineRange = "2200 - 2800";

		/// <summary>
		/// 联络线范围
		/// </summary>
		[ObservableProperty]
		private string _crosslineRange = "1500 - 2100";

		/// <summary>
		/// 时间范围
		/// </summary>
		[ObservableProperty]
		private string _timeRange = "1500ms - 3000ms";

		public SeismicBodyViewModel()
		{
			Id = "SeismicBody";
			Title = "地震体数据";
			IconKey = "🌊";
			Order = 1;

			// 延迟加载地震体图片，避免阻塞UI线程
			Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadSeismicBodyImage(),
				Avalonia.Threading.DispatcherPriority.Background);
		}

		/// <summary>
		/// 加载地震体图片
		/// </summary>
		private void LoadSeismicBodyImage()
		{
			try
			{
				var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/Pics/SeismicBody.png");
				var assets = Avalonia.Platform.AssetLoader.Open(uri);
				SeismicImage = new Bitmap(assets);
				HasImage = true;
			}
			catch
			{
				HasImage = false;
			}
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

		/// <summary>
		/// 加载指定地震体
		/// </summary>
		public void LoadSeismicVolume(string volumeName)
		{
			VolumeName = volumeName;
			Title = $"地震体数据 - {volumeName}";
			LoadSeismicBodyImage();
		}
	}
}
