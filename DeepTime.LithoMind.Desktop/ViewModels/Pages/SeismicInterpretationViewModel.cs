using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 地震解释剖面视图模型
	/// 显示InterpreWindowScale.png并支持缩放
	/// </summary>
	public partial class SeismicInterpretationViewModel : PageViewModelBase
	{
		/// <summary>
		/// 剖面名称
		/// </summary>
		[ObservableProperty]
		private string _sectionName = "主测线剖面 IL-2500";

		/// <summary>
		/// 剖面类型
		/// </summary>
		[ObservableProperty]
		private string _sectionType = "Inline";

		/// <summary>
		/// 剖面图像
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
		/// 剖面位置
		/// </summary>
		[ObservableProperty]
		private string _sectionPosition = "Inline 2500";

		/// <summary>
		/// 显示层位
		/// </summary>
		[ObservableProperty]
		private bool _showHorizons = true;

		/// <summary>
		/// 显示断层
		/// </summary>
		[ObservableProperty]
		private bool _showFaults = true;

		/// <summary>
		/// 显示井
		/// </summary>
		[ObservableProperty]
		private bool _showWells = true;

		public SeismicInterpretationViewModel()
		{
			Id = "SeismicInterpretation";
			Title = "地震解释剖面";
			IconKey = "📊";
			Order = 2;

			// 加载剖面图片
			LoadSectionImage();
		}

		/// <summary>
		/// 加载剖面图片
		/// </summary>
		private void LoadSectionImage()
		{
			try
			{
				var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/Pics/InterpreWindowScale.png");
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
		/// 加载指定剖面
		/// </summary>
		public void LoadSection(string sectionName, string sectionType)
		{
			SectionName = sectionName;
			SectionType = sectionType;
			Title = $"地震解释剖面 - {sectionName}";
			SectionPosition = $"{sectionType} {sectionName}";
			LoadSectionImage();
		}
	}
}
