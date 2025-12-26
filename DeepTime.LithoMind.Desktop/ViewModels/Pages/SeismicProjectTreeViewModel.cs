using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 地震数据节点
	/// </summary>
	public partial class SeismicDataNode : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private string _nodeType = string.Empty; // SeismicVolume, Section, Horizon, Fault, Attribute

		[ObservableProperty]
		private bool _isExpanded;

		[ObservableProperty]
		private string _iconKey = "📁";

		[ObservableProperty]
		private string _description = string.Empty;

		[ObservableProperty]
		private ObservableCollection<SeismicDataNode> _children = new();

		[ObservableProperty]
		private bool _isSelected;

		/// <summary>
		/// 是否勾选（控制是否在中间区域显示）
		/// </summary>
		[ObservableProperty]
		private bool _isChecked = true;

		/// <summary>
		/// 是否显示CheckBox（只有特定类型节点显示）
		/// </summary>
		public bool ShowCheckBox => NodeType is "SeismicVolume" or "Section" or "Horizon" or "Fault" or "Attribute" or "Inline" or "Crossline" or "TimeSlice";

		/// <summary>
		/// 关联的资源路径（用于加载图片等）
		/// </summary>
		[ObservableProperty]
		private string _resourcePath = string.Empty;
	}

	/// <summary>
	/// 地震综合功能分区的工程结构目录视图模型
	/// 参考Petrel的工程资源树设计
	/// </summary>
	public partial class SeismicProjectTreeViewModel : PageViewModelBase
	{
		/// <summary>
		/// 根节点集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<SeismicDataNode> _rootNodes = new();

		/// <summary>
		/// 当前选中的节点
		/// </summary>
		[ObservableProperty]
		private SeismicDataNode? _selectedNode;

		/// <summary>
		/// 地震体选择事件
		/// </summary>
		public event Action<SeismicDataNode>? SeismicVolumeSelected;

		/// <summary>
		/// 剖面选择事件
		/// </summary>
		public event Action<SeismicDataNode>? SectionSelected;

		/// <summary>
		/// 勾选状态变化事件
		/// </summary>
		public event Action<SeismicDataNode, bool>? NodeCheckedChanged;

		public SeismicProjectTreeViewModel()
		{
			Id = "SeismicProjectTree";
			Title = "工程结构目录";
			IconKey = "📂";
			Order = 1;

			// 加载地震数据结构
			LoadSeismicDataStructure();
		}

		/// <summary>
		/// 加载地震数据结构（参考Petrel工程资源树）
		/// </summary>
		private void LoadSeismicDataStructure()
		{
			RootNodes.Clear();

			// 创建项目根节点
			var projectRoot = new SeismicDataNode
			{
				Name = "LithoMind地震工程",
				NodeType = "Project",
				IsExpanded = true,
				IconKey = "📦"
			};

			// 地震体数据文件夹
			var seismicVolumesFolder = new SeismicDataNode
			{
				Name = "地震体数据",
				NodeType = "Folder",
				IsExpanded = true,
				IconKey = "🌊"
			};

			// 添加地震体
			seismicVolumesFolder.Children.Add(CreateSeismicVolumeNode("主力三维地震体", "3D Survey A", true));
			seismicVolumesFolder.Children.Add(CreateSeismicVolumeNode("补充地震数据", "3D Survey B", false));

			projectRoot.Children.Add(seismicVolumesFolder);

			// 地震解释剖面文件夹
			var sectionsFolder = new SeismicDataNode
			{
				Name = "地震解释剖面",
				NodeType = "Folder",
				IsExpanded = true,
				IconKey = "📊"
			};

			sectionsFolder.Children.Add(new SeismicDataNode
			{
				Name = "主测线剖面 IL-2500",
				NodeType = "Section",
				IconKey = "📈",
				Description = "Inline 2500",
				ResourcePath = "InterpreWindowScale.png",
				IsChecked = true
			});

			sectionsFolder.Children.Add(new SeismicDataNode
			{
				Name = "联络线剖面 XL-1800",
				NodeType = "Section",
				IconKey = "📈",
				Description = "Crossline 1800",
				ResourcePath = "InterpreWindowScale.png",
				IsChecked = true
			});

			sectionsFolder.Children.Add(new SeismicDataNode
			{
				Name = "任意线剖面 AB",
				NodeType = "Section",
				IconKey = "📐",
				Description = "Arbitrary Line A-B",
				ResourcePath = "InterpreWindowScale.png",
				IsChecked = false
			});

			projectRoot.Children.Add(sectionsFolder);

			// 层位数据文件夹
			var horizonsFolder = new SeismicDataNode
			{
				Name = "层位数据",
				NodeType = "Folder",
				IsExpanded = true,
				IconKey = "📏"
			};

			horizonsFolder.Children.Add(CreateHorizonNode("T1顶面", "#E74C3C", 1850));
			horizonsFolder.Children.Add(CreateHorizonNode("T2顶面", "#3498DB", 2100));
			horizonsFolder.Children.Add(CreateHorizonNode("T3顶面", "#27AE60", 2350));
			horizonsFolder.Children.Add(CreateHorizonNode("T4顶面", "#9B59B6", 2580));

			projectRoot.Children.Add(horizonsFolder);

			// 断层数据文件夹
			var faultsFolder = new SeismicDataNode
			{
				Name = "断层数据",
				NodeType = "Folder",
				IsExpanded = false,
				IconKey = "⚡"
			};

			faultsFolder.Children.Add(new SeismicDataNode 
			{ 
				Name = "F1主断层", 
				NodeType = "Fault", 
				IconKey = "⚡", 
				Description = "正断层, NE走向",
				IsChecked = true
			});
			faultsFolder.Children.Add(new SeismicDataNode 
			{ 
				Name = "F2次级断层", 
				NodeType = "Fault", 
				IconKey = "⚡", 
				Description = "正断层, NW走向",
				IsChecked = true
			});
			faultsFolder.Children.Add(new SeismicDataNode 
			{ 
				Name = "F3分支断层", 
				NodeType = "Fault", 
				IconKey = "⚡", 
				Description = "正断层, N走向",
				IsChecked = false
			});

			projectRoot.Children.Add(faultsFolder);

			// 地震属性文件夹
			var attributesFolder = new SeismicDataNode
			{
				Name = "地震属性",
				NodeType = "Folder",
				IsExpanded = false,
				IconKey = "🎨"
			};

			attributesFolder.Children.Add(new SeismicDataNode 
			{ 
				Name = "振幅属性", 
				NodeType = "Attribute", 
				IconKey = "📊", 
				Description = "RMS Amplitude",
				IsChecked = true
			});
			attributesFolder.Children.Add(new SeismicDataNode 
			{ 
				Name = "频率属性", 
				NodeType = "Attribute", 
				IconKey = "📊", 
				Description = "Instantaneous Frequency",
				IsChecked = false
			});
			attributesFolder.Children.Add(new SeismicDataNode 
			{ 
				Name = "相位属性", 
				NodeType = "Attribute", 
				IconKey = "📊", 
				Description = "Instantaneous Phase",
				IsChecked = false
			});

			projectRoot.Children.Add(attributesFolder);

			RootNodes.Add(projectRoot);
		}

		/// <summary>
		/// 创建地震体节点
		/// </summary>
		private SeismicDataNode CreateSeismicVolumeNode(string name, string description, bool isExpanded)
		{
			var volumeNode = new SeismicDataNode
			{
				Name = name,
				NodeType = "SeismicVolume",
				IsExpanded = isExpanded,
				IconKey = "🌊",
				Description = description,
				ResourcePath = "SeismicBody.png",
				IsChecked = true
			};

			// Inline剖面
			var inlineFolder = new SeismicDataNode
			{
				Name = "主测线 (Inline)",
				NodeType = "InlineFolder",
				IconKey = "📁",
				IsExpanded = false
			};
			inlineFolder.Children.Add(new SeismicDataNode { Name = "IL-2400", NodeType = "Inline", IconKey = "➖", Description = "Inline 2400", IsChecked = true });
			inlineFolder.Children.Add(new SeismicDataNode { Name = "IL-2500", NodeType = "Inline", IconKey = "➖", Description = "Inline 2500", IsChecked = true });
			inlineFolder.Children.Add(new SeismicDataNode { Name = "IL-2600", NodeType = "Inline", IconKey = "➖", Description = "Inline 2600", IsChecked = false });
			volumeNode.Children.Add(inlineFolder);

			// Crossline剖面
			var crosslineFolder = new SeismicDataNode
			{
				Name = "联络线 (Crossline)",
				NodeType = "CrosslineFolder",
				IconKey = "📁",
				IsExpanded = false
			};
			crosslineFolder.Children.Add(new SeismicDataNode { Name = "XL-1700", NodeType = "Crossline", IconKey = "➖", Description = "Crossline 1700", IsChecked = true });
			crosslineFolder.Children.Add(new SeismicDataNode { Name = "XL-1800", NodeType = "Crossline", IconKey = "➖", Description = "Crossline 1800", IsChecked = true });
			crosslineFolder.Children.Add(new SeismicDataNode { Name = "XL-1900", NodeType = "Crossline", IconKey = "➖", Description = "Crossline 1900", IsChecked = false });
			volumeNode.Children.Add(crosslineFolder);

			// 时间切片
			var timeSliceFolder = new SeismicDataNode
			{
				Name = "时间切片 (Time Slice)",
				NodeType = "TimeSliceFolder",
				IconKey = "📁",
				IsExpanded = false
			};
			timeSliceFolder.Children.Add(new SeismicDataNode { Name = "T=1850ms", NodeType = "TimeSlice", IconKey = "⏱️", Description = "1850毫秒", IsChecked = true });
			timeSliceFolder.Children.Add(new SeismicDataNode { Name = "T=2100ms", NodeType = "TimeSlice", IconKey = "⏱️", Description = "2100毫秒", IsChecked = false });
			timeSliceFolder.Children.Add(new SeismicDataNode { Name = "T=2350ms", NodeType = "TimeSlice", IconKey = "⏱️", Description = "2350毫秒", IsChecked = false });
			volumeNode.Children.Add(timeSliceFolder);

			return volumeNode;
		}

		/// <summary>
		/// 创建层位节点
		/// </summary>
		private SeismicDataNode CreateHorizonNode(string name, string color, double avgTime)
		{
			return new SeismicDataNode
			{
				Name = name,
				NodeType = "Horizon",
				IconKey = "📏",
				Description = $"平均时间: {avgTime}ms",
				IsChecked = true
			};
		}

		/// <summary>
		/// 选择节点命令
		/// </summary>
		[RelayCommand]
		public void SelectNode(SeismicDataNode? node)
		{
			if (node == null)
				return;

			SelectedNode = node;

			// 根据节点类型触发不同的事件
			if (node.NodeType == "SeismicVolume")
			{
				SeismicVolumeSelected?.Invoke(node);
			}
			else if (node.NodeType is "Section" or "Inline" or "Crossline")
			{
				SectionSelected?.Invoke(node);
			}
		}

		/// <summary>
		/// 切换节点勾选状态
		/// </summary>
		[RelayCommand]
		public void ToggleNodeChecked(SeismicDataNode? node)
		{
			if (node != null)
			{
				node.IsChecked = !node.IsChecked;
				NodeCheckedChanged?.Invoke(node, node.IsChecked);

				// 如果是父节点，同步子节点状态
				if (node.Children.Count > 0)
				{
					SetChildrenCheckedState(node.Children, node.IsChecked);
				}
			}
		}

		/// <summary>
		/// 设置子节点的勾选状态
		/// </summary>
		private void SetChildrenCheckedState(ObservableCollection<SeismicDataNode> nodes, bool isChecked)
		{
			foreach (var node in nodes)
			{
				node.IsChecked = isChecked;
				if (node.Children.Count > 0)
				{
					SetChildrenCheckedState(node.Children, isChecked);
				}
			}
		}

		/// <summary>
		/// 获取所有勾选的地震体名称
		/// </summary>
		public string[] GetCheckedSeismicVolumes()
		{
			var checkedVolumes = new System.Collections.Generic.List<string>();
			CollectCheckedNodes(RootNodes, "SeismicVolume", checkedVolumes);
			return checkedVolumes.ToArray();
		}

		/// <summary>
		/// 获取所有勾选的层位
		/// </summary>
		public string[] GetCheckedHorizons()
		{
			var checkedHorizons = new System.Collections.Generic.List<string>();
			CollectCheckedNodes(RootNodes, "Horizon", checkedHorizons);
			return checkedHorizons.ToArray();
		}

		/// <summary>
		/// 递归收集勾选的节点
		/// </summary>
		private void CollectCheckedNodes(ObservableCollection<SeismicDataNode> nodes, string nodeType, System.Collections.Generic.List<string> result)
		{
			foreach (var node in nodes)
			{
				if (node.NodeType == nodeType && node.IsChecked)
				{
					result.Add(node.Name);
				}
				if (node.Children.Count > 0)
				{
					CollectCheckedNodes(node.Children, nodeType, result);
				}
			}
		}

		/// <summary>
		/// 刷新数据
		/// </summary>
		[RelayCommand]
		public void RefreshData()
		{
			LoadSeismicDataStructure();
		}

		/// <summary>
		/// 展开所有
		/// </summary>
		[RelayCommand]
		public void ExpandAll()
		{
			ExpandAllNodes(RootNodes);
		}

		/// <summary>
		/// 折叠所有
		/// </summary>
		[RelayCommand]
		public void CollapseAll()
		{
			CollapseAllNodes(RootNodes);
		}

		private void ExpandAllNodes(ObservableCollection<SeismicDataNode> nodes)
		{
			foreach (var node in nodes)
			{
				node.IsExpanded = true;
				if (node.Children.Count > 0)
				{
					ExpandAllNodes(node.Children);
				}
			}
		}

		private void CollapseAllNodes(ObservableCollection<SeismicDataNode> nodes)
		{
			foreach (var node in nodes)
			{
				node.IsExpanded = false;
				if (node.Children.Count > 0)
				{
					CollapseAllNodes(node.Children);
				}
			}
		}
	}
}
