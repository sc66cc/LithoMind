using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 道类型枚举
	/// </summary>
	public enum TrackType
	{
		/// <summary>深度道</summary>
		Depth,
		/// <summary>文本道</summary>
		Text,
		/// <summary>曲线道</summary>
		Curve,
		/// <summary>解释道</summary>
		Interpretation,
		/// <summary>岩性道</summary>
		Lithology,
		/// <summary>层序道</summary>
		Sequence
	}

	/// <summary>
	/// 单井综合柱状图视图模型
	/// </summary>
	public partial class WellColumnViewModel : PageViewModelBase
	{
		/// <summary>
		/// 当前井名
		/// </summary>
		[ObservableProperty]
		private string _wellName = "Well-5A-1";

		/// <summary>
		/// 井深范围起始
		/// </summary>
		[ObservableProperty]
		private double _depthStart = 4700;

		/// <summary>
		/// 井深范围结束
		/// </summary>
		[ObservableProperty]
		private double _depthEnd = 5000;

		/// <summary>
		/// 柱状图图像（原型阶段使用静态图片）
		/// </summary>
		[ObservableProperty]
		private Bitmap? _columnImage;

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
		/// 曲线道集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<WellLogTrack> _logTracks = new();

		/// <summary>
		/// 道数量计数
		/// </summary>
		private int _trackCounter = 0;

		/// <summary>
		/// 是否显示井选择对话框
		/// </summary>
		[ObservableProperty]
		private bool _showWellSelectionDialog;

		/// <summary>
		/// 可用的井列表
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<WellInfo> _availableWells = new();

		/// <summary>
		/// 当前是否显示原始图片（true）还是推理结果图片（false）
		/// </summary>
		[ObservableProperty]
		private bool _isShowingOriginalImage = true;

		/// <summary>
		/// 推理结果集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<InferenceResult> _inferenceResults = new();

		#region 标注功能相关属性

		/// <summary>
		/// 是否启用标注模式
		/// </summary>
		[ObservableProperty]
		private bool _isAnnotationMode;

		/// <summary>
		/// 是否正在绘制矩形
		/// </summary>
		[ObservableProperty]
		private bool _isDrawingAnnotation;

		/// <summary>
		/// 标注列表
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<WellAnnotation> _annotations = new();

		/// <summary>
		/// 当前选中的标注
		/// </summary>
		[ObservableProperty]
		private WellAnnotation? _selectedAnnotation;

		/// <summary>
		/// 当前绘制中的标注（临时）
		/// </summary>
		[ObservableProperty]
		private WellAnnotation? _currentDrawingAnnotation;

		/// <summary>
		/// 标注模式状态文本
		/// </summary>
		[ObservableProperty]
		private string _annotationModeText = "标注模式: 关闭";

		/// <summary>
		/// 图片实际高度（用于深度换算）
		/// </summary>
		[ObservableProperty]
		private double _imageActualHeight;

		/// <summary>
		/// 图片实际宽度
		/// </summary>
		[ObservableProperty]
		private double _imageActualWidth;

		#endregion

		/// <summary>
		/// 深度段选择事件 - 通知属性窗口更新
		/// </summary>
		public event Action<string, double, double>? DepthRangeSelected;

		/// <summary>
		/// 智能推理完成事件 - 通知属性面板更新
		/// </summary>
		public event Action<string, ObservableCollection<InferenceResult>>? InferenceCompleted;

		/// <summary>
		/// 道添加事件 - 用于通知UI更新
		/// </summary>
		public event Action<WellLogTrack>? TrackAdded;

		/// <summary>
		/// 标注更新事件 - 通知属性面板更新标注列表
		/// </summary>
		public event Action<string, ObservableCollection<WellAnnotation>>? AnnotationsChanged;

		/// <summary>
		/// 标注选中事件 - 通知属性面板选中某个标注
		/// </summary>
		public event Action<WellAnnotation?>? AnnotationSelected;

		public WellColumnViewModel()
		{
			Id = "WellColumn";
			Title = "单井综合柱状图";
			IconKey = "📊";
			Order = 6;

			// 初始化曲线道
			InitializeLogTracks();

			// 初始化可用井列表
			InitializeAvailableWells();

			// 延迟加载示例图片，避免阻塞UI线程
			Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadSampleImage(),
				Avalonia.Threading.DispatcherPriority.Background);
		}

		/// <summary>
		/// 初始化曲线道
		/// </summary>
		private void InitializeLogTracks()
		{
			LogTracks.Add(new WellLogTrack { Name = "深度", Type = "Depth", TrackCategory = TrackType.Depth, IsVisible = true, Color = "#2C3E50" });
			LogTracks.Add(new WellLogTrack { Name = "GR", Type = "GammaRay", TrackCategory = TrackType.Curve, IsVisible = true, Color = "#27AE60" });
			LogTracks.Add(new WellLogTrack { Name = "SP", Type = "SelfPotential", TrackCategory = TrackType.Curve, IsVisible = true, Color = "#3498DB" });
			LogTracks.Add(new WellLogTrack { Name = "RHOB", Type = "Density", TrackCategory = TrackType.Curve, IsVisible = true, Color = "#E74C3C" });
			LogTracks.Add(new WellLogTrack { Name = "DT", Type = "Sonic", TrackCategory = TrackType.Curve, IsVisible = true, Color = "#9B59B6" });
			LogTracks.Add(new WellLogTrack { Name = "岩性", Type = "Lithology", TrackCategory = TrackType.Lithology, IsVisible = true, Color = "#F39C12" });
			LogTracks.Add(new WellLogTrack { Name = "沉积相", Type = "Facies", TrackCategory = TrackType.Interpretation, IsVisible = true, Color = "#1ABC9C" });
			LogTracks.Add(new WellLogTrack { Name = "三级层序", Type = "ThreeSequence", TrackCategory = TrackType.Sequence, IsVisible = true, Color = "#1ABC9C" });

			_trackCounter = LogTracks.Count;
		}

		/// <summary>
		/// 加载示例图片
		/// </summary>
		private void LoadSampleImage()
		{
			try
			{
				// 根据当前状态加载原始图片或推理结果图片
				string imageName = IsShowingOriginalImage ? "A5-1-ori.jpg" : "A5-1-res.jpg";
				var uri = new Uri($"avares://DeepTime.LithoMind.Desktop/Assets/Pics/{imageName}");
				var assets = Avalonia.Platform.AssetLoader.Open(uri);
				ColumnImage = new Bitmap(assets);
				HasImage = true;
			}
			catch
			{
				HasImage = false;
			}
		}

		/// <summary>
		/// 初始化可用井列表
		/// </summary>
		private void InitializeAvailableWells()
		{
			AvailableWells.Add(new WellInfo { Name = "A5-1", IsSelected = true });
			AvailableWells.Add(new WellInfo { Name = "A6-5", IsSelected = false });
			AvailableWells.Add(new WellInfo { Name = "A6-1", IsSelected = false });
			AvailableWells.Add(new WellInfo { Name = "A7-1", IsSelected = false });
			AvailableWells.Add(new WellInfo { Name = "A7-3", IsSelected = false });
			AvailableWells.Add(new WellInfo { Name = "B5-1", IsSelected = false });
			AvailableWells.Add(new WellInfo { Name = "B5-2", IsSelected = false });
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
		/// 选择深度范围
		/// </summary>
		public void SelectDepthRange(double startDepth, double endDepth)
		{
			DepthRangeSelected?.Invoke(WellName, startDepth, endDepth);
		}

		/// <summary>
		/// 切换曲线道显示
		/// </summary>
		[RelayCommand]
		public void ToggleTrack(WellLogTrack? track)
		{
			if (track != null)
			{
				track.IsVisible = !track.IsVisible;
			}
		}

		/// <summary>
		/// 加载指定井的数据
		/// </summary>
		public void LoadWellData(string wellName)
		{
			WellName = wellName;
			Title = $"单井综合柱状图 - {wellName}";
			
			// 原型阶段：根据井名加载对应的示例图片
			LoadSampleImage();
		}

		#region 新增道功能

		/// <summary>
		/// 新增深度道
		/// </summary>
		[RelayCommand]
		public void AddDepthTrack()
		{
			AddTrack(TrackType.Depth, "深度道", "#2C3E50");
		}

		/// <summary>
		/// 新增文本道
		/// </summary>
		[RelayCommand]
		public void AddTextTrack()
		{
			AddTrack(TrackType.Text, "文本道", "#16A085");
		}

		/// <summary>
		/// 新增曲线道
		/// </summary>
		[RelayCommand]
		public void AddCurveTrack()
		{
			AddTrack(TrackType.Curve, "曲线道", "#2980B9");
		}

		/// <summary>
		/// 新增解释道
		/// </summary>
		[RelayCommand]
		public void AddInterpretationTrack()
		{
			AddTrack(TrackType.Interpretation, "解释道", "#8E44AD");
		}

		/// <summary>
		/// 新增岩性道
		/// </summary>
		[RelayCommand]
		public void AddLithologyTrack()
		{
			AddTrack(TrackType.Lithology, "岩性道", "#D35400");
		}

		/// <summary>
		/// 添加道的通用方法
		/// </summary>
		private void AddTrack(TrackType trackType, string baseName, string color)
		{
			_trackCounter++;
			var track = new WellLogTrack
			{
				Name = $"{baseName} {_trackCounter}",
				Type = trackType.ToString(),
				TrackCategory = trackType,
				IsVisible = true,
				Color = color
			};
			LogTracks.Add(track);
			TrackAdded?.Invoke(track);
		}

		/// <summary>
		/// 删除道
		/// </summary>
		[RelayCommand]
		public void RemoveTrack(WellLogTrack? track)
		{
			if (track != null && LogTracks.Contains(track))
			{
				LogTracks.Remove(track);
			}
		}

		#endregion

		#region 智能推理功能

		/// <summary>
		/// 显示井选择对话框
		/// </summary>
		[RelayCommand]
		public void ShowInferenceDialog()
		{
			ShowWellSelectionDialog = true;
		}

		/// <summary>
		/// 取消井选择
		/// </summary>
		[RelayCommand]
		public void CancelWellSelection()
		{
			ShowWellSelectionDialog = false;
		}

		/// <summary>
		/// 开始智能推理
		/// </summary>
		[RelayCommand]
		public async Task StartInference()
		{
			// 关闭对话框
			ShowWellSelectionDialog = false;

			// 模拟推理过程（延迟一下模拟AI计算）
			await System.Threading.Tasks.Task.Delay(500);

			// 切换到推理结果图片
			IsShowingOriginalImage = false;
			LoadSampleImage();

			// 生成模拟推理结果
			GenerateMockInferenceResults();

			// 通知属性面板更新
			InferenceCompleted?.Invoke(WellName, InferenceResults);
		}

		/// <summary>
		/// 全选井
		/// </summary>
		[RelayCommand]
		public void SelectAllWells()
		{
			foreach (var well in AvailableWells)
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
			foreach (var well in AvailableWells)
			{
				well.IsSelected = false;
			}
		}

		/// <summary>
		/// 生成模拟的推理结果
		/// </summary>
		private void GenerateMockInferenceResults()
		{
			InferenceResults.Clear();

			// 模拟生成各个层位的推理结果
			InferenceResults.Add(new InferenceResult
			{
				HorizonName = "上段",
				DepthStart = 4700,
				DepthEnd = 4750,
				Lithofacies = "砂岩",
				SedimentaryFacies = "河道相",
				Confidence = 0.92
			});

			InferenceResults.Add(new InferenceResult
			{
				HorizonName = "中段",
				DepthStart = 4750,
				DepthEnd = 4850,
				Lithofacies = "泥岩",
				SedimentaryFacies = "泛滥相",
				Confidence = 0.88
			});

			InferenceResults.Add(new InferenceResult
			{
				HorizonName = "下段",
				DepthStart = 4850,
				DepthEnd = 4950,
				Lithofacies = "灰岩",
				SedimentaryFacies = "潮坪相",
				Confidence = 0.85
			});
		}

		#endregion

		#region 矩形标注功能

		/// <summary>
		/// 切换标注模式
		/// </summary>
		[RelayCommand]
		public void ToggleAnnotationMode()
		{
			IsAnnotationMode = !IsAnnotationMode;
			AnnotationModeText = IsAnnotationMode ? "标注模式: 开启 (拖拽绘制矩形)" : "标注模式: 关闭";

			if (!IsAnnotationMode)
			{
				IsDrawingAnnotation = false;
				CurrentDrawingAnnotation = null;
			}
		}

		/// <summary>
		/// 启用标注模式（从菜单调用）
		/// </summary>
		[RelayCommand]
		public void EnableAnnotationMode()
		{
			IsAnnotationMode = true;
			AnnotationModeText = "标注模式: 开启 (拖拽绘制矩形)";

			// 通知属性面板切换到标注模式
			AnnotationsChanged?.Invoke(WellName, Annotations);
		}

		/// <summary>
		/// 开始绘制标注矩形
		/// </summary>
		public void StartDrawingAnnotation(double x, double y)
		{
			if (!IsAnnotationMode) return;

			IsDrawingAnnotation = true;
			CurrentDrawingAnnotation = new WellAnnotation
			{
				CanvasLeft = x,
				CanvasTop = y,
				CanvasWidth = 0,
				CanvasHeight = 0,
				Color = GetNextAnnotationColor()
			};
		}

		/// <summary>
		/// 更新绘制中的矩形
		/// </summary>
		public void UpdateDrawingAnnotation(double x, double y)
		{
			if (!IsDrawingAnnotation || CurrentDrawingAnnotation == null) return;

			double startX = CurrentDrawingAnnotation.CanvasLeft;
			double startY = CurrentDrawingAnnotation.CanvasTop;

			// 计算矩形位置和尺寸（支持任意方向拖拽）
			double left = Math.Min(startX, x);
			double top = Math.Min(startY, y);
			double width = Math.Abs(x - startX);
			double height = Math.Abs(y - startY);

			CurrentDrawingAnnotation.CanvasLeft = left;
			CurrentDrawingAnnotation.CanvasTop = top;
			CurrentDrawingAnnotation.CanvasWidth = width;
			CurrentDrawingAnnotation.CanvasHeight = height;
		}

		/// <summary>
		/// 完成绘制标注矩形
		/// </summary>
		public void FinishDrawingAnnotation()
		{
			if (!IsDrawingAnnotation || CurrentDrawingAnnotation == null) return;

			// 只有矩形足够大才添加
			if (CurrentDrawingAnnotation.CanvasWidth > 10 && CurrentDrawingAnnotation.CanvasHeight > 10)
			{
				// 根据画布位置计算深度
				CalculateDepthFromCanvas(CurrentDrawingAnnotation);

				Annotations.Add(CurrentDrawingAnnotation);
				SelectedAnnotation = CurrentDrawingAnnotation;

				// 通知属性面板更新
				AnnotationsChanged?.Invoke(WellName, Annotations);
				AnnotationSelected?.Invoke(CurrentDrawingAnnotation);
			}

			IsDrawingAnnotation = false;
			CurrentDrawingAnnotation = null;
		}

		/// <summary>
		/// 根据画布位置计算深度
		/// </summary>
		private void CalculateDepthFromCanvas(WellAnnotation annotation)
		{
			if (ImageActualHeight <= 0) return;

			double depthRange = DepthEnd - DepthStart;
			double pixelPerMeter = ImageActualHeight / depthRange;

			// 从画布Y坐标换算为深度
			annotation.DepthTop = DepthStart + (annotation.CanvasTop / pixelPerMeter);
			annotation.DepthBottom = DepthStart + ((annotation.CanvasTop + annotation.CanvasHeight) / pixelPerMeter);

			// 确保顶部深度小于底部深度
			if (annotation.DepthTop > annotation.DepthBottom)
			{
				(annotation.DepthTop, annotation.DepthBottom) = (annotation.DepthBottom, annotation.DepthTop);
			}
		}

		/// <summary>
		/// 选中标注
		/// </summary>
		[RelayCommand]
		public void SelectAnnotation(WellAnnotation? annotation)
		{
			// 取消之前选中的
			if (SelectedAnnotation != null)
			{
				SelectedAnnotation.IsSelected = false;
			}

			SelectedAnnotation = annotation;

			if (annotation != null)
			{
				annotation.IsSelected = true;
			}

			AnnotationSelected?.Invoke(annotation);
		}

		/// <summary>
		/// 删除标注
		/// </summary>
		[RelayCommand]
		public void DeleteAnnotation(WellAnnotation? annotation)
		{
			if (annotation == null) return;

			Annotations.Remove(annotation);

			if (SelectedAnnotation == annotation)
			{
				SelectedAnnotation = null;
			}

			AnnotationsChanged?.Invoke(WellName, Annotations);
		}

		/// <summary>
		/// 删除选中的标注
		/// </summary>
		[RelayCommand]
		public void DeleteSelectedAnnotation()
		{
			if (SelectedAnnotation != null)
			{
				DeleteAnnotation(SelectedAnnotation);
			}
		}

		/// <summary>
		/// 清除所有标注
		/// </summary>
		[RelayCommand]
		public void ClearAllAnnotations()
		{
			Annotations.Clear();
			SelectedAnnotation = null;
			AnnotationsChanged?.Invoke(WellName, Annotations);
		}

		/// <summary>
		/// 导出标注为JSON
		/// </summary>
		[RelayCommand]
		public async Task ExportAnnotationsToJson()
		{
			if (Annotations.Count == 0) return;

			var exportData = new AnnotationExportData
			{
				WellName = WellName,
				ExportTime = DateTime.Now,
				TotalAnnotations = Annotations.Count,
				Annotations = new ObservableCollection<AnnotationExportItem>()
			};

			foreach (var ann in Annotations)
			{
				exportData.Annotations.Add(new AnnotationExportItem
				{
					Id = ann.Id,
					DepthTop = ann.DepthTop,
					DepthBottom = ann.DepthBottom,
					HorizonName = ann.HorizonName,
					SedimentaryFacies = WellAnnotation.GetSedimentaryFaciesName(ann.SedimentaryFacies),
					LogFacies = WellAnnotation.GetLogFaciesName(ann.LogFacies),
					Description = ann.Description,
					CreatedTime = ann.CreatedTime
				});
			}

			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};

			string json = JsonSerializer.Serialize(exportData, options);

			// 保存到文件
			string fileName = $"Annotations_{WellName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string filePath = Path.Combine(documentsPath, "LithoMind", fileName);

			// 确保目录存在
			Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

			await File.WriteAllTextAsync(filePath, json);

			// 可以在这里添加一个通知
			System.Diagnostics.Debug.WriteLine($"标注已导出到: {filePath}");
		}

		/// <summary>
		/// 获取下一个标注的颜色
		/// </summary>
		private string GetNextAnnotationColor()
		{
			string[] colors = { "#3498DB", "#E74C3C", "#27AE60", "#F39C12", "#9B59B6", "#1ABC9C", "#E67E22", "#2ECC71" };
			return colors[Annotations.Count % colors.Length];
		}

		/// <summary>
		/// 更新标注信息
		/// </summary>
		public void UpdateAnnotation(WellAnnotation annotation)
		{
			// 触发属性更新通知
			AnnotationsChanged?.Invoke(WellName, Annotations);
		}

		#endregion
	}

	/// <summary>
	/// 曲线道信息
	/// </summary>
	public partial class WellLogTrack : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private string _type = string.Empty;

		[ObservableProperty]
		private TrackType _trackCategory = TrackType.Curve;

		[ObservableProperty]
		private bool _isVisible = true;

		[ObservableProperty]
		private string _color = "#000000";

		/// <summary>
		/// 获取道类型显示名称
		/// </summary>
		public string TrackTypeDisplay => TrackCategory switch
		{
			TrackType.Depth => "深度道",
			TrackType.Text => "文本道",
			TrackType.Curve => "曲线道",
			TrackType.Interpretation => "解释道",
			TrackType.Lithology => "岩性道",
			_ => "未知"
		};
	}

	/// <summary>
	/// 井信息
	/// </summary>
	public partial class WellInfo : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private bool _isSelected;
	}

	/// <summary>
	/// 智能推理结果
	/// </summary>
	public partial class InferenceResult : ObservableObject
	{
		/// <summary>
		/// 层位名称
		/// </summary>
		[ObservableProperty]
		private string _horizonName = string.Empty;

		/// <summary>
		/// 深度起始
		/// </summary>
		[ObservableProperty]
		private double _depthStart;

		/// <summary>
		/// 深度结束
		/// </summary>
		[ObservableProperty]
		private double _depthEnd;

		/// <summary>
		/// 岩相类型
		/// </summary>
		[ObservableProperty]
		private string _lithofacies = string.Empty;

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
		/// 置信度百分比显示
		/// </summary>
		public string ConfidencePercent => $"{Confidence * 100:F0}%";
	}
}
