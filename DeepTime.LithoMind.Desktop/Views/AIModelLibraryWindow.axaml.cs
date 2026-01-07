using Avalonia.Controls;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Views
{
	public partial class AIModelLibraryWindow : Window
	{
		public AIModelLibraryWindow()
		{
			InitializeComponent();

			var viewModel = new AIModelLibraryViewModel();
			DataContext = viewModel;

			// 订阅关闭请求事件
			viewModel.CloseRequested += () => Close();
		}
	}
}
