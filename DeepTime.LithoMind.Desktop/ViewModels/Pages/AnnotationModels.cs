using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 地震相枚举
	/// </summary>
	public enum SeismicFaciesType
	{
		[JsonPropertyName("未定义")]
		Undefined,
		[JsonPropertyName("平行反射")]
		Parallel,
		[JsonPropertyName("亚平行反射")]
		SubParallel,
		[JsonPropertyName("发散反射")]
		Divergent,
		[JsonPropertyName("前积反射")]
		Progradation,
		[JsonPropertyName("S形前积")]
		SigmoidProgradation,
		[JsonPropertyName("斜交前积")]
		ObliqueProgradation,
		[JsonPropertyName("复合前积")]
		ComplexProgradation,
		[JsonPropertyName("丘状反射")]
		Hummocky,
		[JsonPropertyName("杂乱反射")]
		Chaotic,
		[JsonPropertyName("空白反射")]
		TransparentReflection,
		[JsonPropertyName("波状反射")]
		Wavy,
		[JsonPropertyName("透镜状反射")]
		Lenticular,
		[JsonPropertyName("楔状反射")]
		Wedge,
		[JsonPropertyName("上超反射")]
		Onlap,
		[JsonPropertyName("下超反射")]
		Downlap,
		[JsonPropertyName("削截反射")]
		Truncation
	}

	/// <summary>
	/// 层位枚举
	/// </summary>
	public enum HorizonType
	{
		[JsonPropertyName("未定义")]
		Undefined,
		[JsonPropertyName("SQ1")]
		SQ1,
		[JsonPropertyName("SQ2")]
		SQ2,
		[JsonPropertyName("SQ3")]
		SQ3,
		[JsonPropertyName("SQ4")]
		SQ4,
		[JsonPropertyName("SQ5")]
		SQ5,
		[JsonPropertyName("HST")]
		HST,
		[JsonPropertyName("TST")]
		TST,
		[JsonPropertyName("LST")]
		LST,
		[JsonPropertyName("上段")]
		Upper,
		[JsonPropertyName("中段")]
		Middle,
		[JsonPropertyName("下段")]
		Lower,
		[JsonPropertyName("一段")]
		Section1,
		[JsonPropertyName("二段")]
		Section2,
		[JsonPropertyName("三段")]
		Section3
	}

	/// <summary>
	/// 沉积相枚举
	/// </summary>
	public enum SedimentaryFaciesType
	{
		[JsonPropertyName("未定义")]
		Undefined,
		[JsonPropertyName("河道")]
		Channel,
		[JsonPropertyName("分流河道")]
		DistributaryChannel,
		[JsonPropertyName("河道边缘")]
		ChannelMargin,
		[JsonPropertyName("河口坝")]
		MouthBar,
		[JsonPropertyName("泛滥平原")]
		FloodPlain,
		[JsonPropertyName("天然堤")]
		NaturalLevee,
		[JsonPropertyName("决口扇")]
		CrevasseSplay,
		[JsonPropertyName("湖泊")]
		Lake,
		[JsonPropertyName("浅湖")]
		ShallowLake,
		[JsonPropertyName("深湖")]
		DeepLake,
		[JsonPropertyName("滨湖")]
		LakeShore,
		[JsonPropertyName("三角洲前缘")]
		DeltaFront,
		[JsonPropertyName("三角洲平原")]
		DeltaPlain,
		[JsonPropertyName("前三角洲")]
		Prodelta,
		[JsonPropertyName("潮坪")]
		TidalFlat,
		[JsonPropertyName("沼泽")]
		Swamp
	}

	/// <summary>
	/// 测井相枚举
	/// </summary>
	public enum LogFaciesType
	{
		[JsonPropertyName("未定义")]
		Undefined,
		[JsonPropertyName("箱形")]
		BoxShape,
		[JsonPropertyName("钟形")]
		BellShape,
		[JsonPropertyName("漏斗形")]
		FunnelShape,
		[JsonPropertyName("齿化箱形")]
		SerratedBox,
		[JsonPropertyName("齿化钟形")]
		SerratedBell,
		[JsonPropertyName("齿化漏斗形")]
		SerratedFunnel,
		[JsonPropertyName("指形")]
		FingerShape,
		[JsonPropertyName("平直形")]
		FlatShape,
		[JsonPropertyName("复合形")]
		CompositeShape
	}

	/// <summary>
	/// 矩形标注数据模型
	/// </summary>
	public partial class WellAnnotation : ObservableObject
	{
		/// <summary>
		/// 标注唯一ID
		/// </summary>
		[ObservableProperty]
		private string _id = Guid.NewGuid().ToString("N")[..8];

		/// <summary>
		/// 顶部深度（米）
		/// </summary>
		[ObservableProperty]
		private double _depthTop;

		/// <summary>
		/// 底部深度（米）
		/// </summary>
		[ObservableProperty]
		private double _depthBottom;

		/// <summary>
		/// 层位名称
		/// </summary>
		[ObservableProperty]
		private string _horizonName = string.Empty;

		/// <summary>
		/// 层位类型
		/// </summary>
		[ObservableProperty]
		private HorizonType _horizon = HorizonType.Undefined;

		/// <summary>
		/// 沉积相类型
		/// </summary>
		[ObservableProperty]
		private SedimentaryFaciesType _sedimentaryFacies = SedimentaryFaciesType.Undefined;

		/// <summary>
		/// 测井相类型
		/// </summary>
		[ObservableProperty]
		private LogFaciesType _logFacies = LogFaciesType.Undefined;

		/// <summary>
		/// 描述信息
		/// </summary>
		[ObservableProperty]
		private string _description = string.Empty;

		/// <summary>
		/// 创建时间
		/// </summary>
		[ObservableProperty]
		private DateTime _createdTime = DateTime.Now;

		/// <summary>
		/// 是否选中
		/// </summary>
		[ObservableProperty]
		private bool _isSelected;

		/// <summary>
		/// 矩形在画布上的Y坐标（顶部）
		/// </summary>
		[ObservableProperty]
		private double _canvasTop;

		/// <summary>
		/// 矩形在画布上的高度
		/// </summary>
		[ObservableProperty]
		private double _canvasHeight;

		/// <summary>
		/// 矩形在画布上的X坐标（左侧）
		/// </summary>
		[ObservableProperty]
		private double _canvasLeft;

		/// <summary>
		/// 矩形在画布上的宽度
		/// </summary>
		[ObservableProperty]
		private double _canvasWidth;

		/// <summary>
		/// 标注颜色
		/// </summary>
		[ObservableProperty]
		private string _color = "#3498DB";

		/// <summary>
		/// 深度范围显示
		/// </summary>
		public string DepthRangeDisplay => $"{DepthTop:F1}m - {DepthBottom:F1}m";

		/// <summary>
		/// 层位显示名称
		/// </summary>
		public string HorizonDisplay => GetHorizonName(Horizon);

		/// <summary>
		/// 沉积相显示名称
		/// </summary>
		public string SedimentaryFaciesDisplay => GetSedimentaryFaciesName(SedimentaryFacies);

		/// <summary>
		/// 测井相显示名称
		/// </summary>
		public string LogFaciesDisplay => GetLogFaciesName(LogFacies);

		/// <summary>
		/// 获取层位中文名称
		/// </summary>
		public static string GetHorizonName(HorizonType type) => type switch
		{
			HorizonType.Undefined => "未定义",
			HorizonType.SQ1 => "SQ1",
			HorizonType.SQ2 => "SQ2",
			HorizonType.SQ3 => "SQ3",
			HorizonType.SQ4 => "SQ4",
			HorizonType.SQ5 => "SQ5",
			HorizonType.HST => "HST",
			HorizonType.TST => "TST",
			HorizonType.LST => "LST",
			HorizonType.Upper => "上段",
			HorizonType.Middle => "中段",
			HorizonType.Lower => "下段",
			HorizonType.Section1 => "一段",
			HorizonType.Section2 => "二段",
			HorizonType.Section3 => "三段",
			_ => "未知"
		};

		/// <summary>
		/// 获取沉积相中文名称
		/// </summary>
		public static string GetSedimentaryFaciesName(SedimentaryFaciesType type) => type switch
		{
			SedimentaryFaciesType.Undefined => "未定义",
			SedimentaryFaciesType.Channel => "河道",
			SedimentaryFaciesType.DistributaryChannel => "分流河道",
			SedimentaryFaciesType.ChannelMargin => "河道边缘",
			SedimentaryFaciesType.MouthBar => "河口坝",
			SedimentaryFaciesType.FloodPlain => "泛滥平原",
			SedimentaryFaciesType.NaturalLevee => "天然堤",
			SedimentaryFaciesType.CrevasseSplay => "决口扇",
			SedimentaryFaciesType.Lake => "湖泊",
			SedimentaryFaciesType.ShallowLake => "浅湖",
			SedimentaryFaciesType.DeepLake => "深湖",
			SedimentaryFaciesType.LakeShore => "滨湖",
			SedimentaryFaciesType.DeltaFront => "三角洲前缘",
			SedimentaryFaciesType.DeltaPlain => "三角洲平原",
			SedimentaryFaciesType.Prodelta => "前三角洲",
			SedimentaryFaciesType.TidalFlat => "潮坪",
			SedimentaryFaciesType.Swamp => "沼泽",
			_ => "未知"
		};

		/// <summary>
		/// 获取测井相中文名称
		/// </summary>
		public static string GetLogFaciesName(LogFaciesType type) => type switch
		{
			LogFaciesType.Undefined => "未定义",
			LogFaciesType.BoxShape => "箱形",
			LogFaciesType.BellShape => "钟形",
			LogFaciesType.FunnelShape => "漏斗形",
			LogFaciesType.SerratedBox => "齿化箱形",
			LogFaciesType.SerratedBell => "齿化钟形",
			LogFaciesType.SerratedFunnel => "齿化漏斗形",
			LogFaciesType.FingerShape => "指形",
			LogFaciesType.FlatShape => "平直形",
			LogFaciesType.CompositeShape => "复合形",
			_ => "未知"
		};

		/// <summary>
		/// 获取所有沉积相选项
		/// </summary>
		public static ObservableCollection<SedimentaryFaciesOption> GetSedimentaryFaciesOptions()
		{
			var options = new ObservableCollection<SedimentaryFaciesOption>();
			foreach (SedimentaryFaciesType type in Enum.GetValues(typeof(SedimentaryFaciesType)))
			{
				options.Add(new SedimentaryFaciesOption { Type = type, DisplayName = GetSedimentaryFaciesName(type) });
			}
			return options;
		}

		/// <summary>
		/// 获取所有测井相选项
		/// </summary>
		public static ObservableCollection<LogFaciesOption> GetLogFaciesOptions()
		{
			var options = new ObservableCollection<LogFaciesOption>();
			foreach (LogFaciesType type in Enum.GetValues(typeof(LogFaciesType)))
			{
				options.Add(new LogFaciesOption { Type = type, DisplayName = GetLogFaciesName(type) });
			}
			return options;
		}

		/// <summary>
		/// 获取所有层位选项
		/// </summary>
		public static ObservableCollection<HorizonOption> GetHorizonOptions()
		{
			var options = new ObservableCollection<HorizonOption>();
			foreach (HorizonType type in Enum.GetValues(typeof(HorizonType)))
			{
				options.Add(new HorizonOption { Type = type, DisplayName = GetHorizonName(type) });
			}
			return options;
		}
	}

	/// <summary>
	/// 层位选项（用于下拉框绑定）
	/// </summary>
	public class HorizonOption
	{
		public HorizonType Type { get; set; }
		public string DisplayName { get; set; } = string.Empty;
	}

	/// <summary>
	/// 沉积相选项（用于下拉框绑定）
	/// </summary>
	public class SedimentaryFaciesOption
	{
		public SedimentaryFaciesType Type { get; set; }
		public string DisplayName { get; set; } = string.Empty;
	}

	/// <summary>
	/// 测井相选项（用于下拉框绑定）
	/// </summary>
	public class LogFaciesOption
	{
		public LogFaciesType Type { get; set; }
		public string DisplayName { get; set; } = string.Empty;
	}

	/// <summary>
	/// 标注导出数据结构
	/// </summary>
	public class AnnotationExportData
	{
		public string WellName { get; set; } = string.Empty;
		public DateTime ExportTime { get; set; } = DateTime.Now;
		public int TotalAnnotations { get; set; }
		public ObservableCollection<AnnotationExportItem> Annotations { get; set; } = new();
	}

	/// <summary>
	/// 单个标注导出项
	/// </summary>
	public class AnnotationExportItem
	{
		public string Id { get; set; } = string.Empty;
		public double DepthTop { get; set; }
		public double DepthBottom { get; set; }
		public string HorizonName { get; set; } = string.Empty;
		public string SedimentaryFacies { get; set; } = string.Empty;
		public string LogFacies { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTime CreatedTime { get; set; }
	}

	#region 地震剖面多边形标注相关

	/// <summary>
	/// 地震相选项（用于下拉框绑定）
	/// </summary>
	public class SeismicFaciesOption
	{
		public SeismicFaciesType Type { get; set; }
		public string DisplayName { get; set; } = string.Empty;
	}

	/// <summary>
	/// 沉积相多选选项（用于复选框绑定）
	/// </summary>
	public partial class SedimentaryFaciesCheckOption : ObservableObject
	{
		public SedimentaryFaciesType Type { get; set; }
		public string DisplayName { get; set; } = string.Empty;

		[ObservableProperty]
		private bool _isSelected;
	}

	/// <summary>
	/// 多边形标注数据模型（用于地震剖面）
	/// </summary>
	public partial class SeismicPolygonAnnotation : ObservableObject
	{
		/// <summary>
		/// 标注唯一ID
		/// </summary>
		[ObservableProperty]
		private string _id = Guid.NewGuid().ToString("N")[..8];

		/// <summary>
		/// 多边形顶点集合（画布坐标）
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<Point> _points = new();

		/// <summary>
		/// 时间范围起始（双程旅行时，毫秒）
		/// </summary>
		[ObservableProperty]
		private double _timeStart;

		/// <summary>
		/// 时间范围结束（双程旅行时，毫秒）
		/// </summary>
		[ObservableProperty]
		private double _timeEnd;

		/// <summary>
		/// 地震相类型（单选）
		/// </summary>
		[ObservableProperty]
		private SeismicFaciesType _seismicFacies = SeismicFaciesType.Undefined;

		/// <summary>
		/// 沉积相类型（保留用于兼容）
		/// </summary>
		[ObservableProperty]
		private SedimentaryFaciesType _sedimentaryFacies = SedimentaryFaciesType.Undefined;

		/// <summary>
		/// 沉积相多选选项列表
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<SedimentaryFaciesCheckOption> _sedimentaryFaciesOptions = new();

		/// <summary>
		/// 描述信息
		/// </summary>
		[ObservableProperty]
		private string _description = string.Empty;

		/// <summary>
		/// 创建时间
		/// </summary>
		[ObservableProperty]
		private DateTime _createdTime = DateTime.Now;

		/// <summary>
		/// 是否选中
		/// </summary>
		[ObservableProperty]
		private bool _isSelected;

		/// <summary>
		/// 标注颜色
		/// </summary>
		[ObservableProperty]
		private string _color = "#3498DB";

		/// <summary>
		/// 多边形是否闭合
		/// </summary>
		[ObservableProperty]
		private bool _isClosed;

		/// <summary>
		/// 时间范围显示（毫秒）
		/// </summary>
		public string TimeRangeDisplay => $"{TimeStart:F0}ms - {TimeEnd:F0}ms";

		/// <summary>
		/// 地震相显示名称
		/// </summary>
		public string SeismicFaciesDisplay => GetSeismicFaciesName(SeismicFacies);

		/// <summary>
		/// 沉积相显示名称
		/// </summary>
		public string SedimentaryFaciesDisplay => WellAnnotation.GetSedimentaryFaciesName(SedimentaryFacies);

		/// <summary>
		/// 获取地震相中文名称
		/// </summary>
		public static string GetSeismicFaciesName(SeismicFaciesType type) => type switch
		{
			SeismicFaciesType.Undefined => "未定义",
			SeismicFaciesType.Parallel => "平行反射",
			SeismicFaciesType.SubParallel => "亚平行反射",
			SeismicFaciesType.Divergent => "发散反射",
			SeismicFaciesType.Progradation => "前积反射",
			SeismicFaciesType.SigmoidProgradation => "S形前积",
			SeismicFaciesType.ObliqueProgradation => "斜交前积",
			SeismicFaciesType.ComplexProgradation => "复合前积",
			SeismicFaciesType.Hummocky => "丘状反射",
			SeismicFaciesType.Chaotic => "杂乱反射",
			SeismicFaciesType.TransparentReflection => "空白反射",
			SeismicFaciesType.Wavy => "波状反射",
			SeismicFaciesType.Lenticular => "透镜状反射",
			SeismicFaciesType.Wedge => "楔状反射",
			SeismicFaciesType.Onlap => "上超反射",
			SeismicFaciesType.Downlap => "下超反射",
			SeismicFaciesType.Truncation => "削截反射",
			_ => "未知"
		};

		/// <summary>
		/// 获取所有地震相选项
		/// </summary>
		public static ObservableCollection<SeismicFaciesOption> GetSeismicFaciesOptions()
		{
			var options = new ObservableCollection<SeismicFaciesOption>();
			foreach (SeismicFaciesType type in Enum.GetValues(typeof(SeismicFaciesType)))
			{
				options.Add(new SeismicFaciesOption { Type = type, DisplayName = GetSeismicFaciesName(type) });
			}
			return options;
		}

		/// <summary>
		/// 获取沉积相多选选项列表
		/// </summary>
		public static ObservableCollection<SedimentaryFaciesCheckOption> GetSedimentaryFaciesCheckOptions()
		{
			var options = new ObservableCollection<SedimentaryFaciesCheckOption>();
			foreach (SedimentaryFaciesType type in Enum.GetValues(typeof(SedimentaryFaciesType)))
			{
				if (type != SedimentaryFaciesType.Undefined)
				{
					options.Add(new SedimentaryFaciesCheckOption
					{
						Type = type,
						DisplayName = WellAnnotation.GetSedimentaryFaciesName(type),
						IsSelected = false
					});
				}
			}
			return options;
		}

		/// <summary>
		/// 初始化沉积相多选选项
		/// </summary>
		public void InitializeSedimentaryFaciesOptions()
		{
			SedimentaryFaciesOptions = GetSedimentaryFaciesCheckOptions();
		}

		/// <summary>
		/// 获取选中的沉积相列表
		/// </summary>
		public List<SedimentaryFaciesType> GetSelectedSedimentaryFacies()
		{
			var selected = new List<SedimentaryFaciesType>();
			foreach (var option in SedimentaryFaciesOptions)
			{
				if (option.IsSelected)
				{
					selected.Add(option.Type);
				}
			}
			return selected;
		}

		/// <summary>
		/// 获取选中的沉积相显示文本
		/// </summary>
		public string GetSelectedSedimentaryFaciesDisplay()
		{
			var selected = GetSelectedSedimentaryFacies();
			if (selected.Count == 0) return "未选择";
			var names = selected.Select(t => WellAnnotation.GetSedimentaryFaciesName(t));
			return string.Join(", ", names);
		}

		/// <summary>
		/// 根据顶点计算时间范围
		/// </summary>
		public void CalculateTimeRangeFromPoints(double imageHeight, double timeRangeStart, double timeRangeEnd)
		{
			if (Points.Count == 0 || imageHeight <= 0) return;

			double minY = double.MaxValue;
			double maxY = double.MinValue;

			foreach (var point in Points)
			{
				if (point.Y < minY) minY = point.Y;
				if (point.Y > maxY) maxY = point.Y;
			}

			double timeRange = timeRangeEnd - timeRangeStart;
			double pixelPerMs = imageHeight / timeRange;

			TimeStart = timeRangeStart + (minY / pixelPerMs);
			TimeEnd = timeRangeStart + (maxY / pixelPerMs);
		}

		/// <summary>
		/// 添加顶点
		/// </summary>
		public void AddPoint(Point point)
		{
			Points.Add(point);
		}

		/// <summary>
		/// 获取多边形的边界矩形
		/// </summary>
		public Rect GetBounds()
		{
			if (Points.Count == 0) return default;

			double minX = double.MaxValue, minY = double.MaxValue;
			double maxX = double.MinValue, maxY = double.MinValue;

			foreach (var point in Points)
			{
				if (point.X < minX) minX = point.X;
				if (point.Y < minY) minY = point.Y;
				if (point.X > maxX) maxX = point.X;
				if (point.Y > maxY) maxY = point.Y;
			}

			return new Rect(minX, minY, maxX - minX, maxY - minY);
		}

		/// <summary>
		/// 检查点是否在多边形内部
		/// </summary>
		public bool ContainsPoint(Point testPoint)
		{
			if (Points.Count < 3) return false;

			bool result = false;
			int j = Points.Count - 1;

			for (int i = 0; i < Points.Count; i++)
			{
				if ((Points[i].Y < testPoint.Y && Points[j].Y >= testPoint.Y ||
					 Points[j].Y < testPoint.Y && Points[i].Y >= testPoint.Y) &&
					(Points[i].X + (testPoint.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) * (Points[j].X - Points[i].X) < testPoint.X))
				{
					result = !result;
				}
				j = i;
			}

			return result;
		}
	}

	/// <summary>
	/// 地震剖面标注导出数据结构
	/// </summary>
	public class SeismicAnnotationExportData
	{
		public string SectionName { get; set; } = string.Empty;
		public DateTime ExportTime { get; set; } = DateTime.Now;
		public int TotalAnnotations { get; set; }
		public ObservableCollection<SeismicAnnotationExportItem> Annotations { get; set; } = new();
	}

	/// <summary>
	/// 地震剖面单个标注导出项
	/// </summary>
	public class SeismicAnnotationExportItem
	{
		public string Id { get; set; } = string.Empty;
		public double TimeStart { get; set; }
		public double TimeEnd { get; set; }
		public List<PointExport> Points { get; set; } = new();
		public string SeismicFacies { get; set; } = string.Empty;
		public string SedimentaryFacies { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTime CreatedTime { get; set; }
	}

	/// <summary>
	/// 点导出结构
	/// </summary>
	public class PointExport
	{
		public double X { get; set; }
		public double Y { get; set; }
	}

	#endregion
}
