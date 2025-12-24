using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;
using DeepTime.LithoMind.Desktop.ViewModels;

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
			// 1. 定义中间的文档区
			var documentDock = new DocumentDock
			{
				Id = "DocumentsPane",
				Title = "Documents",
				Proportion = double.NaN,
				IsCollapsable = false
			};

			// 2. 根据 moduleId 决定里面放什么
			switch (moduleId)
			{
				case "DataManager":
					documentDock.VisibleDockables = CreateList<IDockable>(
						new DataManagerViewModel() 
					);
					break;

				case "SingleWell":
					documentDock.VisibleDockables = CreateList<IDockable>(
						new SingleWellViewModel() 
					);
					break;

				case "Seismic":
					documentDock.VisibleDockables = CreateList<IDockable>(
						new SeismicViewModel() 
					);
					break;
				case "Fusion":
					documentDock.VisibleDockables = CreateList<IDockable>(
						new FusionViewModel()
						);
					break;
				case "Mapping":
					documentDock.VisibleDockables = CreateList<IDockable>(
						new MappingViewModel()
						);
					break;
				case "Stratigraphy":
					documentDock.VisibleDockables = CreateList<IDockable>(
						new StratigraphyViewModel()
						);
					break;

				default:
					documentDock.VisibleDockables = CreateList<IDockable>(
						new DataManagerViewModel()
						);
					break;
			}

			// 3. 构建根布局
			var mainLayout = new ProportionalDock
			{
				Orientation = Orientation.Horizontal,
				VisibleDockables = CreateList<IDockable>(documentDock)
			};

			var root = new RootDock
			{
				Title = moduleId,
				ActiveDockable = mainLayout,
				DefaultDockable = mainLayout,
				VisibleDockables = CreateList<IDockable>(mainLayout)
			};

			return root;
		}
	}
}