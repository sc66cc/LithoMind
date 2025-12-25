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
					mainLayout = CreateSimpleLayout(new SingleWellViewModel());
					break;

				case "Seismic":
					mainLayout = CreateSimpleLayout(new SeismicViewModel());
					break;

				case "Mapping":
					mainLayout = CreateSimpleLayout(new MappingViewModel());
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

		/// <summary>
		/// 数据管理模块的专属布局：本地目录(1/8) + 工程目录(1/8) + 数据预览(3/4)
		/// VSCode风格：支持标签页分组、拖拽停靠、悬浮窗口
		/// </summary>
		private ProportionalDock CreateDataManagerLayout()
		{
			// 左侧：本地文件目录面板 - 使用DocumentDock支持标签页
			var localFileDoc1 = new DataManagerViewModel { Id = "LocalFiles", Title = "本地文件" };
			var localFileDoc2 = new DataManagerViewModel { Id = "LocalRecent", Title = "最近使用" };
		
			var leftDock = new DocumentDock
			{
				Id = "LocalFileExplorer",
				Title = "本地资源",
				Proportion = 0.125,
				CanCreateDocument = true,
				ActiveDockable = localFileDoc1,
				VisibleDockables = CreateList<IDockable>(localFileDoc1, localFileDoc2),
				// 启用拖拽和浮动功能
				CanFloat = true,
				CanPin = true,
				CanClose = true,
				IsCollapsable = true
			};
		
			// 中间：工程文件目录面板 - 使用DocumentDock支持标签页
			var projectFileDoc1 = new DataManagerViewModel { Id = "ProjectFiles", Title = "工程文件" };
			var projectFileDoc2 = new DataManagerViewModel { Id = "ProjectProps", Title = "工程属性" };
		
			var middleDock = new DocumentDock
			{
				Id = "ProjectFileExplorer",
				Title = "工程管理",
				Proportion = 0.125,
				CanCreateDocument = true,
				ActiveDockable = projectFileDoc1,
				VisibleDockables = CreateList<IDockable>(projectFileDoc1, projectFileDoc2),
				// 启用拖拽和浮动功能
				CanFloat = true,
				CanPin = true,
				CanClose = true,
				IsCollapsable = true
			};
		
			// 右侧：数据预览区域 - 支持多标签页
			var previewDoc1 = new DataManagerViewModel { Id = "Preview", Title = "数据预览" };
			var previewDoc2 = new DataManagerViewModel { Id = "PreviewDetails", Title = "详细信息" };
			var previewDoc3 = new DataManagerViewModel { Id = "PreviewMeta", Title = "元数据" };
		
			var rightDock = new DocumentDock
			{
				Id = "DataPreview",
				Title = "预览区域",
				Proportion = 0.75,
				CanCreateDocument = true,
				ActiveDockable = previewDoc1,
				VisibleDockables = CreateList<IDockable>(previewDoc1, previewDoc2, previewDoc3),
				// 启用拖拽和浮动功能
				CanFloat = true,
				CanPin = true,
				CanClose = true,
				IsCollapsable = true
			};
		
			// 创建分隔条（关键！这样才能调整大小）
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
	}
}