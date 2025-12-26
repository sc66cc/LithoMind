using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 工程结构节点模型 - 静态硬编码数据
	/// </summary>
	public partial class ProjectNode : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private string _fullPath = string.Empty;

		[ObservableProperty]
		private bool _isDirectory;

		[ObservableProperty]
		private bool _isExpanded;

		[ObservableProperty]
		private string _iconKey = "📄";

		[ObservableProperty]
		private string _fileExtension = string.Empty;

		[ObservableProperty]
		private string _fileSize = string.Empty;

		[ObservableProperty]
		private ObservableCollection<ProjectNode> _children = new();

		/// <summary>
		/// 图层是否可见 - 用于工区平面图图层控制
		/// </summary>
		[ObservableProperty]
		private bool _isLayerVisible = true;

		/// <summary>
		/// 图层可见性变化事件
		/// </summary>
		public event Action<ProjectNode, bool>? LayerVisibilityChanged;

		/// <summary>
		/// 当图层可见性变化时调用
		/// </summary>
		partial void OnIsLayerVisibleChanged(bool value)
		{
			LayerVisibilityChanged?.Invoke(this, value);
		}

		/// <summary>
		/// 节点类型描述
		/// </summary>
		public string TypeDescription => IsDirectory ? "文件夹" : GetFileTypeDescription();

		private string GetFileTypeDescription()
		{
			return FileExtension.ToLowerInvariant() switch
			{
				".las" => "测井曲线文件",
				".sgy" or ".segy" => "地震数据文件",
				".txt" => "文本文件",
				".pdf" => "PDF文档",
				".doc" or ".docx" => "Word文档",
				".xls" or ".xlsx" => "Excel表格",
				".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" => "图片文件",
				".lmproj" => "LithoMind项目文件",
				_ => "未知文件类型"
			};
		}
	}

	/// <summary>
	/// 工程结构目录视图模型
	/// 可复用组件 - 用于数据管理和其他功能分区
	/// </summary>
	public partial class ProjectFilesViewModel : PageViewModelBase
	{
		/// <summary>
		/// 文件选择事件 - 用于通知预览区域更新
		/// </summary>
		public event Action<ProjectNode>? FileSelected;

		/// <summary>
		/// 图层可见性变化事件 - 用于通知工区平面图更新图层显示
		/// </summary>
		public event Action<string, bool>? LayerVisibilityChanged;

		/// <summary>
		/// 是否显示图层复选框
		/// </summary>
		[ObservableProperty]
		private bool _showLayerCheckBoxes;

		/// <summary>
		/// 根节点集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<ProjectNode> _rootNodes = new();

		/// <summary>
		/// 当前选中的节点
		/// </summary>
		[ObservableProperty]
		private ProjectNode? _selectedNode;

		/// <summary>
		/// 当前工程名称
		/// </summary>
		[ObservableProperty]
		private string _projectName = "LithoMind演示工程";

		/// <summary>
		/// 当前工程路径
		/// </summary>
		[ObservableProperty]
		private string _projectPath = @"D:\Projects\LithoMind\Demo";

		public ProjectFilesViewModel()
		{
			Id = "ProjectFiles";
			Title = "工程结构目录";
			IconKey = "📂";
			Order = 2;

			// 加载静态硬编码的工程结构
			LoadStaticProjectStructure();
		}

		/// <summary>
		/// 加载静态工程结构 - 硬编码数据用于原型演示
		/// </summary>
		private void LoadStaticProjectStructure()
		{
			RootNodes.Clear();

			// 创建工程根节点
			var projectRoot = new ProjectNode
			{
				Name = ProjectName,
				FullPath = ProjectPath,
				IsDirectory = true,
				IsExpanded = true,
				IconKey = "📦"
			};

			// 1. 测井数据文件夹
			var wellLogsFolder = CreateFolder("测井数据", "WellLogs", "📊");
			wellLogsFolder.Children.Add(CreateFile("Well_A1_GR.las", "WellLogs/Well_A1_GR.las", ".las", "156 KB"));
			wellLogsFolder.Children.Add(CreateFile("Well_A2_DEN.las", "WellLogs/Well_A2_DEN.las", ".las", "142 KB"));
			wellLogsFolder.Children.Add(CreateFile("Well_B1_RT.las", "WellLogs/Well_B1_RT.las", ".las", "198 KB"));
			wellLogsFolder.Children.Add(CreateFile("综合测井曲线.txt", "WellLogs/综合测井曲线.txt", ".txt", "45 KB"));
			projectRoot.Children.Add(wellLogsFolder);

			// 2. 地震数据文件夹
			var seismicFolder = CreateFolder("地震数据", "Seismic", "🥓");
			seismicFolder.Children.Add(CreateFile("3D_Survey_Area1.sgy", "Seismic/3D_Survey_Area1.sgy", ".sgy", "2.3 GB"));
			seismicFolder.Children.Add(CreateFile("2D_Line_001.segy", "Seismic/2D_Line_001.segy", ".segy", "856 MB"));
			seismicFolder.Children.Add(CreateFile("地震剖面说明.txt", "Seismic/地震剖面说明.txt", ".txt", "12 KB"));
			projectRoot.Children.Add(seismicFolder);

			// 3. 地质图件文件夹
			var mapsFolder = CreateFolder("地质图件", "Maps", "🗺️");
			mapsFolder.Children.Add(CreateFile("构造图.shp", "Maps/构造图.shp", ".shp", "2.1 MB"));
			mapsFolder.Children.Add(CreateFile("沉积相图.shp", "Maps/沉积相图.shp", ".shp", "1.8 MB"));
			mapsFolder.Children.Add(CreateFile("储层分布图.shp", "Maps/储层分布图.shp", ".shp", "4.5 MB"));
			mapsFolder.Children.Add(CreateFile("地层对比图.wlp", "Maps/地层对比图.wlp", ".wlp", "890 KB"));
			projectRoot.Children.Add(mapsFolder);

			// 4. 文档资料文件夹
			var docsFolder = CreateFolder("文档资料", "Documents", "📚");
			docsFolder.Children.Add(CreateFile("项目报告.pdf", "Documents/项目报告.pdf", ".pdf", "5.6 MB"));
			docsFolder.Children.Add(CreateFile("技术方案.docx", "Documents/技术方案.docx", ".docx", "2.3 MB"));
			docsFolder.Children.Add(CreateFile("数据统计表.xlsx", "Documents/数据统计表.xlsx", ".xlsx", "1.2 MB"));
			docsFolder.Children.Add(CreateFile("工作日志.txt", "Documents/工作日志.txt", ".txt", "28 KB"));
			projectRoot.Children.Add(docsFolder);

			// 5. 分析结果文件夹
			var resultsFolder = CreateFolder("分析结果", "Results", "📈");
			resultsFolder.Children.Add(CreateFile("岩性分析结果.txt", "Results/岩性分析结果.txt", ".txt", "34 KB"));
			resultsFolder.Children.Add(CreateFile("储层预测图.png", "Results/储层预测图.png", ".png", "3.2 MB"));
			resultsFolder.Children.Add(CreateFile("井震对比图.jpg", "Results/井震对比图.jpg", ".jpg", "2.7 MB"));
			resultsFolder.Children.Add(CreateFile("T1层岩相古地理图.pdf", "Results/井震对比图.pdf", ".pdf", "20.7 MB"));

			projectRoot.Children.Add(resultsFolder);

			// 6. 项目配置文件
			projectRoot.Children.Add(CreateFile("工程文件说明.txt", "工程文件说明.txt", ".txt", "2 KB"));

			RootNodes.Add(projectRoot);
		}

		/// <summary>
		/// 创建文件夹节点
		/// </summary>
		private ProjectNode CreateFolder(string name, string path, string icon = "📁")
		{
			return new ProjectNode
			{
				Name = name,
				FullPath = System.IO.Path.Combine(ProjectPath, path),
				IsDirectory = true,
				IsExpanded = false,
				IconKey = icon
			};
		}

		/// <summary>
		/// 创建文件节点
		/// </summary>
		private ProjectNode CreateFile(string name, string path, string extension, string size)
		{
			return new ProjectNode
			{
				Name = name,
				FullPath = System.IO.Path.Combine(ProjectPath, path),
				IsDirectory = false,
				IconKey = GetFileIcon(extension),
				FileExtension = extension,
				FileSize = size
			};
		}

		/// <summary>
		/// 根据文件扩展名获取图标
		/// </summary>
		private static string GetFileIcon(string extension)
		{
			return extension.ToLowerInvariant() switch
			{
				".las" => "📊",
				".sgy" or ".segy" => "🥓",
				".txt" => "📝",
				".pdf" => "📕",
				".doc" or ".docx" => "📘",
				".xls" or ".xlsx" => "📗",
				".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" => "🖼️",
				".lmproj" => "📂",
				_ => "📄"
			};
		}

		/// <summary>
		/// 选择节点命令
		/// </summary>
		[RelayCommand]
		public void SelectNode(ProjectNode? node)
		{
			if (node == null)
				return;

			SelectedNode = node;

			// 如果是文件，触发文件选择事件
			if (!node.IsDirectory)
			{
				FileSelected?.Invoke(node);
			}
		}

		/// <summary>
		/// 刷新工程结构命令
		/// </summary>
		[RelayCommand]
		public void RefreshProject()
		{
			LoadStaticProjectStructure();
		}

		/// <summary>
		/// 展开所有节点命令
		/// </summary>
		[RelayCommand]
		public void ExpandAll()
		{
			ExpandAllNodes(RootNodes);
		}

		/// <summary>
		/// 折叠所有节点命令
		/// </summary>
		[RelayCommand]
		public void CollapseAll()
		{
			CollapseAllNodes(RootNodes);
		}

		private void ExpandAllNodes(ObservableCollection<ProjectNode> nodes)
		{
			foreach (var node in nodes)
			{
				if (node.IsDirectory)
				{
					node.IsExpanded = true;
					ExpandAllNodes(node.Children);
				}
			}
		}

		private void CollapseAllNodes(ObservableCollection<ProjectNode> nodes)
		{
			foreach (var node in nodes)
			{
				if (node.IsDirectory)
				{
					node.IsExpanded = false;
					CollapseAllNodes(node.Children);
				}
			}
		}

		/// <summary>
		/// 设置图层复选框显示状态
		/// </summary>
		public void SetLayerCheckBoxVisibility(bool visible)
		{
			ShowLayerCheckBoxes = visible;
		}

		/// <summary>
		/// 图层可见性变化命令
		/// </summary>
		[RelayCommand]
		public void ToggleLayerVisibility(ProjectNode? node)
		{
			if (node != null)
			{
				// 触发事件通知工区平面图
				LayerVisibilityChanged?.Invoke(node.FullPath, node.IsLayerVisible);
			}
		}

		/// <summary>
		/// 注册节点的图层可见性变化事件
		/// </summary>
		private void RegisterLayerVisibilityEvents(ObservableCollection<ProjectNode> nodes)
		{
			foreach (var node in nodes)
			{
				node.LayerVisibilityChanged += (n, visible) =>
				{
					LayerVisibilityChanged?.Invoke(n.FullPath, visible);
				};

				if (node.Children.Count > 0)
				{
					RegisterLayerVisibilityEvents(node.Children);
				}
			}
		}
	}
}
