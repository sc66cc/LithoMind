using System;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages 
{
	/// <summary>
	/// 地层智能对比视图模型
	/// </summary>
	public partial class StratigraphyViewModel : PageViewModelBase
	{
		/// <summary>
		/// 数据资源树节点
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<DataResourceNode> _dataResources = new();

		/// <summary>
		/// 联井层序剖面图像
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
		/// 当前选中的数据资源节点
		/// </summary>
		[ObservableProperty]
		private DataResourceNode? _selectedNode;

		/// <summary>
		/// 对比槽集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<CorrelationTrack> _correlationTracks = new();

		public StratigraphyViewModel()
		{
			Id = "Stratigraphy";
			Title = "等时地层格架构建";
			IconKey = "\U0001f9f1";
			Order = 2;

			// 初始化数据资源树
			InitializeDataResources();
			
			// 加载示例图片
			LoadSampleImage();
		}

		/// <summary>
		/// 初始化数据资源树结构
		/// </summary>
		private void InitializeDataResources()
		{
			// 根据 Petrel 软件的工程结构设计，创建地层对比相关的数据资源
			
			// 1. 地震数据 - 用于确定大尺度地层界面
			var seismicNode = new DataResourceNode
			{
				Name = "地震数据",
				Icon = "🥓",
				IsExpanded = true,
				IsVisible = true,
				Children = new ObservableCollection<DataResourceNode>
				{
					new DataResourceNode { Name = "A工区地震体", Icon = "📁", IsVisible = true },
					new DataResourceNode { Name = "地震层位", Icon = "📁", IsVisible = true },
					new DataResourceNode { Name = "断层解释", Icon = "📁", IsVisible = false }
				}
			};

			// 2. 测井曲线数据 - 用于识别曲线突变点
			var wellLogsNode = new DataResourceNode
			{
				Name = "测井曲线数据",
				Icon = "📈",
				IsExpanded = true,
				IsVisible = true,
				Children = new ObservableCollection<DataResourceNode>
				{
					new DataResourceNode { Name = "A5-1井", Icon = "⚽", IsVisible = true },
					new DataResourceNode { Name = "A6-5井", Icon = "⚽", IsVisible = true },
					new DataResourceNode { Name = "A6-1井", Icon = "⚽", IsVisible = true },
					new DataResourceNode { Name = "A7-1井", Icon = "⚽", IsVisible = true },
					new DataResourceNode { Name = "A7-3井", Icon = "⚽", IsVisible = false }
				}
			};

			// 3. 古生物证据数据
			var paleoBioNode = new DataResourceNode
			{
				Name = "古生物数据",
				Icon = "🦕",
				IsExpanded = true,
				IsVisible = true,
				Children = new ObservableCollection<DataResourceNode>
				{
					new DataResourceNode { Name = "生物带", Icon = "📁", IsVisible = true },
					new DataResourceNode { Name = "孢粉", Icon = "📁", IsVisible = true },
				}
			};

			// 4. 其他相关地质数据
			var otherDataNode = new DataResourceNode
			{
				Name = "其他地质数据",
				Icon = "📊",
				IsExpanded = false,
				IsVisible = true,
				Children = new ObservableCollection<DataResourceNode>
				{
					new DataResourceNode { Name = "岩心数据", Icon = "📁", IsVisible = false },
					new DataResourceNode { Name = "沉积相数据", Icon = "📁", IsVisible = false },
					new DataResourceNode { Name = "时-深关系", Icon = "📁", IsVisible = true }
				}
			};

			DataResources.Add(seismicNode);
			DataResources.Add(wellLogsNode);
			DataResources.Add(paleoBioNode);
			DataResources.Add(otherDataNode);
		}

		/// <summary>
		/// 加载示例图片
		/// </summary>
		private void LoadSampleImage()
		{
			try
			{
				// 加载联井层序剖面图
				var uri = new Uri("avares://DeepTime.LithoMind.Desktop/Assets/Pics/联井层序剖面.jpg");
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

		// ===== 右键菜单命令 =====

		/// <summary>
		/// 新建对比槽
		/// </summary>
		[RelayCommand]
		public void CreateCorrelationTrack()
		{
			var trackNumber = CorrelationTracks.Count + 1;
			var newTrack = new CorrelationTrack
			{
				Name = $"对比槽 {trackNumber}",
				TrackType = "Standard",
				Width = 100,
				IsVisible = true
			};
			CorrelationTracks.Add(newTrack);
			
			// TODO: 在界面上显示新建的对比槽
		}

		/// <summary>
		/// 删除对比槽
		/// </summary>
		[RelayCommand]
		public void DeleteCorrelationTrack(CorrelationTrack? track)
		{
			if (track != null && CorrelationTracks.Contains(track))
			{
				CorrelationTracks.Remove(track);
			}
		}

		/// <summary>
		/// 编辑对比槽
		/// </summary>
		[RelayCommand]
		public void EditCorrelationTrack(CorrelationTrack? track)
		{
			if (track != null)
			{
				// TODO: 打开对比槽编辑对话框
			}
		}

		/// <summary>
		/// 设置地层填充方案
		/// </summary>
		[RelayCommand]
		public void SetStratumFillScheme()
		{
			// TODO: 打开地层填充方案对话框
			// 方案包括：颜色填充、图案填充、渐变填充等
		}

		/// <summary>
		/// 自动对比
		/// </summary>
		[RelayCommand]
		public void AutoCorrelation()
		{
			// TODO: 执行自动地层对比算法
		}

		/// <summary>
		/// 导出对比结果
		/// </summary>
		[RelayCommand]
		public void ExportCorrelationResult()
		{
			// TODO: 导出对比结果为图片或数据文件
		}

		/// <summary>
		/// 显示/隐藏数据层
		/// </summary>
		[RelayCommand]
		public void ToggleDataLayer(DataResourceNode? node)
		{
			if (node != null)
			{
				node.IsVisible = !node.IsVisible;
			}
		}

		/// <summary>
		/// 添加数据到对比视图
		/// </summary>
		[RelayCommand]
		public void AddDataToView(DataResourceNode? node)
		{
			if (node != null)
			{
				SelectedNode = node;
				// TODO: 将选中的数据添加到对比视图中
			}
		}

		/// <summary>
		/// 刷新数据资源
		/// </summary>
		[RelayCommand]
		public void RefreshDataResources()
		{
			// 重新加载数据资源树
			DataResources.Clear();
			InitializeDataResources();
		}
	}

	/// <summary>
	/// 数据资源树节点
	/// </summary>
	public partial class DataResourceNode : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private string _icon = "📁";

		[ObservableProperty]
		private bool _isExpanded;

		[ObservableProperty]
		private bool _isVisible = true;

		[ObservableProperty]
		private ObservableCollection<DataResourceNode>? _children;
	}

	/// <summary>
	/// 对比槽模型
	/// </summary>
	public partial class CorrelationTrack : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private string _trackType = "Standard";

		[ObservableProperty]
		private double _width = 100;

		[ObservableProperty]
		private bool _isVisible = true;

		[ObservableProperty]
		private string _fillScheme = "Color"; // Color, Pattern, Gradient
	}
}