using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Views
{
	/// <summary>
	/// 全局搜索窗口
	/// </summary>
	public partial class GlobalSearchView : Window
	{
		public GlobalSearchView()
		{
			InitializeComponent();

			// 订阅键盘事件，支持 ESC 关闭窗口
			KeyDown += OnKeyDown;
		}

		/// <summary>
		/// 键盘按键处理
		/// </summary>
		private void OnKeyDown(object? sender, KeyEventArgs e)
		{
			// ESC 关闭窗口
			if (e.Key == Key.Escape)
			{
				Close();
			}
		}

		/// <summary>
		/// 搜索结果项被点击
		/// </summary>
		private void OnResultItemTapped(object? sender, TappedEventArgs e)
		{
			if (sender is Border border && border.DataContext is SearchResultItem item)
			{
				// 触发选择结果命令
				if (DataContext is GlobalSearchViewModel viewModel)
				{
					viewModel.SelectResultCommand.Execute(item);

					// 执行结果后关闭窗口
					Close();
				}
			}
		}
	}
}
