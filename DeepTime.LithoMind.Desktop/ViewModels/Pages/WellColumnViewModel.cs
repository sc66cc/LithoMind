using System;
using System.Collections.ObjectModel;
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
		/// 深度段选择事件 - 通知属性窗口更新
		/// </summary>
		public event Action<string, double, double>? DepthRangeSelected;

		/// <summary>
		/// 道添加事件 - 用于通知UI更新
		/// </summary>
		public event Action<WellLogTrack>? TrackAdded;

		public WellColumnViewModel()
		{
			Id = "WellColumn";
			Title = "单井综合柱状图";
			IconKey = "📊";
			Order = 6;

			// 初始化曲线道
			InitializeLogTracks();
			
			// 加载示例图片
			LoadSampleImage();
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
				// 尝试加载示例柱状图图片
				var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/Pics/A5-1.jpg");
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
}
