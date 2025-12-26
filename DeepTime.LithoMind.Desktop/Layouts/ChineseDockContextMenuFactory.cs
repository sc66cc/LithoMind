using System;
using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace DeepTime.LithoMind.Desktop.Layouts
{
	/// <summary>
	/// 中文化的Dock右键菜单工厂
	/// 重写Dock.Avalonia默认的英文菜单文本为中文
	/// </summary>
	public static class ChineseDockContextMenuFactory
	{
		/// <summary>
		/// 创建中文化的标签页右键菜单
		/// </summary>
		public static ContextMenu CreateTabContextMenu(IDockable dockable, Action<IDockable>? onClose = null, Action<IDockable>? onCloseOthers = null, Action<IDockable>? onCloseAll = null, Action<IDockable>? onFloat = null, Action<IDockable>? onDock = null, Action<IDockable>? onPin = null)
		{
			var menu = new ContextMenu();
			
			// 关闭
			var closeItem = new MenuItem { Header = "关闭" };
			if (onClose != null)
			{
				closeItem.Click += (s, e) => onClose(dockable);
			}
			menu.Items.Add(closeItem);
			
			// 关闭其他
			var closeOthersItem = new MenuItem { Header = "关闭其他" };
			if (onCloseOthers != null)
			{
				closeOthersItem.Click += (s, e) => onCloseOthers(dockable);
			}
			menu.Items.Add(closeOthersItem);
			
			// 关闭所有
			var closeAllItem = new MenuItem { Header = "关闭所有" };
			if (onCloseAll != null)
			{
				closeAllItem.Click += (s, e) => onCloseAll(dockable);
			}
			menu.Items.Add(closeAllItem);
			
			menu.Items.Add(new Separator());
			
			// 浮动
			var floatItem = new MenuItem { Header = "浮动" };
			if (onFloat != null)
			{
				floatItem.Click += (s, e) => onFloat(dockable);
			}
			menu.Items.Add(floatItem);
			
			// 停靠
			var dockItem = new MenuItem { Header = "停靠" };
			if (onDock != null)
			{
				dockItem.Click += (s, e) => onDock(dockable);
			}
			menu.Items.Add(dockItem);
			
			// 固定
			var pinItem = new MenuItem { Header = "固定/取消固定" };
			if (onPin != null)
			{
				pinItem.Click += (s, e) => onPin(dockable);
			}
			menu.Items.Add(pinItem);
			
			return menu;
		}

		/// <summary>
		/// 创建中文化的工具面板右键菜单
		/// </summary>
		public static ContextMenu CreateToolContextMenu(IDockable dockable, Action<IDockable>? onFloat = null, Action<IDockable>? onDock = null, Action<IDockable>? onAutoHide = null, Action<IDockable>? onClose = null)
		{
			var menu = new ContextMenu();
			
			// 浮动
			var floatItem = new MenuItem { Header = "浮动" };
			if (onFloat != null)
			{
				floatItem.Click += (s, e) => onFloat(dockable);
			}
			menu.Items.Add(floatItem);
			
			// 停靠
			var dockItem = new MenuItem { Header = "停靠" };
			if (onDock != null)
			{
				dockItem.Click += (s, e) => onDock(dockable);
			}
			menu.Items.Add(dockItem);
			
			// 自动隐藏
			var autoHideItem = new MenuItem { Header = "自动隐藏" };
			if (onAutoHide != null)
			{
				autoHideItem.Click += (s, e) => onAutoHide(dockable);
			}
			menu.Items.Add(autoHideItem);
			
			menu.Items.Add(new Separator());
			
			// 关闭
			var closeItem = new MenuItem { Header = "关闭" };
			if (onClose != null)
			{
				closeItem.Click += (s, e) => onClose(dockable);
			}
			menu.Items.Add(closeItem);
			
			return menu;
		}
	}
}
