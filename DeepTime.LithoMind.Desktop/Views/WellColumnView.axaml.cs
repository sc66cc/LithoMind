using Avalonia.Controls;
using Avalonia.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Views
{
	/// <summary>
	/// 单井综合柱状图视图代码后置
	/// </summary>
	public partial class WellColumnView : UserControl
	{
		public WellColumnView()
		{
			InitializeComponent();
			
			// 注册鼠标滚轮事件用于缩放
			this.PointerWheelChanged += OnPointerWheelChanged;
		}

		/// <summary>
		/// 处理鼠标滚轮缩放
		/// </summary>
		private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
		{
			if (DataContext is WellColumnViewModel vm)
			{
				// Ctrl+滚轮进行缩放
				if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
				{
					if (e.Delta.Y > 0)
					{
						vm.ZoomIn();
					}
					else
					{
						vm.ZoomOut();
					}
					e.Handled = true;
				}
			}
		}
	}
}
