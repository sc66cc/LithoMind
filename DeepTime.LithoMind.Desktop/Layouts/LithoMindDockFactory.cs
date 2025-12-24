using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Layouts
{
	public class LithoMindDockFactory : Factory
	{
		private readonly object _context;

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

				case "Fusion":
					mainLayout = CreateSimpleLayout(new FusionViewModel());
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
				Title = moduleId,
				ActiveDockable = mainLayout,
				DefaultDockable = mainLayout,
				VisibleDockables = CreateList<IDockable>(mainLayout)
			};

			return root;
		}

		/// <summary>
		/// 数据管理模块的专属布局：本地目录(1/8) + 工程目录(1/8) + 数据预览(3/4)
		/// </summary>
		private ProportionalDock CreateDataManagerLayout()
		{
			// 左侧：本地文件目录面板
			var localFileDoc = new DataManagerViewModel { Id = "LocalFiles", Title = "本地文件" };

			var leftDock = new DocumentDock
			{
				Id = "LocalFileExplorer",
				Title = "本地文件目录",
				Proportion = 0.125,
				CanCreateDocument = false,
				ActiveDockable = localFileDoc,
				VisibleDockables = CreateList<IDockable>(localFileDoc)
			};

			// 中间：工程文件目录面板
			var projectFileDoc = new DataManagerViewModel { Id = "ProjectFiles", Title = "工程文件" };

			var middleDock = new DocumentDock
			{
				Id = "ProjectFileExplorer",
				Title = "工程文件目录",
				Proportion = 0.125,
				CanCreateDocument = false,
				ActiveDockable = projectFileDoc,
				VisibleDockables = CreateList<IDockable>(projectFileDoc)
			};

			// 右侧：数据预览区域
			var previewDoc = new DataManagerViewModel { Id = "Preview", Title = "数据预览" };

			var rightDock = new DocumentDock
			{
				Id = "DataPreview",
				Title = "数据预览",
				Proportion = 0.75,
				CanCreateDocument = true,
				ActiveDockable = previewDoc,
				VisibleDockables = CreateList<IDockable>(previewDoc)
			};

			// 水平布局：从左到右排列三个面板，使用ProportionalDock支持调整大小
			var layout = new ProportionalDock
			{
				Id = "DataManagerMainLayout",
				Orientation = Orientation.Horizontal,
				VisibleDockables = CreateList<IDockable>(leftDock, middleDock, rightDock)
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
				Orientation = Orientation.Horizontal,
				VisibleDockables = CreateList<IDockable>(documentDock)
			};

			return layout;
		}
	}
}