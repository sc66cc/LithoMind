using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Views
{
    public partial class LocalFilesView : UserControl
    {
        private TreeView? _treeView;
        private LocalFilesViewModel? _viewModel;

        public LocalFilesView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        /// <summary>
        /// 数据上下文变化时订阅事件
        /// </summary>
        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            // 取消旧的订阅
            if (_viewModel != null)
            {
                _viewModel.ScrollToNodeRequested -= OnScrollToNodeRequested;
            }

            // 订阅新的ViewModel事件
            if (DataContext is LocalFilesViewModel viewModel)
            {
                _viewModel = viewModel;
                _viewModel.ScrollToNodeRequested += OnScrollToNodeRequested;
            }
        }

        /// <summary>
        /// 响应滚动到节点请求
        /// </summary>
        private void OnScrollToNodeRequested(FileSystemNode node)
        {
            // 使用Dispatcher确保在UI线程上执行，并等待布局更新完成
            Dispatcher.UIThread.Post(() =>
            {
                ScrollTreeViewToNode(node);
            }, DispatcherPriority.Background);
        }

        /// <summary>
        /// 滚动TreeView到指定节点
        /// </summary>
        private void ScrollTreeViewToNode(FileSystemNode targetNode)
        {
            try
            {
                // 获取TreeView控件
                _treeView ??= this.FindControl<TreeView>("FileTreeView");
                
                if (_treeView == null)
                {
                    // 如果没有指定名称，尝试遍历查找
                    _treeView = this.GetVisualDescendants().OfType<TreeView>().FirstOrDefault();
                }

                if (_treeView == null)
                    return;

                // 查找对应的TreeViewItem
                var treeViewItem = FindTreeViewItem(_treeView, targetNode);
                if (treeViewItem != null)
                {
                    // 滚动到该项使其可见
                    treeViewItem.BringIntoView();
                    
                    // 确保该项被选中
                    treeViewItem.IsSelected = true;
                    treeViewItem.Focus();
                }
            }
            catch (Exception)
            {
                // 忽略滚动错误
            }
        }

        /// <summary>
        /// 在TreeView中查找对应的TreeViewItem
        /// </summary>
        private TreeViewItem? FindTreeViewItem(ItemsControl container, FileSystemNode targetNode)
        {
            if (container == null || targetNode == null)
                return null;

            // 遍历容器中的所有项
            foreach (var item in container.Items)
            {
                if (item == null)
                    continue;

                if (item == targetNode)
                {
                    // 找到匹配的数据项，获取对应的TreeViewItem
                    return container.ContainerFromItem(item) as TreeViewItem;
                }

                // 获取当前项的容器
                var childContainer = container.ContainerFromItem(item) as TreeViewItem;
                if (childContainer != null)
                {
                    // 递归查找子节点
                    var result = FindTreeViewItem(childContainer, targetNode);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 处理TreeView的选择变更事件
        /// </summary>
        private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is LocalFilesViewModel viewModel && e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is FileSystemNode node)
                {
                    viewModel.SelectNodeCommand.Execute(node);
                }
            }
        }

        /// <summary>
        /// 处理TreeViewItem的展开事件 - 延迟加载子节点
        /// </summary>
        private void OnTreeViewItemExpanding(object? sender, RoutedEventArgs e)
        {
            if (e.Source is TreeViewItem treeViewItem && 
                treeViewItem.DataContext is FileSystemNode node)
            {
                if (DataContext is LocalFilesViewModel viewModel)
                {
                    viewModel.ExpandNodeCommand.Execute(node);
                }
            }
        }

        /// <summary>
        /// 控件卸载时取消事件订阅
        /// </summary>
        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            
            if (_viewModel != null)
            {
                _viewModel.ScrollToNodeRequested -= OnScrollToNodeRequested;
                _viewModel = null;
            }
        }
    }
}
