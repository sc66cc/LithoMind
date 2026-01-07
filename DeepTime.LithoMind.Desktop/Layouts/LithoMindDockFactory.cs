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
		private PropertyPanelViewModel? _propertyPanelVM;

		// 地震模块相关引用
		private ToolDock? _seismicPropertyDock;
		private PropertyPanelViewModel? _seismicPropertyPanelVM;
		private ProportionalDock? _seismicMainLayout;
		private ProportionalDockSplitter? _seismicPropertySplitter;
		private bool _seismicPropertyVisible;

		// ViewModel缓存：按模块ID和ViewModel ID缓存，避免重复创建
		private readonly Dictionary<string, Dictionary<string, IDockable>> _viewModelCache = new();

		// 布局缓存：缓存整个布局结构，避免重复创建（性能优化核心）
		private readonly Dictionary<string, IRootDock> _layoutCache = new();

		// 布局初始化标记：记录哪些布局已经初始化过，避免重复初始化
		private readonly HashSet<string> _initializedLayouts = new();

		// 事件订阅标记：记录哪些事件已经订阅，避免重复订阅导致内存泄漏
		private readonly HashSet<string> _subscribedEvents = new();

		public LithoMindDockFactory(object context)
		{
			_context = context;
		}

		/// <summary>
		/// 检查事件是否已订阅，如果未订阅则标记为已订阅
		/// </summary>
		/// <param name="eventKey">事件唯一标识（格式：模块ID_源ViewModel_目标ViewModel_事件名）</param>
		/// <returns>如果事件未订阅返回true，已订阅返回false</returns>
		private bool TrySubscribeEvent(string eventKey)
		{
			if (_subscribedEvents.Contains(eventKey))
				return false;

			_subscribedEvents.Add(eventKey);
			return true;
		}

		/// <summary>
		/// 获取或创建ViewModel（带缓存）
		/// </summary>
		private T GetOrCreateViewModel<T>(string moduleId, string viewModelId, Func<T> factory) 
			where T : class, IDockable
		{
			if (!_viewModelCache.TryGetValue(moduleId, out var moduleCache))
			{
				moduleCache = new Dictionary<string, IDockable>();
				_viewModelCache[moduleId] = moduleCache;
			}

			if (moduleCache.TryGetValue(viewModelId, out var cached) && cached is T cachedVM)
			{
				return cachedVM;
			}

			var newVM = factory();
			moduleCache[viewModelId] = newVM;
			return newVM;
		}

		/// <summary>
		/// 检查ViewModel是否是新创建的（用于判断是否需要建立事件连接）
		/// </summary>
		private bool IsViewModelNew<T>(string moduleId, string viewModelId) where T : class, IDockable
		{
			if (!_viewModelCache.TryGetValue(moduleId, out var moduleCache))
				return true;
			
			return !moduleCache.ContainsKey(viewModelId);
		}

		// 默认布局（可以是空的，或者指向第一个模块）
		public override IRootDock CreateLayout()
		{
			return CreateLayoutForModule("DataManager");
		}

		// 🔥 核心：根据模块ID创建不同的布局（带缓存优化）
		public IRootDock CreateLayoutForModule(string moduleId)
		{
			// 优先从缓存获取布局，避免重复创建
			if (_layoutCache.TryGetValue(moduleId, out var cachedLayout))
			{
				_rootDock = cachedLayout;
				return cachedLayout;
			}

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
					mainLayout = CreateStratigraphyLayout();
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

			// 缓存布局
			_layoutCache[moduleId] = root;
			_rootDock = root;
			return root;
		}

		/// <summary>
		/// 预创建所有模块的布局（可在应用启动时调用，提升首次切换速度）
		/// </summary>
		public void PreloadAllLayouts()
		{
			var moduleIds = new[] { "DataManager", "SingleWell", "Seismic", "Mapping", "Stratigraphy" };
			foreach (var moduleId in moduleIds)
			{
				if (!_layoutCache.ContainsKey(moduleId))
				{
					CreateLayoutForModule(moduleId);
				}
			}
		}

		/// <summary>
		/// 清除指定模块的布局缓存（用于需要重建布局的场景）
		/// </summary>
		public void ClearLayoutCache(string? moduleId = null)
		{
			if (string.IsNullOrEmpty(moduleId))
			{
				_layoutCache.Clear();
			}
			else
			{
				_layoutCache.Remove(moduleId);
			}
		}

		/// <summary>
		/// 隐藏地震属性窗口（切换到地震模块时默认关闭）
		/// </summary>
		public void HideSeismicPropertyPane()
		{
			if (!_seismicPropertyVisible && _seismicPropertyDock == null)
				return;

			_seismicPropertyVisible = false;
			if (_seismicMainLayout?.VisibleDockables == null || _seismicPropertyDock == null)
				return;

			var docks = _seismicMainLayout.VisibleDockables;
			if (docks.Contains(_seismicPropertyDock))
			{
				docks.Remove(_seismicPropertyDock);
			}
			if (_seismicPropertySplitter != null && docks.Contains(_seismicPropertySplitter))
			{
				docks.Remove(_seismicPropertySplitter);
			}
		}

		/// <summary>
		/// 显示地震属性窗口（标注模式时打开）
		/// </summary>
		public void ShowSeismicPropertyPane()
		{
			if (_seismicPropertyVisible)
				return;

			if (_seismicMainLayout == null || _seismicPropertyDock == null)
				return;

			var docks = _seismicMainLayout.VisibleDockables ?? CreateList<IDockable>();
			_seismicMainLayout.VisibleDockables = docks;

			if (_seismicPropertySplitter == null)
			{
				_seismicPropertySplitter = new ProportionalDockSplitter
				{
					Id = "SeismicSplitterProperty",
					Title = "Splitter"
				};
			}

			if (!docks.Contains(_seismicPropertySplitter))
			{
				docks.Add(_seismicPropertySplitter);
			}
			if (!docks.Contains(_seismicPropertyDock))
			{
				docks.Add(_seismicPropertyDock);
			}

			_seismicPropertyVisible = true;
		}

		/// <summary>
		/// 启动地震标注模式：打开属性窗并同步到标注列表
		/// </summary>
		public void StartSeismicAnnotationMode()
		{
			var seismicInterpretationVM = GetOrCreateViewModel("Seismic", "SeismicInterpretation", () => new SeismicInterpretationViewModel());
			var propertyPanelVM = _seismicPropertyPanelVM ?? GetOrCreateViewModel("Seismic", "SeismicProperty", () => new PropertyPanelViewModel());

			propertyPanelVM.SwitchToSeismicMode(seismicInterpretationVM.SectionName);
			seismicInterpretationVM.StartAnnotationMode();
			propertyPanelVM.SetSeismicAnnotations(seismicInterpretationVM.SectionName, seismicInterpretationVM.SeismicAnnotations);

			ShowSeismicPropertyPane();
		}

		/// <summary>
		/// 重写 InitLayout 以确保正确初始化 DockState（带重复初始化检查）
		/// </summary>
		public override void InitLayout(IDockable layout)
		{
			if (layout is IRootDock rootDock)
			{
				var moduleId = rootDock.Title ?? "Unknown";

				// 检查是否已经初始化过，避免重复初始化
				if (_initializedLayouts.Contains(moduleId))
				{
					// 已初始化过，只需更新引用
					_rootDock = rootDock;
					return;
				}

				// 调用基类的初始化方法
				base.InitLayout(layout);

				_rootDock = rootDock;

				// 设置默认活动面板
				if (rootDock.DefaultDockable != null)
				{
					rootDock.ActiveDockable = rootDock.DefaultDockable;
				}

				// 设置焦点停靠面板
				SetFocusedDockable(rootDock, rootDock.DefaultDockable);

				// 标记为已初始化
				_initializedLayouts.Add(moduleId);
			}
			else
			{
				// 非 RootDock 类型，直接调用基类方法
				base.InitLayout(layout);
			}
		}

		private ProportionalDock CreateDataManagerLayout()
		{
			const string moduleId = "DataManager";
			// 使用缓存获取或创建ViewModel
			// 右侧：数据预览区域 - 使用FilePreviewViewModel实现文件预览
			var previewVM = GetOrCreateViewModel(moduleId, "FilePreview", () => new FilePreviewViewModel());

			// 工区平面图 - 支持缩放/拖拽
			var workAreaMapVM = GetOrCreateViewModel(moduleId, "WorkAreaMap", () => new WorkAreaMapViewModel());

			// 左侧：本地文件目录面板 - 使用LocalFilesViewModel实现真实文件系统访问
			var localFilesVM = GetOrCreateViewModel(moduleId, "LocalFiles", () => new LocalFilesViewModel());

			// 建立本地文件目录和预览区域的事件连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_LocalFiles_FilePreview_FileSelected"))
			{
				localFilesVM.FileSelected += async (fileNode) =>
				{
					await previewVM.PreviewLocalFileAsync(fileNode);
				};
			}
		
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
			var projectFilesVM = GetOrCreateViewModel(moduleId, "ProjectFiles", () => new ProjectFilesViewModel());

			// 建立工程目录和预览区域的事件连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_ProjectFiles_FilePreview_FileSelected"))
			{
				projectFilesVM.FileSelected += async (fileNode) =>
				{
					await previewVM.PreviewFileAsync(fileNode);
				};
			}

			// 建立工程目录和工区平面图的图层控制事件连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_ProjectFiles_WorkAreaMap_LayerVisibilityChanged"))
			{
				projectFilesVM.LayerVisibilityChanged += (layerPath, isVisible) =>
				{
					workAreaMapVM.SetLayerVisibility(layerPath, isVisible);
				};
			}

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
		/// 右侧: 层位信息属性窗口（默认隐藏，标注模式时显示）
		/// </summary>
		private ProportionalDock CreateSeismicLayout()
		{
			const string moduleId = "Seismic";

			// 左侧：地震工程结构目录
			var seismicProjectTreeVM = GetOrCreateViewModel(moduleId, "SeismicProjectTree", () => new SeismicProjectTreeViewModel());

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
			var seismicBodyVM = GetOrCreateViewModel(moduleId, "SeismicBody", () => new SeismicBodyViewModel());
			var seismicInterpretationVM = GetOrCreateViewModel(moduleId, "SeismicInterpretation", () => new SeismicInterpretationViewModel());

			// 右侧：属性窗口（使用通用PropertyPanelViewModel）- 默认不显示
			var propertyPanelVM = GetOrCreateViewModel(moduleId, "SeismicProperty", () =>
			{
				var vm = new PropertyPanelViewModel();
				return vm;
			});

			// 建立地震相智能推理与属性面板的连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_SeismicInterpretation_PropertyPanel_SeismicInferenceCompleted"))
			{
				seismicInterpretationVM.SeismicInferenceCompleted += (sectionName, results) =>
				{
					propertyPanelVM.SetSeismicInferenceResults(sectionName, results);
				};
			}

			// 建立地震标注与属性面板的连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_SeismicInterpretation_PropertyPanel_AnnotationsChanged"))
			{
				seismicInterpretationVM.SeismicAnnotationsChanged += (sectionName, annotations) =>
				{
					propertyPanelVM.SetSeismicAnnotations(sectionName, annotations);
					ShowSeismicPropertyPane();
				};
			}

			// 建立属性面板删除标注与地震解释视图的连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_PropertyPanel_SeismicInterpretation_AnnotationDeleted"))
			{
				propertyPanelVM.SeismicAnnotationDeleted += (annotation) =>
				{
					seismicInterpretationVM.DeleteAnnotation(annotation);
				};
			}

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

			// 右侧属性窗口 - 默认隐藏
			var rightDock = new ToolDock
			{
				Id = "SeismicPropertyPane",
				Title = "属性窗口",
				Proportion = 0.18,
				Alignment = Alignment.Right,
				ActiveDockable = propertyPanelVM,
				VisibleDockables = CreateList<IDockable>(propertyPanelVM),
				GripMode = GripMode.Visible,
				CanFloat = true,
				CanPin = true,
				CanClose = true,
				IsCollapsable = true
			};

			// 保存属性窗口引用，用于后续显示/隐藏
			_seismicPropertyDock = rightDock;
			_seismicPropertyPanelVM = propertyPanelVM;

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

			// 水平布局：左侧工程目录 + 中间文档区（默认不显示属性窗口）
			var layout = new ProportionalDock
			{
				Id = "SeismicMainLayout",
				Orientation = Orientation.Horizontal,
				VisibleDockables = CreateList<IDockable>(
					leftDock,
					splitter1,
					middleDock
				)
			};

			// 保存布局引用，用于后续添加属性窗口
			_seismicMainLayout = layout;
			_seismicPropertySplitter = splitter2;
			_seismicPropertyVisible = false;

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
			const string moduleId = "SingleWell";
			
			// 左侧：工程结构目录
			var wellProjectTreeVM = GetOrCreateViewModel(moduleId, "WellProjectTree", () => new WellProjectTreeViewModel());
			
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
			var wellColumnVM = GetOrCreateViewModel(moduleId, "WellColumn", () => new WellColumnViewModel());
			var wellCorrelationVM = GetOrCreateViewModel(moduleId, "WellCorrelation", () => new WellCorrelationViewModel());

			// 右侧：属性窗口
			var propertyPanelVM = GetOrCreateViewModel(moduleId, "PropertyPanel", () => new PropertyPanelViewModel());
			_propertyPanelVM = propertyPanelVM; // 保存引用

			// 建立事件连接：选择井时加载对应柱状图（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_WellProjectTree_WellColumn_WellSelected"))
			{
				wellProjectTreeVM.WellSelected += (wellName) =>
				{
					wellColumnVM.LoadWellData(wellName);
				};
			}

			// 建立智能推理与属性面板的连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_WellColumn_PropertyPanel_InferenceCompleted"))
			{
				wellColumnVM.InferenceCompleted += (wellName, results) =>
				{
					propertyPanelVM.SetInferenceResults(wellName, results);
				};
			}

			// 建立标注与属性面板的连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_WellColumn_PropertyPanel_AnnotationsChanged"))
			{
				wellColumnVM.AnnotationsChanged += (wellName, annotations) =>
				{
					propertyPanelVM.SetAnnotations(wellName, annotations);
				};
			}

			// 建立标注选中与属性面板的连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_WellColumn_PropertyPanel_AnnotationSelected"))
			{
				wellColumnVM.AnnotationSelected += (annotation) =>
				{
					propertyPanelVM.SetSelectedAnnotation(annotation);
				};
			}

			// 建立属性面板删除标注与柱状图的连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_PropertyPanel_WellColumn_AnnotationDeleted"))
			{
				propertyPanelVM.AnnotationDeleted += (annotation) =>
				{
					wellColumnVM.DeleteAnnotation(annotation);
				};
			}

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
			const string moduleId = "Mapping";
			
			// 左侧：图层管理器（类ArcGIS风格）
			var mappingLayerVM = GetOrCreateViewModel(moduleId, "MappingLayer", () => new MappingLayerViewModel());
			
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
			var sandBodyThicknessVM = GetOrCreateViewModel(moduleId, "SandBodyThickness", () => new SandBodyThicknessViewModel());
			var sandRatioVM = GetOrCreateViewModel(moduleId, "SandRatio", () => new SandRatioViewModel());
			var carbonateContentVM = GetOrCreateViewModel(moduleId, "CarbonateContent", () => new CarbonateContentViewModel());
			var lithofaciesVM = GetOrCreateViewModel(moduleId, "LithofaciesPaleogeography", () => new LithofaciesPaleogeographyViewModel());

			// 右侧：GIS工具栏和属性窗口
			var mappingToolsVM = GetOrCreateViewModel(moduleId, "MappingTools", () => new MappingToolsViewModel());

			// 建立图层选择事件连接（仅首次订阅）
			if (TrySubscribeEvent($"{moduleId}_MappingLayer_MappingTools_LayerSelected"))
			{
				mappingLayerVM.LayerSelected += (layer) =>
				{
					mappingToolsVM.SetSelectedLayer(layer);
				};
			}

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
		/// 地层对比布局
		/// 左侧: 数据资源视图 (20%)
		/// 右侧: 联井层序剖面图显示 (80%)
		/// </summary>
		private ProportionalDock CreateStratigraphyLayout()
		{
			const string moduleId = "Stratigraphy";
			
			// 创建地层对比ViewModel（用于右侧图片显示）
			var stratigraphyVM = GetOrCreateViewModel(moduleId, "Stratigraphy", () => new StratigraphyViewModel());
		
			// 右侧：联井层序剖面显示区（DocumentDock）
			var rightDock = new DocumentDock
			{
				Id = "StratigraphySectionPane",
				Title = "联井层序剖面",
				Proportion = double.NaN,
				IsCollapsable = false,
				ActiveDockable = stratigraphyVM,
				VisibleDockables = CreateList<IDockable>(stratigraphyVM),
				CanFloat = true,
				CanPin = true,
				CanClose = true,
				CanCreateDocument = true
			};
		
			var splitter = new ProportionalDockSplitter
			{
				Id = "StratigraphySplitter",
				Title = "Splitter"
			};
		
			// 水平布局：右侧剖面显示（不需要左侧Dock，直接在View中实现数据资源树）
			var layout = new ProportionalDock
			{
				Id = "StratigraphyMainLayout",
				Orientation = Orientation.Horizontal,
				VisibleDockables = CreateList<IDockable>(
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
						if (_rootDock != null)
						{
							SetFocusedDockable(_rootDock, doc);
						}
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

		/// <summary>
		/// 显示井智能推理对话框（从单井列表选择）
		/// </summary>
		public void ShowWellInferenceDialog()
		{
			if (_rootDock == null) return;

			// 激活单井柱状图标签页
			ActivateDocumentInCurrentLayout("WellColumn");

			// 查找WellColumnViewModel并显示井选择对话框
			var wellColumnVM = FindDocumentRecursive(_rootDock, "WellColumn") as WellColumnViewModel;
			if (wellColumnVM != null)
			{
				wellColumnVM.ShowInferenceDialogCommand.Execute(null);
			}
		}

		/// <summary>
		/// 显示基于工区平面图的智能推理对话框
		/// </summary>
		public void ShowMapBasedInferenceDialog()
		{
			if (_rootDock == null) return;

			// 先激活工区平面图
			ActivateDocumentInCurrentLayout("WorkAreaMap");

			// TODO: 实现工区平面图井选择对话框
			// 当前暂时切换到单井列表选择
			ShowWellInferenceDialog();
		}

		/// <summary>
		/// 显示地震道号范围推理对话框
		/// </summary>
		public void ShowSeismicTraceInferenceDialog()
		{
			if (_rootDock == null) return;

			// 激活地震解释剖面标签页
			ActivateDocumentInCurrentLayout("SeismicInterpretation");

			// 查找SeismicInterpretationViewModel并显示道号范围对话框
			var seismicInterpVM = FindDocumentRecursive(_rootDock, "SeismicInterpretation") as SeismicInterpretationViewModel;
			if (seismicInterpVM != null)
			{
				seismicInterpVM.ShowInferenceDialogCommand.Execute(null);
			}
		}

		/// <summary>
		/// 显示模型对比对话框
		/// </summary>
		public void ShowModelComparisonDialog()
		{
			if (_rootDock == null) return;

			// 激活地震解释剖面标签页
			ActivateDocumentInCurrentLayout("SeismicInterpretation");

			// TODO: 实现模型对比对话框
			// 当前暂时显示道号范围对话框
			ShowSeismicTraceInferenceDialog();
		}

		/// <summary>
		/// 启用矩形标注模式
		/// </summary>
		public void EnableRectangleAnnotationMode()
		{
			if (_rootDock == null) return;

			// 激活单井柱状图标签页
			ActivateDocumentInCurrentLayout("WellColumn");

			// 查找WellColumnViewModel并启用标注模式
			var wellColumnVM = FindDocumentRecursive(_rootDock, "WellColumn") as WellColumnViewModel;
			if (wellColumnVM != null)
			{
				wellColumnVM.EnableAnnotationModeCommand.Execute(null);
			}
		}
	}
}