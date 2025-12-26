using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Layouts
{
	/// <summary>
	/// LithoMind Dock 布局工厂
	/// 支持VSCode风格的标签页拖拽分组、智能停靠、悬浮窗口等功能
	/// </summary>
	public class LithoMindDockFactory : Factory
	{
		private readonly object _context;
		private IRootDock? _rootDock;

		public LithoMindDockFactory(object context)
		{
			_context = context;
		}

		// 默认布局（可以是空的，或者指向第一个模块）
		public override IRootDock CreateLayout()
		{
			return CreateLayoutForModule("DataManager");
		}

		// 🔥 核心：根据模块ID创建不同的布局
		public IRootDock CreateLayoutForModule(string moduleId)
		{
			ProportionalDock mainLayout;

			// 根据不同模块创建不同的布局结构
			switch (moduleId)
			{
				case "DataManager":
					mainLayout = CreateDataManagerLayout();
					break;

				case "SingleWell":
					mainLayout = CreateSingleWellLayout();
					break;

				case "Seismic":
					mainLayout = CreateSeismicLayout();
					break;

				case "Mapping":
					mainLayout = CreateMappingLayout();
					break;

				case "Stratigraphy":
					mainLayout = CreateSimpleLayout(new StratigraphyViewModel());
					break;

				default:
					mainLayout = CreateDataManagerLayout();
					break;
			}

			var root = new RootDock
			{
				Id = "Root",
				Title = moduleId,
				IsCollapsable = false,
				ActiveDockable = mainLayout,
				DefaultDockable = mainLayout,
				VisibleDockables = CreateList<IDockable>(mainLayout),
				// 启用窗口管理功能
				CanFloat = true,
				CanPin = true,
				CanClose = true
			};

			_rootDock = root;
			return root;
		}

		/// <summary>
		/// 重写 InitLayout 以确保正确初始化 DockState
		/// </summary>
		public override void InitLayout(IDockable layout)
		{
			// 调用基类的初始化方法
			base.InitLayout(layout);

			// 确保 DockState 正确初始化
			if (layout is IRootDock rootDock)
			{
				_rootDock = rootDock;
				
				// 设置默认活动面板
				if (rootDock.DefaultDockable != null)
				{
					rootDock.ActiveDockable = rootDock.DefaultDockable;
				}

				// 设置焦点停靠面板
				SetFocusedDockable(rootDock, rootDock.DefaultDockable);
			}
		}

		private ProportionalDock CreateDataManagerLayout()
		{
			// 右侧：数据预览区域 - 使用FilePreviewViewModel实现文件预览
			var previewVM = new FilePreviewViewModel();

			// 工区平面图 - 支持缩放/拖拽
			var workAreaMapVM = new WorkAreaMapViewModel();

			// 左侧：本地文件目录面板 - 使用LocalFilesViewModel实现真实文件系统访问
			var localFilesVM = new LocalFilesViewModel();
			
			// 建立本地文件目录和预览区域的事件连接
			localFilesVM.FileSelected += async (fileNode) =>
			{
				await previewVM.PreviewLocalFileAsync(fileNode);
			};
		
			var leftDock = new ToolDock
			{
				Id = "LocalFilesPane",
				Title = "本地文件目录",
				Proportion = 0.15,
				Alignment = Alignment.Left,
				ActiveDockable = localFilesVM,
				VisibleDockables = CreateList<IDockable>(localFilesVM),
				GripMode = GripMode.Visible,
				CanFloat = true,
				CanPin = true,
				CanClose = false,  // 本地文件面板不允许关闭
				IsCollapsable = true
			};
			
			// 中间：工程结构目录面板 - 使用ProjectFilesViewModel实现工程结构显示
			var projectFilesVM = new ProjectFilesViewModel();
			
			// 建立工程目录和预览区域的事件连接
			projectFilesVM.FileSelected += async (fileNode) =>
			{
				await previewVM.PreviewFileAsync(fileNode);
			};

			// 建立工程目录和工区平面图的图层控制事件连接
			projectFilesVM.LayerVisibilityChanged += (layerPath, isVisible) =>
			{
				workAreaMapVM.SetLayerVisibility(layerPath, isVisible);
			};

			// 当工区平面图被激活时，显示图层复选框
			// 注意：实际应用中可以通过监听标签页切换事件来实现
			// 当前原型阶段默认显示图层复选框
			projectFilesVM.SetLayerCheckBoxVisibility(true);
			
			var middleDock = new ToolDock
			{
				Id = "ProjectFilePane",
				Title = "工程结构目录",
				Proportion = 0.15,
				Alignment = Alignment.Left,
				ActiveDockable = projectFilesVM,
				VisibleDockables = CreateList<IDockable>(projectFilesVM),
				GripMode = GripMode.Visible,
				CanFloat = true,
				CanPin = true,
				CanClose = false,  // 工程目录面板不允许关闭
				IsCollapsable = true
			};
			
			// 右侧：数据预览区域 + 工区平面图标签页
			var rightDock = new DocumentDock
			{
				Id = "MainDocumentPane",
				Title = "预览区域",
				Proportion = double.NaN,
				IsCollapsable = false,
				ActiveDockable = previewVM,
				VisibleDockables = CreateList<IDockable>(previewVM, workAreaMapVM),
				CanFloat = true,
				CanPin = true,
				CanClose = true,
				CanCreateDocument = true
			};
			
			var splitter1 = new ProportionalDockSplitter
			{
				Id = "Splitter1",
				Title = "Splitter"
			};
			
			var splitter2 = new ProportionalDockSplitter
			{
				Id = "Splitter2",
				Title = "Splitter"
			};
			
			// 水平布局：左侧Dock + 分隔条 + 中间Dock + 分隔条 + 右侧Dock
			var layout = new ProportionalDock
			{
				Id = "DataManagerMainLayout",
				Orientation = Orientation.Horizontal,
				VisibleDockables = CreateList<IDockable>(
					leftDock, 
					splitter1, 
					middleDock, 
					splitter2, 
					rightDock
				)
			};
			
			return layout;
		}

		/// <summary>
		/// 其他模块的简单布局：单一文档区域
		/// </summary>
		private ProportionalDock CreateSimpleLayout(IDockable viewModel)
		{
			var documentDock = new DocumentDock
			{
				Id = "MainDocument",
				Title = "主工作区",
				Proportion = double.NaN,
				ActiveDockable = viewModel,
				VisibleDockables = CreateList<IDockable>(viewModel)
			};

			var layout = new ProportionalDock
			{
				Id = "SimpleLayout",
				Orientation = Orientation.Horizontal,
				VisibleDockables = CreateList<IDockable>(documentDock)
			};

			return layout;
		}

		/// <summary>
		/// 地震综合布局
		/// 左侧: 地震工程结构目录 (15%)
		/// 中间: 地震体数据、地震解释剖面标签页
		/// 右侧: 层位信息属性窗口
		/// </summary>
		private ProportionalDock CreateSeismicLayout()
		{
			// 左侧：地震工程结构目录
			var seismicProjectTreeVM = new SeismicProjectTreeViewModel();
			
			var leftDock = new ToolDock
			{
				Id = "SeismicProjectTreePane",
				Title = "地震工程目录",
				Proportion = 0.15,
				Alignment = Alignment.Left,
				ActiveDockable = seismicProjectTreeVM,
				VisibleDockables = CreateList<IDockable>(seismicProjectTreeVM),
				GripMode = GripMode.Visible,
				CanFloat = true,
				CanPin = true,
				CanClose = false,
				IsCollapsable = true
			};

			// 中间：地震体数据和地震解释剖面标签页
			var seismicBodyVM = new SeismicBodyViewModel();
			var seismicInterpretationVM = new SeismicInterpretationViewModel();

			// 右侧：层位信息属性窗口
			var seismicPropertyVM = new SeismicPropertyViewModel();

			var middleDock = new DocumentDock
			{
				Id = "SeismicDocumentPane",
				Title = "地震数据显示区",
				Proportion = double.NaN,
				IsCollapsable = false,
				ActiveDockable = seismicBodyVM,
				VisibleDockables = CreateList<IDockable>(seismicBodyVM, seismicInterpretationVM),
				CanFloat = true,
				CanPin = true,
				CanClose = true,
				CanCreateDocument = true
			};

			var rightDock = new ToolDock
			{
				Id = "SeismicPropertyPane",
				Title = "层位属性",
				Proportion = 0.18,
				Alignment = Alignment.Right,
				ActiveDockable = seismicPropertyVM,
				VisibleDockables = CreateList<IDockable>(seismicPropertyVM),
				GripMode = GripMode.Visible,
				CanFloat = true,
				CanPin = true,
				CanClose = false,
				IsCollapsable = true
			};

			var splitter1 = new ProportionalDockSplitter
			{
				Id = "SeismicSplitter1",
				Title = "Splitter"
			};

			var splitter2 = new ProportionalDockSplitter
			{
				Id = "SeismicSplitter2",
				Title = "Splitter"
			};

			// 水平布局：左侧工程目录 + 中间文档区 + 右侧属性窗口
			var layout = new ProportionalDock
			{
				Id = "SeismicMainLayout",
				Orientation = Orientation.Horizontal,
				VisibleDockables = CreateList<IDockable>(
					leftDock,
					splitter1,
					middleDock,
					splitter2,
					rightDock
				)
			};

			return layout;
		}

		/// <summary>
		/// 井综合数据布局
		/// 左侧: 工程结构目录 (1/3)
		/// 中间: 单井综合柱状图、联井剖面图等标签页
		/// 右侧: 属性窗口 (JSON数据显示)
		/// </summary>
		private ProportionalDock CreateSingleWellLayout()
		{
			// 左侧：工程结构目录
			var wellProjectTreeVM = new WellProjectTreeViewModel();
			
			var leftDock = new ToolDock
			{
				Id = "WellProjectTreePane",
				Title = "工程结构目录",
				Proportion = 0.15,
				Alignment = Alignment.Left,
				ActiveDockable = wellProjectTreeVM,
				VisibleDockables = CreateList<IDockable>(wellProjectTreeVM),
				GripMode = GripMode.Visible,
				CanFloat = true,
				CanPin = true,
				CanClose = false,
				IsCollapsable = true
			};

			// 中间：单井综合柱状图和联井剖面图标签页
			var wellColumnVM = new WellColumnViewModel();
			var wellCorrelationVM = new WellCorrelationViewModel();

			// 右侧：属性窗口
			var propertyPanelVM = new PropertyPanelViewModel();

			// 建立事件连接：选择井时加载对应柱状图
			wellProjectTreeVM.WellSelected += (wellName) =>
			{
				wellColumnVM.LoadWellData(wellName);
			};

			var middleDock = new DocumentDock
			{
				Id = "WellDocumentPane",
				Title = "井数据显示区",
				Proportion = double.NaN,
				IsCollapsable = false,
				ActiveDockable = wellColumnVM,
				VisibleDockables = CreateList<IDockable>(wellColumnVM, wellCorrelationVM),
				CanFloat = true,
				CanPin = true,
				CanClose = true,
				CanCreateDocument = true
			};

			var rightDock = new ToolDock
			{
				Id = "PropertyPane",
				Title = "属性窗口",
				Proportion = 0.15,
				Alignment = Alignment.Right,
				ActiveDockable = propertyPanelVM,
				VisibleDockables = CreateList<IDockable>(propertyPanelVM),
				GripMode = GripMode.Visible,
				CanFloat = true,
				CanPin = true,
				CanClose = false,
				IsCollapsable = true
			};

			var splitter1 = new ProportionalDockSplitter
			{
				Id = "SingleWellSplitter1",
				Title = "Splitter"
			};

			var splitter2 = new ProportionalDockSplitter
			{
				Id = "SingleWellSplitter2",
				Title = "Splitter"
			};

			// 水平布局：左侧工程目录 + 中间文档区 + 右侧属性窗口
			var layout = new ProportionalDock
			{
				Id = "SingleWellMainLayout",
				Orientation = Orientation.Horizontal,
				VisibleDockables = CreateList<IDockable>(
					leftDock,
					splitter1,
					middleDock,
					splitter2,
					rightDock
				)
			};

			return layout;
		}

		/// <summary>
		/// 编图制图布局
		/// 左侧: 图层管理器 (15%)
		/// 中间: 砂体等厚图/砂地比图/碳酸盐岩含量图/岩相古地理图
		/// 右侧: GIS工具栏和属性窗口 (15%)
		/// </summary>
		private ProportionalDock CreateMappingLayout()
		{
			// 左侧：图层管理器（类ArcGIS风格）
			var mappingLayerVM = new MappingLayerViewModel();
			
			var leftDock = new ToolDock
			{
				Id = "MappingLayerPane",
				Title = "图层管理",
				Proportion = 0.15,
				Alignment = Alignment.Left,
				ActiveDockable = mappingLayerVM,
				VisibleDockables = CreateList<IDockable>(mappingLayerVM),
				GripMode = GripMode.Visible,
				CanFloat = true,
				CanPin = true,
				CanClose = false,
				IsCollapsable = true
			};

			// 中间：四个制图标签页
			var sandBodyThicknessVM = new SandBodyThicknessViewModel();
			var sandRatioVM = new SandRatioViewModel();
			var carbonateContentVM = new CarbonateContentViewModel();
			var lithofaciesVM = new LithofaciesPaleogeographyViewModel();

			// 右侧：GIS工具栏和属性窗口
			var mappingToolsVM = new MappingToolsViewModel();

			// 建立图层选择事件连接
			mappingLayerVM.LayerSelected += (layer) =>
			{
				mappingToolsVM.SetSelectedLayer(layer);
			};

			var middleDock = new DocumentDock
			{
				Id = "MappingDocumentPane",
				Title = "制图区域",
				Proportion = double.NaN,
				IsCollapsable = false,
				ActiveDockable = sandBodyThicknessVM,
				VisibleDockables = CreateList<IDockable>(
					sandBodyThicknessVM, 
					sandRatioVM, 
					carbonateContentVM, 
					lithofaciesVM),
				CanFloat = true,
				CanPin = true,
				CanClose = true,
				CanCreateDocument = true
			};

			var rightDock = new ToolDock
			{
				Id = "MappingToolsPane",
				Title = "工具栏",
				Proportion = 0.15,
				Alignment = Alignment.Right,
				ActiveDockable = mappingToolsVM,
				VisibleDockables = CreateList<IDockable>(mappingToolsVM),
				GripMode = GripMode.Visible,
				CanFloat = true,
				CanPin = true,
				CanClose = false,
				IsCollapsable = true
			};

			var splitter1 = new ProportionalDockSplitter
			{
				Id = "MappingSplitter1",
				Title = "Splitter"
			};

			var splitter2 = new ProportionalDockSplitter
			{
				Id = "MappingSplitter2",
				Title = "Splitter"
			};

			// 水平布局：左侧图层管理 + 中间制图区 + 右侧工具栏
			var layout = new ProportionalDock
			{
				Id = "MappingMainLayout",
				Orientation = Orientation.Horizontal,
				VisibleDockables = CreateList<IDockable>(
					leftDock,
					splitter1,
					middleDock,
					splitter2,
					rightDock
				)
			};

			return layout;
		}

		/// <summary>
		/// 在当前布局中激活指定的文档标签页
		/// </summary>
		/// <param name="documentId">文档ID（如 "WorkAreaMap", "WellColumn", "SeismicBody" 等）</param>
		public void ActivateDocumentInCurrentLayout(string documentId)
		{
			if (_rootDock == null) return;

			// 递归查找并激活DocumentDock中的文档
			ActivateDocumentRecursive(_rootDock, documentId);
		}

		/// <summary>
		/// 递归查找并激活指定文档
		/// </summary>
		private bool ActivateDocumentRecursive(IDockable dockable, string documentId)
		{
			// 如果是DocumentDock，查找目标文档
			if (dockable is DocumentDock documentDock && documentDock.VisibleDockables != null)
			{
				foreach (var doc in documentDock.VisibleDockables)
				{
					if (doc?.Id == documentId)
					{
						// 找到目标文档，激活它
						documentDock.ActiveDockable = doc;
						SetFocusedDockable(_rootDock, doc);
						return true;
					}
				}
			}

			// 递归查找子面板
			if (dockable is IDock dock && dock.VisibleDockables != null)
			{
				foreach (var child in dock.VisibleDockables)
				{
					if (child != null && ActivateDocumentRecursive(child, documentId))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// 显示井选择对话框并激活联井剖面视图
		/// </summary>
		public void ShowWellSelectionAndActivateCorrelation()
		{
			if (_rootDock == null) return;

			// 查找联井剖面ViewModel
			var correlationVM = FindDocumentRecursive(_rootDock, "WellCorrelation") as WellCorrelationViewModel;
			
			if (correlationVM != null)
			{
				// 清空并添加井数据（A5-1, A6-5, A6-1, A7-1, A7-3）
				correlationVM.Wells.Clear();
				correlationVM.Wells.Add(new CorrelationWell { Name = "A5-1", IsSelected = true });
				correlationVM.Wells.Add(new CorrelationWell { Name = "A6-5", IsSelected = true });
				correlationVM.Wells.Add(new CorrelationWell { Name = "A6-1", IsSelected = true });
				correlationVM.Wells.Add(new CorrelationWell { Name = "A7-1", IsSelected = true });
				correlationVM.Wells.Add(new CorrelationWell { Name = "A7-3", IsSelected = true });
				
				// 显示井选择对话框
				correlationVM.ShowWellSelector = true;
				
				// 激活联井剖面标签页
				ActivateDocumentInCurrentLayout("WellCorrelation");
			}
		}

		/// <summary>
		/// 递归查找指定文档
		/// </summary>
		private IDockable? FindDocumentRecursive(IDockable dockable, string documentId)
		{
			// 如果当前ID匹配，返回
			if (dockable?.Id == documentId)
			{
				return dockable;
			}

			// 递归查找子面板
			if (dockable is IDock dock && dock.VisibleDockables != null)
			{
				foreach (var child in dock.VisibleDockables)
				{
					if (child != null)
					{
						var found = FindDocumentRecursive(child, documentId);
						if (found != null)
						{
							return found;
						}
					}
				}
			}

			return null;
		}
	}
}