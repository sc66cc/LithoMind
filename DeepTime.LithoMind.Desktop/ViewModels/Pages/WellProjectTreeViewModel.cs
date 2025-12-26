using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 井数据节点
	/// </summary>
	public partial class WellDataNode : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private string _nodeType = string.Empty; // Well, WellLog, Marker, Section

		[ObservableProperty]
		private bool _isExpanded;

		[ObservableProperty]
		private string _iconKey = "📁";

		[ObservableProperty]
		private string _description = string.Empty;

		[ObservableProperty]
		private ObservableCollection<WellDataNode> _children = new();

		[ObservableProperty]
		private bool _isSelected;

		/// <summary>
		/// 是否勾选（控制是否显示在柱状图/剖面图中）
		/// </summary>
		[ObservableProperty]
		private bool _isChecked = true;

		/// <summary>
		/// 是否显示CheckBox（只有特定类型节点显示）
		/// </summary>
		public bool ShowCheckBox => NodeType is "Well" or "Log" or "Marker" or "Lithology" or "Facies" or "Section" or "Horizon";

		/// <summary>
		/// 是否可以拖拽
		/// </summary>
		public bool CanDrag => NodeType is "Well" or "Log" or "Section";
	}

	/// <summary>
	/// 井综合数据页面的工程结构目录视图模型
	/// </summary>
	public partial class WellProjectTreeViewModel : PageViewModelBase
	{
		/// <summary>
		/// 根节点集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<WellDataNode> _rootNodes = new();

		/// <summary>
		/// 当前选中的节点
		/// </summary>
		[ObservableProperty]
		private WellDataNode? _selectedNode;

		/// <summary>
		/// 井选择事件
		/// </summary>
		public event Action<string>? WellSelected;

		/// <summary>
		/// 数据项选择事件
		/// </summary>
		public event Action<WellDataNode>? DataItemSelected;

		/// <summary>
		/// 勾选状态变化事件 - 通知柱状图/剖面图更新
		/// </summary>
		public event Action<WellDataNode, bool>? NodeCheckedChanged;

		public WellProjectTreeViewModel()
		{
			Id = "WellProjectTree";
			Title = "工程结构目录";
			IconKey = "📂";
			Order = 8;

			// 加载井数据结构
			LoadWellDataStructure();
		}

		/// <summary>
		/// 加载井数据结构
		/// </summary>
		private void LoadWellDataStructure()
		{
			RootNodes.Clear();

			// 创建项目根节点
			var projectRoot = new WellDataNode
			{
				Name = "LithoMind演示工程",
				NodeType = "Project",
				IsExpanded = true,
				IconKey = "📦"
			};

			// 井数据文件夹
			var wellsFolder = new WellDataNode
			{
				Name = "井数据",
				NodeType = "Folder",
				IsExpanded = true,
				IconKey = "🛢️"
			};

			// 添加多口井
			wellsFolder.Children.Add(CreateWellNode("Well-A5-1", "探井", true));
			wellsFolder.Children.Add(CreateWellNode("Well-A6-5", "探井", false));
			wellsFolder.Children.Add(CreateWellNode("Well-A6-1", "探井", false));
			wellsFolder.Children.Add(CreateWellNode("Well-A7-1", "探井", false));
			wellsFolder.Children.Add(CreateWellNode("Well-A7-3", "探井", false));

			projectRoot.Children.Add(wellsFolder);

			// 联井剖面文件夹
			var sectionsFolder = new WellDataNode
			{
				Name = "联井剖面",
				NodeType = "Folder",
				IsExpanded = true,
				IconKey = "📈"
			};

			sectionsFolder.Children.Add(new WellDataNode
			{
				Name = "联井剖面-01",
				NodeType = "Section",
				IconKey = "📊",
				Description = "A5-1 -> A6-5 -> A7-1 ...剖面"
			});

			sectionsFolder.Children.Add(new WellDataNode
			{
				Name = "联井剖面-02",
				NodeType = "Section",
				IconKey = "📊",
				Description = "A5-1 -> A6-5 -> A7-3 ...剖面"
			});

			projectRoot.Children.Add(sectionsFolder);

			// 层位数据文件夹
			var horizonsFolder = new WellDataNode
			{
				Name = "层位数据",
				NodeType = "Folder",
				IsExpanded = false,
				IconKey = "📏"
			};

			horizonsFolder.Children.Add(new WellDataNode { Name = "T1层位", NodeType = "Horizon", IconKey = "➖" });
			horizonsFolder.Children.Add(new WellDataNode { Name = "T2层位", NodeType = "Horizon", IconKey = "➖" });
			horizonsFolder.Children.Add(new WellDataNode { Name = "T3层位", NodeType = "Horizon", IconKey = "➖" });

			projectRoot.Children.Add(horizonsFolder);

			RootNodes.Add(projectRoot);
		}

		/// <summary>
		/// 创建井节点
		/// </summary>
		private WellDataNode CreateWellNode(string wellName, string wellType, bool isExpanded)
		{
			var wellNode = new WellDataNode
			{
				Name = wellName,
				NodeType = "Well",
				IsExpanded = isExpanded,
				IconKey = "🛢️",
				Description = wellType
			};

			// 测井曲线
			var logsNode = new WellDataNode
			{
				Name = "测井曲线",
				NodeType = "LogFolder",
				IconKey = "📊",
				IsExpanded = false
			};
			logsNode.Children.Add(new WellDataNode { Name = "GR", NodeType = "Log", IconKey = "📈", Description = "自然伽马" });
			logsNode.Children.Add(new WellDataNode { Name = "SP", NodeType = "Log", IconKey = "📈", Description = "自然电位" });
			logsNode.Children.Add(new WellDataNode { Name = "RHOB", NodeType = "Log", IconKey = "📈", Description = "密度" });
			logsNode.Children.Add(new WellDataNode { Name = "DT", NodeType = "Log", IconKey = "📈", Description = "声波时差" });
			wellNode.Children.Add(logsNode);

			// 分层数据
			var markersNode = new WellDataNode
			{
				Name = "分层数据",
				NodeType = "MarkerFolder",
				IconKey = "📏",
				IsExpanded = false
			};
			markersNode.Children.Add(new WellDataNode { Name = "T1", NodeType = "Marker", IconKey = "➖", Description = "2520m" });
			markersNode.Children.Add(new WellDataNode { Name = "T2", NodeType = "Marker", IconKey = "➖", Description = "2680m" });
			markersNode.Children.Add(new WellDataNode { Name = "T3", NodeType = "Marker", IconKey = "➖", Description = "2850m" });
			wellNode.Children.Add(markersNode);

			// 岩性解释
			wellNode.Children.Add(new WellDataNode
			{
				Name = "岩性解释",
				NodeType = "Lithology",
				IconKey = "🪨",
				Description = "岩相分析结果"
			});

			// 沉积相解释
			wellNode.Children.Add(new WellDataNode
			{
				Name = "沉积相解释",
				NodeType = "Facies",
				IconKey = "🌊",
				Description = "沉积相分析结果"
			});

			return wellNode;
		}

		/// <summary>
		/// 选择节点命令
		/// </summary>
		[RelayCommand]
		public void SelectNode(WellDataNode? node)
		{
			if (node == null)
				return;

			SelectedNode = node;

			// 根据节点类型触发不同的事件
			if (node.NodeType == "Well")
			{
				WellSelected?.Invoke(node.Name);
			}
			else
			{
				DataItemSelected?.Invoke(node);
			}
		}

		/// <summary>
		/// 切换节点勾选状态
		/// </summary>
		[RelayCommand]
		public void ToggleNodeChecked(WellDataNode? node)
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
		private void SetChildrenCheckedState(ObservableCollection<WellDataNode> nodes, bool isChecked)
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
		/// 获取所有勾选的井名
		/// </summary>
		public string[] GetCheckedWellNames()
		{
			var checkedWells = new System.Collections.Generic.List<string>();
			CollectCheckedWells(RootNodes, checkedWells);
			return checkedWells.ToArray();
		}

		/// <summary>
		/// 递归收集勾选的井
		/// </summary>
		private void CollectCheckedWells(ObservableCollection<WellDataNode> nodes, System.Collections.Generic.List<string> result)
		{
			foreach (var node in nodes)
			{
				if (node.NodeType == "Well" && node.IsChecked)
				{
					result.Add(node.Name);
				}
				if (node.Children.Count > 0)
				{
					CollectCheckedWells(node.Children, result);
				}
			}
		}

		/// <summary>
		/// 刷新数据
		/// </summary>
		[RelayCommand]
		public void RefreshData()
		{
			LoadWellDataStructure();
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

		private void ExpandAllNodes(ObservableCollection<WellDataNode> nodes)
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

		private void CollapseAllNodes(ObservableCollection<WellDataNode> nodes)
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
