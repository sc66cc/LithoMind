using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

		/// <summary>
		/// 是否显示道号范围选择对话框
		/// </summary>
		[ObservableProperty]
		private bool _showTraceRangeDialog;

		/// <summary>
		/// xLine起始道号
		/// </summary>
		[ObservableProperty]
		private int _xLineStart = 250;

		/// <summary>
		/// xLine结束道号
		/// </summary>
		[ObservableProperty]
		private int _xLineEnd = 300;

		/// <summary>
		/// inLine起始道号
		/// </summary>
		[ObservableProperty]
		private int _inLineStart = 250;

		/// <summary>
		/// inLine结束道号
		/// </summary>
		[ObservableProperty]
		private int _inLineEnd = 300;

		/// <summary>
		/// 当前是否显示原始图片（true）还是推理结果图片（false）
		/// </summary>
		[ObservableProperty]
		private bool _isShowingOriginalImage = true;

		/// <summary>
		/// 地震相推理结果集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<SeismicFaciesInferenceResult> _inferenceResults = new();

		/// <summary>
		/// 智能推理完成事件 - 通知属性面板更新
		/// </summary>
		public event Action<string, ObservableCollection<SeismicFaciesInferenceResult>>? SeismicInferenceCompleted;

		public SeismicInterpretationViewModel()
		{
			Id = "SeismicInterpretation";
			Title = "地震解释剖面";
			IconKey = "📊";
			Order = 2;

			// 延迟加载剖面图片，避免阻塞UI线程
			Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadSectionImage(),
				Avalonia.Threading.DispatcherPriority.Background);
		}

		/// <summary>
		/// 加载剖面图片
		/// </summary>
		private void LoadSectionImage()
		{
			try
			{
				// 根据当前状态加载原始图片或推理结果图片
				string imageName = IsShowingOriginalImage ? "InterpreWindowScale.png" : "InterpreWindowScaleRES.png";
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

		#region 智能推理功能

		/// <summary>
		/// 显示道号范围选择对话框
		/// </summary>
		[RelayCommand]
		public void ShowInferenceDialog()
		{
			ShowTraceRangeDialog = true;
		}

		/// <summary>
		/// 取消道号范围选择
		/// </summary>
		[RelayCommand]
		public void CancelTraceRangeSelection()
		{
			ShowTraceRangeDialog = false;
		}

		/// <summary>
		/// 开始地震相智能推理
		/// </summary>
		[RelayCommand]
		public async Task StartSeismicInference()
		{
			// 关闭对话框
			ShowTraceRangeDialog = false;

			// 模拟推理过程（延迟一下模拟AI计算）
			await Task.Delay(800);

			// 切换到推理结果图片
			IsShowingOriginalImage = false;
			LoadSectionImage();

			// 生成模拟推理结果
			GenerateMockSeismicInferenceResults();

			// 通知属性面板更新
			SeismicInferenceCompleted?.Invoke(SectionName, InferenceResults);
		}

		/// <summary>
		/// 生成模拟的地震相推理结果
		/// </summary>
		private void GenerateMockSeismicInferenceResults()
		{
			InferenceResults.Clear();

			// 模拟生成地震相推理结果
			InferenceResults.Add(new SeismicFaciesInferenceResult
			{
				SeismicFacies = "平行-亚平行反射",
				SedimentaryFacies = "河道分流相",
				Confidence = 0.89,
				SourceDirection = "北东向",
				Description = "反射同相轴连续性好，振幅中-强，频率中等，显示河道砂体特征"
			});

			InferenceResults.Add(new SeismicFaciesInferenceResult
			{
				SeismicFacies = "亚平行-波状反射",
				SedimentaryFacies = "泛滥平原相",
				Confidence = 0.85,
				SourceDirection = "不明显",
				Description = "反射连续性较差，振幅弱，频率低，为泛滥泥岩沉积"
			});

			InferenceResults.Add(new SeismicFaciesInferenceResult
			{
				SeismicFacies = "丘状-杂乱反射",
				SedimentaryFacies = "河口坝相",
				Confidence = 0.91,
				SourceDirection = "东向",
				Description = "丘状反射特征明显，振幅强，为河口坝砂体堆积"
			});

			InferenceResults.Add(new SeismicFaciesInferenceResult
			{
				SeismicFacies = "平行-连续强振幅",
				SedimentaryFacies = "三角洲前缘相",
				Confidence = 0.87,
				SourceDirection = "南东向",
				Description = "反射连续性好，振幅强，为三角洲前缘沉积体系"
			});
		}

		#endregion
	}

	/// <summary>
	/// 地震相智能推理结果
	/// </summary>
	public partial class SeismicFaciesInferenceResult : ObservableObject
	{
		/// <summary>
		/// 地震相类型
		/// </summary>
		[ObservableProperty]
		private string _seismicFacies = string.Empty;

		/// <summary>
		/// 沉积相类型
		/// </summary>
		[ObservableProperty]
		private string _sedimentaryFacies = string.Empty;

		/// <summary>
		/// 置信度（0-1）
		/// </summary>
		[ObservableProperty]
		private double _confidence;

		/// <summary>
		/// 物源方向
		/// </summary>
		[ObservableProperty]
		private string _sourceDirection = string.Empty;

		/// <summary>
		/// 综合描述
		/// </summary>
		[ObservableProperty]
		private string _description = string.Empty;

		/// <summary>
		/// 置信度百分比显示
		/// </summary>
		public string ConfidencePercent => $"{Confidence * 100:F0}%";
	}
}
