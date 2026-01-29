using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 属性窗口视图模型
	/// 用于显示JSON格式的数据（岩相、沉积相、地质描述信息等）
	/// </summary>
	public partial class PropertyPanelViewModel : PageViewModelBase
	{
		/// <summary>
		/// 当前显示的属性名称
		/// </summary>
		[ObservableProperty]
		private string _propertyTitle = "属性信息";

		/// <summary>
		/// 当前显示的JSON内容（格式化后的字符串）
		/// </summary>
		[ObservableProperty]
		private string _jsonContent = string.Empty;

		/// <summary>
		/// 当前选中的井名
		/// </summary>
		[ObservableProperty]
		private string _currentWellName = string.Empty;

		/// <summary>
		/// 当前选中的深度段
		/// </summary>
		[ObservableProperty]
		private string _currentDepthRange = string.Empty;

		/// <summary>
		/// 是否有数据显示
		/// </summary>
		[ObservableProperty]
		private bool _hasData;

		/// <summary>
		/// 深度段属性集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<DepthPropertyItem> _depthProperties = new();

		/// <summary>
		/// 是否显示推理结果
		/// </summary>
		[ObservableProperty]
		private bool _showInferenceResults;

		/// <summary>
		/// 是否显示地震相推理结果
		/// </summary>
		[ObservableProperty]
		private bool _showSeismicInferenceResults;

		/// <summary>
		/// 是否显示标注编辑模式
		/// </summary>
		[ObservableProperty]
		private bool _showAnnotationMode;

		/// <summary>
		/// 是否显示地震标注编辑模式
		/// </summary>
		[ObservableProperty]
		private bool _showSeismicAnnotationMode;

		/// <summary>
		/// 标注列表
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<WellAnnotation> _annotations = new();

		/// <summary>
		/// 地震标注列表
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<SeismicPolygonAnnotation> _seismicAnnotations = new();

		/// <summary>
		/// 当前选中的标注
		/// </summary>
		[ObservableProperty]
		private WellAnnotation? _selectedAnnotation;

		/// <summary>
		/// 当前选中的地震标注
		/// </summary>
		[ObservableProperty]
		private SeismicPolygonAnnotation? _selectedSeismicAnnotation;

		/// <summary>
		/// 沉积相选项列表
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<SedimentaryFaciesOption> _sedimentaryFaciesOptions = new();

		/// <summary>
		/// 测井相选项列表
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<LogFaciesOption> _logFaciesOptions = new();

		/// <summary>
		/// 层位选项列表
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<HorizonOption> _horizonOptionsList = new();

		/// <summary>
		/// 地震相选项列表（单选下拉框）
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<SeismicFaciesOption> _seismicFaciesOptions = new();

		/// <summary>
		/// 预设的岩性选项
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<string> _lithologyOptions = new();

		/// <summary>
		/// 预设的沉积相选项
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<string> _sedimentaryFaciesTextOptions = new();

		/// <summary>
		/// 预设的层位选项
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<string> _horizonOptions = new();

		public PropertyPanelViewModel()
		{
			Id = "PropertyPanel";
			Title = "属性窗口";
			IconKey = "📋";
			Order = 5;

			// 初始化预设选项
			InitializeOptions();

			// 初始化标注相关选项
			InitializeAnnotationOptions();

			// 初始化地震相关选项
			InitializeSeismicOptions();

			// 加载示例数据
			LoadSampleData();
		}

		/// <summary>
		/// 初始化地震相关选项
		/// </summary>
		private void InitializeSeismicOptions()
		{
			// 初始化地震相选项（单选下拉框）
			SeismicFaciesOptions = SeismicPolygonAnnotation.GetSeismicFaciesOptions();
		}

		/// <summary>
		/// 初始化标注相关选项
		/// </summary>
		private void InitializeAnnotationOptions()
		{
			// 初始化沉积相枚举选项
			SedimentaryFaciesOptions = WellAnnotation.GetSedimentaryFaciesOptions();

			// 初始化测井相枚举选项
			LogFaciesOptions = WellAnnotation.GetLogFaciesOptions();

			// 初始化层位枚举选项
			HorizonOptionsList = WellAnnotation.GetHorizonOptions();

			// 初始化层位选项
			HorizonOptions.Add("SQ1");
			HorizonOptions.Add("SQ2");
			HorizonOptions.Add("SQ3");
			HorizonOptions.Add("HST");
			HorizonOptions.Add("TST");
			HorizonOptions.Add("LST");
			HorizonOptions.Add("上段");
			HorizonOptions.Add("中段");
			HorizonOptions.Add("下段");
		}

		/// <summary>
		/// 初始化预设选项
		/// </summary>
		private void InitializeOptions()
		{
			// 岩性预设选项
			LithologyOptions.Add("粗砂岩");
			LithologyOptions.Add("中砂岩");
			LithologyOptions.Add("细砂岩");
			LithologyOptions.Add("粉砂岩");
			LithologyOptions.Add("泥岩");
			LithologyOptions.Add("砂质泥岩");
			LithologyOptions.Add("泥质砂岩");
			LithologyOptions.Add("灰岩");
			LithologyOptions.Add("白云岩");
			LithologyOptions.Add("页岩");
			LithologyOptions.Add("煤层");

			// 沉积相预设选项（文本）
			SedimentaryFaciesTextOptions.Add("河道");
			SedimentaryFaciesTextOptions.Add("分流河道");
			SedimentaryFaciesTextOptions.Add("河道边缘");
			SedimentaryFaciesTextOptions.Add("河口坝");
			SedimentaryFaciesTextOptions.Add("泛滥平原");
			SedimentaryFaciesTextOptions.Add("湖泊");
			SedimentaryFaciesTextOptions.Add("浅湖");
			SedimentaryFaciesTextOptions.Add("滨湖");
			SedimentaryFaciesTextOptions.Add("三角洲前缘");
			SedimentaryFaciesTextOptions.Add("深湖");
		}

		/// <summary>
		/// 加载示例数据
		/// </summary>
		private void LoadSampleData()
		{
			CurrentWellName = "A5-1";
			CurrentDepthRange = "4700m - 5000m";
			HasData = true;

			DepthProperties.Clear();
			
			// 添加示例深度段数据
			DepthProperties.Add(new DepthPropertyItem
			{
				DepthStart = 4700,
				DepthEnd = 4750,
				Lithology = "细砂岩",
				SedimentaryFacies = "河道",
				GeologicalDescription = "灰色细砂岩，分选中等，含少量泥质，见交错层理"
			});

			DepthProperties.Add(new DepthPropertyItem
			{
				DepthStart = 4750,
				DepthEnd = 4820,
				Lithology = "粉砂岩",
				SedimentaryFacies = "河道边缘",
				GeologicalDescription = "浅灰色粉砂岩，含较多泥质，见水平层理"
			});

			DepthProperties.Add(new DepthPropertyItem
			{
				DepthStart = 4820,
				DepthEnd = 4880,
				Lithology = "泥岩",
				SedimentaryFacies = "泛滥平原",
				GeologicalDescription = "深灰色泥岩，质纯，含少量植物碎片"
			});

			DepthProperties.Add(new DepthPropertyItem
			{
				DepthStart = 4880,
				DepthEnd = 4890,
				Lithology = "中砂岩",
				SedimentaryFacies = "分流河道",
				GeologicalDescription = "灰白色中砂岩，分选好，石英为主，见槽状交错层理"
			});

			DepthProperties.Add(new DepthPropertyItem
			{
				DepthStart = 4890,
				DepthEnd = 5000,
				Lithology = "细砂岩夹薄层泥岩",
				SedimentaryFacies = "河口坝",
				GeologicalDescription = "灰色细砂岩与薄层泥岩互层，见波状层理"
			});

			// 生成JSON内容用于显示
			UpdateJsonContent();
		}

		/// <summary>
		/// 更新JSON内容显示
		/// </summary>
		private void UpdateJsonContent()
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};

			var data = new
			{
				WellName = CurrentWellName,
				DepthRange = CurrentDepthRange,
				Properties = DepthProperties
			};

			JsonContent = JsonSerializer.Serialize(data, options);
		}

		/// <summary>
		/// 设置当前井的属性数据
		/// </summary>
		public void SetWellProperties(string wellName, ObservableCollection<DepthPropertyItem> properties)
		{
			CurrentWellName = wellName;
			DepthProperties = properties;
			HasData = properties.Count > 0;
			ShowInferenceResults = false;

			if (HasData)
			{
				var firstDepth = properties[0].DepthStart;
				var lastDepth = properties[properties.Count - 1].DepthEnd;
				CurrentDepthRange = $"{firstDepth}m - {lastDepth}m";
			}

			UpdateJsonContent();
		}

		/// <summary>
		/// 设置智能推理结果
		/// </summary>
		public void SetInferenceResults(string wellName, ObservableCollection<InferenceResult> results)
		{
			CurrentWellName = wellName;
			ShowInferenceResults = true;
			ShowSeismicInferenceResults = false;
			HasData = results.Count > 0;

			// 将InferenceResult转换为DepthPropertyItem
			DepthProperties.Clear();
			foreach (var result in results)
			{
				DepthProperties.Add(new DepthPropertyItem
				{
					DepthStart = result.DepthStart,
					DepthEnd = result.DepthEnd,
					Lithology = result.Lithofacies,
					SedimentaryFacies = result.SedimentaryFacies,
					GeologicalDescription = $"层位: {result.HorizonName}, 置信度: {result.ConfidencePercent}",
					Confidence = result.Confidence
				});
			}

			if (HasData)
			{
				var firstDepth = DepthProperties[0].DepthStart;
				var lastDepth = DepthProperties[DepthProperties.Count - 1].DepthEnd;
				CurrentDepthRange = $"{firstDepth}m - {lastDepth}m";
			}

			PropertyTitle = "岩相/沉积相推理结果";
			UpdateJsonContent();
		}

		/// <summary>
		/// 设置地震相智能推理结果
		/// </summary>
		public void SetSeismicInferenceResults(string sectionName, ObservableCollection<SeismicFaciesInferenceResult> results)
		{
			CurrentWellName = sectionName;
			ShowInferenceResults = false;
			ShowSeismicInferenceResults = true;
			HasData = results.Count > 0;

			// 将SeismicFaciesInferenceResult转换为DepthPropertyItem
			DepthProperties.Clear();
			int index = 1;
			foreach (var result in results)
			{
				DepthProperties.Add(new DepthPropertyItem
				{
					DepthStart = 0, // 地震相不使用深度，使用索引
					DepthEnd = 0,
					Lithology = result.SeismicFacies,
					SedimentaryFacies = result.SedimentaryFacies,
					GeologicalDescription = $"物源方向: {result.SourceDirection}\n{result.Description}",
					Confidence = result.Confidence,
					SeismicFaciesIndex = index++
				});
			}

			CurrentDepthRange = $"道号范围: xLine/inLine";
			PropertyTitle = "地震相/沉积相推理结果";
			UpdateJsonContent();
		}

		/// <summary>
		/// 选择深度段
		/// </summary>
		[RelayCommand]
		public void SelectDepthProperty(DepthPropertyItem? item)
		{
			if (item != null)
			{
				PropertyTitle = $"深度段: {item.DepthStart}m - {item.DepthEnd}m";
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
			CurrentWellName = string.Empty;
			CurrentDepthRange = string.Empty;
			DepthProperties.Clear();
			JsonContent = string.Empty;
			HasData = false;
			PropertyTitle = "属性信息";
		}

		#region 标注管理功能

		/// <summary>
		/// 设置标注列表（从WellColumnViewModel接收）
		/// </summary>
		public void SetAnnotations(string wellName, ObservableCollection<WellAnnotation> annotations)
		{
			CurrentWellName = wellName;
			Annotations = annotations;
			ShowAnnotationMode = true;
			ShowSeismicAnnotationMode = false;
			ShowInferenceResults = false;
			ShowSeismicInferenceResults = false;
			HasData = annotations.Count > 0 || true; // 即使没有标注也显示面板
			PropertyTitle = "标注列表";

			if (annotations.Count > 0)
			{
				var firstDepth = annotations[0].DepthTop;
				var lastDepth = annotations[annotations.Count - 1].DepthBottom;
				CurrentDepthRange = $"{firstDepth:F0}m - {lastDepth:F0}m";
			}
			else
			{
				CurrentDepthRange = "请在图上绘制标注矩形";
			}

			UpdateAnnotationJsonContent();
		}

		/// <summary>
		/// 设置选中的标注
		/// </summary>
		public void SetSelectedAnnotation(WellAnnotation? annotation)
		{
			SelectedAnnotation = annotation;
			if (annotation != null)
			{
				PropertyTitle = $"标注: {annotation.DepthRangeDisplay}";
			}
		}

		/// <summary>
		/// 删除选中的标注
		/// </summary>
		[RelayCommand]
		public void DeleteSelectedAnnotation()
		{
			if (SelectedAnnotation != null)
			{
				Annotations.Remove(SelectedAnnotation);
				SelectedAnnotation = null;
			}
		}

		/// <summary>
		/// 删除指定的标注
		/// </summary>
		[RelayCommand]
		public void DeleteAnnotation(WellAnnotation? annotation)
		{
			if (annotation != null)
			{
				Annotations.Remove(annotation);
				if (SelectedAnnotation == annotation)
				{
					SelectedAnnotation = null;
				}
				// 触发标注删除事件
				AnnotationDeleted?.Invoke(annotation);
			}
		}

		/// <summary>
		/// 标注删除事件 - 通知 WellColumnViewModel 同步删除
		/// </summary>
		public event Action<WellAnnotation>? AnnotationDeleted;

		/// <summary>
		/// 更新标注JSON显示
		/// </summary>
		public void UpdateAnnotationJsonContent()
		{
			if (!ShowAnnotationMode || Annotations.Count == 0)
			{
				JsonContent = string.Empty;
				return;
			}

			var exportData = new AnnotationExportData
			{
				WellName = CurrentWellName,
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

			JsonContent = JsonSerializer.Serialize(exportData, options);
		}

		#endregion

		#region 地震标注管理功能

		/// <summary>
		/// 设置地震标注列表
		/// </summary>
		public void SetSeismicAnnotations(string sectionName, ObservableCollection<SeismicPolygonAnnotation> annotations)
		{
			CurrentWellName = sectionName;
			SeismicAnnotations = annotations;
			ShowSeismicAnnotationMode = true;
			ShowAnnotationMode = false;
			ShowInferenceResults = false;
			ShowSeismicInferenceResults = false;
			HasData = true;
			PropertyTitle = "地震标注列表";

			if (annotations.Count > 0)
			{
				var firstTime = annotations[0].TimeStart;
				var lastTime = annotations[annotations.Count - 1].TimeEnd;
				CurrentDepthRange = $"{firstTime:F0}ms - {lastTime:F0}ms";
			}
			else
			{
				CurrentDepthRange = "请在剖面上绘制标注";
			}

			UpdateSeismicAnnotationJsonContent();
		}

		/// <summary>
		/// 设置选中的地震标注
		/// </summary>
		public void SetSelectedSeismicAnnotation(SeismicPolygonAnnotation? annotation)
		{
			SelectedSeismicAnnotation = annotation;
			if (annotation != null)
			{
				PropertyTitle = $"地震标注: {annotation.TimeRangeDisplay}";
			}
		}

		/// <summary>
		/// 删除选中的地震标注
		/// </summary>
		[RelayCommand]
		public void DeleteSelectedSeismicAnnotation()
		{
			if (SelectedSeismicAnnotation != null)
			{
				SeismicAnnotations.Remove(SelectedSeismicAnnotation);
				SelectedSeismicAnnotation = null;
			}
		}

		/// <summary>
		/// 删除指定的地震标注
		/// </summary>
		[RelayCommand]
		public void DeleteSeismicAnnotation(SeismicPolygonAnnotation? annotation)
		{
			if (annotation != null)
			{
				SeismicAnnotations.Remove(annotation);
				if (SelectedSeismicAnnotation == annotation)
				{
					SelectedSeismicAnnotation = null;
				}
				SeismicAnnotationDeleted?.Invoke(annotation);
			}
		}

		/// <summary>
		/// 地震标注删除事件
		/// </summary>
		public event Action<SeismicPolygonAnnotation>? SeismicAnnotationDeleted;

		/// <summary>
		/// 更新地震标注JSON显示
		/// </summary>
		public void UpdateSeismicAnnotationJsonContent()
		{
			if (!ShowSeismicAnnotationMode || SeismicAnnotations.Count == 0)
			{
				JsonContent = string.Empty;
				return;
			}

			var exportData = new SeismicAnnotationExportData
			{
				SectionName = CurrentWellName,
				ExportTime = DateTime.Now,
				TotalAnnotations = SeismicAnnotations.Count,
				Annotations = new ObservableCollection<SeismicAnnotationExportItem>()
			};

			foreach (var ann in SeismicAnnotations)
			{
				var item = new SeismicAnnotationExportItem
				{
					Id = ann.Id,
					TimeStart = ann.TimeStart,
					TimeEnd = ann.TimeEnd,
					SeismicFacies = SeismicPolygonAnnotation.GetSeismicFaciesName(ann.SeismicFacies),
					SedimentaryFacies = ann.GetSelectedSedimentaryFaciesDisplay(),
					Description = ann.Description,
					CreatedTime = ann.CreatedTime
				};
				exportData.Annotations.Add(item);
			}

			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};

			JsonContent = JsonSerializer.Serialize(exportData, options);
		}

		/// <summary>
		/// 切换到地震标注模式（用于地震相智能分析场景）
		/// </summary>
		public void SwitchToSeismicMode(string sectionName = "地震剖面")
		{
			CurrentWellName = sectionName;
			ShowSeismicAnnotationMode = true;
			ShowAnnotationMode = false;
			ShowInferenceResults = false;
			ShowSeismicInferenceResults = false;
			HasData = true;
			PropertyTitle = "地震标注";
			CurrentDepthRange = "请在剖面上绘制标注";

			// 如果没有标注，创建一个示例
			if (SeismicAnnotations.Count == 0)
			{
				var sampleAnnotation = new SeismicPolygonAnnotation
				{
					TimeStart = 1500,
					TimeEnd = 2000,
					SeismicFacies = SeismicFaciesType.Parallel,
					Description = "示例地震标注"
				};
				sampleAnnotation.InitializeSedimentaryFaciesOptions();
				SeismicAnnotations.Add(sampleAnnotation);
			}

			UpdateSeismicAnnotationJsonContent();
		}

		#endregion
	}

	/// <summary>
	/// 深度段属性项 - 支持编辑
	/// </summary>
	public partial class DepthPropertyItem : ObservableObject
	{
		/// <summary>
		/// 起始深度（米）
		/// </summary>
		[ObservableProperty]
		private double _depthStart;

		/// <summary>
		/// 终止深度（米）
		/// </summary>
		[ObservableProperty]
		private double _depthEnd;

		/// <summary>
		/// 岩性
		/// </summary>
		[ObservableProperty]
		private string _lithology = string.Empty;

		/// <summary>
		/// 沉积相
		/// </summary>
		[ObservableProperty]
		private string _sedimentaryFacies = string.Empty;

		/// <summary>
		/// 地质描述
		/// </summary>
		[ObservableProperty]
		private string _geologicalDescription = string.Empty;

		/// <summary>
		/// 置信度（智能推理时使用）
		/// </summary>
		[ObservableProperty]
		private double _confidence;

		/// <summary>
		/// 地震相索引（地震相推理时使用）
		/// </summary>
		[ObservableProperty]
		private int _seismicFaciesIndex;

		/// <summary>
		/// 深度范围显示
		/// </summary>
		public string DepthRangeDisplay => SeismicFaciesIndex > 0 
			? $"地震相 #{SeismicFaciesIndex}" 
			: $"{DepthStart:F1}m - {DepthEnd:F1}m";
	}
}
