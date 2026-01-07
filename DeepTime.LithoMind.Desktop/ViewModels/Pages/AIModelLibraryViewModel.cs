using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 智能模型状态枚举
	/// </summary>
	public enum ModelStatus
	{
		/// <summary>
		/// 可用
		/// </summary>
		Available,
		/// <summary>
		/// 训练中
		/// </summary>
		Training,
		/// <summary>
		/// 已停用
		/// </summary>
		Disabled,
		/// <summary>
		/// 待验证
		/// </summary>
		PendingValidation
	}

	/// <summary>
	/// 智能模型数据模型
	/// </summary>
	public partial class IntelligentModel : ObservableObject
	{
		/// <summary>
		/// 模型ID
		/// </summary>
		[ObservableProperty]
		private string _id = string.Empty;

		/// <summary>
		/// 模型名称
		/// </summary>
		[ObservableProperty]
		private string _name = string.Empty;

		/// <summary>
		/// 模型用途描述
		/// </summary>
		[ObservableProperty]
		private string _purpose = string.Empty;

		/// <summary>
		/// 训练时间
		/// </summary>
		[ObservableProperty]
		private DateTime _trainedDate;

		/// <summary>
		/// 可用状态
		/// </summary>
		[ObservableProperty]
		private ModelStatus _status;

		/// <summary>
		/// 模型版本
		/// </summary>
		[ObservableProperty]
		private string _version = string.Empty;

		/// <summary>
		/// 模型精度（百分比）
		/// </summary>
		[ObservableProperty]
		private double _accuracy;

		/// <summary>
		/// 适用区块/凹陷
		/// </summary>
		[ObservableProperty]
		private string _applicableArea = string.Empty;

		/// <summary>
		/// 训练时间显示格式
		/// </summary>
		public string TrainedDateDisplay => TrainedDate.ToString("yyyy-MM-dd");

		/// <summary>
		/// 状态显示文本
		/// </summary>
		public string StatusDisplay => Status switch
		{
			ModelStatus.Available => "可用",
			ModelStatus.Training => "训练中",
			ModelStatus.Disabled => "已停用",
			ModelStatus.PendingValidation => "待验证",
			_ => "未知"
		};

		/// <summary>
		/// 状态颜色
		/// </summary>
		public string StatusColor => Status switch
		{
			ModelStatus.Available => "#4CAF50",      // 绿色
			ModelStatus.Training => "#2196F3",       // 蓝色
			ModelStatus.Disabled => "#9E9E9E",       // 灰色
			ModelStatus.PendingValidation => "#FF9800", // 橙色
			_ => "#9E9E9E"
		};

		/// <summary>
		/// 精度显示
		/// </summary>
		public string AccuracyDisplay => $"{Accuracy:F1}%";
	}

	/// <summary>
	/// 智能模型库窗口 ViewModel
	/// </summary>
	public partial class AIModelLibraryViewModel : ObservableObject
	{
		/// <summary>
		/// 模型列表
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<IntelligentModel> _models = new();

		/// <summary>
		/// 选中的模型
		/// </summary>
		[ObservableProperty]
		private IntelligentModel? _selectedModel;

		/// <summary>
		/// 搜索关键字
		/// </summary>
		[ObservableProperty]
		private string _searchKeyword = string.Empty;

		/// <summary>
		/// 窗口关闭请求事件
		/// </summary>
		public event Action? CloseRequested;

		public AIModelLibraryViewModel()
		{
			LoadSampleModels();
		}

		/// <summary>
		/// 加载示例模型数据
		/// </summary>
		private void LoadSampleModels()
		{
			Models = new ObservableCollection<IntelligentModel>
			{
				new IntelligentModel
				{
					Id = "MODEL_001",
					Name = "惠西南惠州凹陷单井相智能模型 v1",
					Purpose = "基于测井曲线的单井岩相智能识别，适用于惠州凹陷古近系地层",
					TrainedDate = new DateTime(2024, 8, 15),
					Status = ModelStatus.Available,
					Version = "v1.0.0",
					Accuracy = 92.5,
					ApplicableArea = "惠州凹陷"
				},
				new IntelligentModel
				{
					Id = "MODEL_002",
					Name = "惠西南惠州凹陷单井相智能模型 v2",
					Purpose = "基于测井曲线的单井岩相智能识别，增强了砂泥岩识别能力",
					TrainedDate = new DateTime(2024, 11, 20),
					Status = ModelStatus.Available,
					Version = "v2.0.0",
					Accuracy = 95.2,
					ApplicableArea = "惠州凹陷"
				},
				new IntelligentModel
				{
					Id = "MODEL_003",
					Name = "珠江口盆地沉积相智能推理模型",
					Purpose = "基于多井联合分析的沉积相智能推理，支持三角洲、扇三角洲等相带识别",
					TrainedDate = new DateTime(2024, 10, 8),
					Status = ModelStatus.Available,
					Version = "v1.2.0",
					Accuracy = 88.7,
					ApplicableArea = "珠江口盆地"
				},
				new IntelligentModel
				{
					Id = "MODEL_004",
					Name = "白云凹陷地震相识别模型",
					Purpose = "基于地震属性的地震相自动识别，支持前积、平行等反射结构识别",
					TrainedDate = new DateTime(2024, 9, 25),
					Status = ModelStatus.Available,
					Version = "v1.0.0",
					Accuracy = 85.3,
					ApplicableArea = "白云凹陷"
				},
				new IntelligentModel
				{
					Id = "MODEL_005",
					Name = "陆丰凹陷储层预测模型",
					Purpose = "基于井震联合的储层厚度和物性预测",
					TrainedDate = new DateTime(2024, 12, 1),
					Status = ModelStatus.Training,
					Version = "v0.9.0",
					Accuracy = 78.5,
					ApplicableArea = "陆丰凹陷"
				},
				new IntelligentModel
				{
					Id = "MODEL_006",
					Name = "恩平凹陷岩性识别模型",
					Purpose = "基于常规测井曲线的岩性自动识别，支持砂岩、泥岩、碳酸盐岩等",
					TrainedDate = new DateTime(2024, 7, 10),
					Status = ModelStatus.PendingValidation,
					Version = "v1.1.0",
					Accuracy = 90.1,
					ApplicableArea = "恩平凹陷"
				},
				new IntelligentModel
				{
					Id = "MODEL_007",
					Name = "番禺低隆起沉积微相模型",
					Purpose = "精细沉积微相识别，支持水下分流河道、河口坝等微相类型",
					TrainedDate = new DateTime(2024, 6, 5),
					Status = ModelStatus.Disabled,
					Version = "v0.8.0",
					Accuracy = 72.3,
					ApplicableArea = "番禺低隆起"
				},
				new IntelligentModel
				{
					Id = "MODEL_008",
					Name = "文昌凹陷古地貌恢复模型",
					Purpose = "基于地震数据的古地貌智能恢复与分析",
					TrainedDate = new DateTime(2024, 11, 15),
					Status = ModelStatus.Available,
					Version = "v1.0.0",
					Accuracy = 86.8,
					ApplicableArea = "文昌凹陷"
				}
			};
		}

		/// <summary>
		/// 关闭窗口命令
		/// </summary>
		[RelayCommand]
		public void Close()
		{
			CloseRequested?.Invoke();
		}

		/// <summary>
		/// 刷新模型列表命令
		/// </summary>
		[RelayCommand]
		public void Refresh()
		{
			LoadSampleModels();
		}

		/// <summary>
		/// 使用选中的模型
		/// </summary>
		[RelayCommand]
		public void UseModel()
		{
			if (SelectedModel == null || SelectedModel.Status != ModelStatus.Available)
				return;

			// TODO: 实现模型使用逻辑
			CloseRequested?.Invoke();
		}
	}
}
