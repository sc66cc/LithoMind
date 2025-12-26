using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Views
{
	/// <summary>
	/// 地震体数据视图代码后置
	/// </summary>
	public partial class SeismicBodyView : UserControl
	{
		private bool _isPanning = false;
		private Point _lastPanPosition;

		public SeismicBodyView()
		{
			InitializeComponent();
			
			// 注册鼠标事件用于缩放和拖拽
			this.PointerWheelChanged += OnPointerWheelChanged;
			this.PointerPressed += OnPointerPressed;
			this.PointerMoved += OnPointerMoved;
			this.PointerReleased += OnPointerReleased;
		}

		/// <summary>
		/// 处理鼠标滚轮缩放
		/// </summary>
		private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
		{
			if (DataContext is SeismicBodyViewModel vm)
			{
				vm.SetZoom(e.Delta.Y);
				e.Handled = true;
			}
		}

		/// <summary>
		/// 处理鼠标按下 - 开始拖拽
		/// </summary>
		private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			var props = e.GetCurrentPoint(this).Properties;
			if (props.IsLeftButtonPressed)
			{
				_isPanning = true;
				_lastPanPosition = e.GetPosition(this);
				e.Pointer.Capture(this);
				e.Handled = true;
			}
		}

		/// <summary>
		/// 处理鼠标移动 - 拖拽平移
		/// </summary>
		private void OnPointerMoved(object? sender, PointerEventArgs e)
		{
			if (_isPanning && DataContext is SeismicBodyViewModel vm)
			{
				var currentPosition = e.GetPosition(this);
				var deltaX = currentPosition.X - _lastPanPosition.X;
				var deltaY = currentPosition.Y - _lastPanPosition.Y;
				
				vm.ApplyPan(deltaX, deltaY);
				
				_lastPanPosition = currentPosition;
				e.Handled = true;
			}
		}

		/// <summary>
		/// 处理鼠标释放 - 停止拖拽
		/// </summary>
		private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
		{
			if (_isPanning)
			{
				_isPanning = false;
				e.Pointer.Capture(null);
				e.Handled = true;
			}
		}
	}
}
