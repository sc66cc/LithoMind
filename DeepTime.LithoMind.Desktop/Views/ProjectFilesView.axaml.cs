using Avalonia.Controls;
using Avalonia.Interactivity;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Views
{
    public partial class ProjectFilesView : UserControl
    {
        public ProjectFilesView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 处理TreeView的选择变更事件
        /// </summary>
        private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ProjectFilesViewModel viewModel && e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is ProjectNode node)
                {
                    viewModel.SelectNodeCommand.Execute(node);
                }
            }
        }
    }
}
