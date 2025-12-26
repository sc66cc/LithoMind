using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Views
{
	/// <summary>
	/// 制图文档视图代码后置
	/// </summary>
	public partial class MappingDocumentView : UserControl
	{
		private bool _isPanning = false;
		private Point _lastPanPosition;
		
		// 图例拖拽相关
		private bool _isLegendDragging = false;
		private Point _legendDragStart;
		private Thickness _legendOriginalMargin;

		public MappingDocumentView()
		{
			InitializeComponent();
			
			// 注册鼠标事件用于缩放和拖拽
			this.PointerWheelChanged += OnPointerWheelChanged;
			this.PointerPressed += OnPointerPressed;
			this.PointerMoved += OnPointerMoved;
			this.PointerReleased += OnPointerReleased;
			
			// 注册图例拖拽事件
			this.Loaded += OnLoaded;
		}
		
		private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			// 获取图例标题栏并注册拖拽事件
			var legendTitleBar = this.FindControl<Border>("LegendTitleBar");
			var legendPanel = this.FindControl<Border>("LegendPanel");
			
			if (legendTitleBar != null && legendPanel != null)
			{
				legendTitleBar.PointerPressed += OnLegendPointerPressed;
				legendTitleBar.PointerMoved += OnLegendPointerMoved;
				legendTitleBar.PointerReleased += OnLegendPointerReleased;
			}
		}
		
		private void OnLegendPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			var props = e.GetCurrentPoint(this).Properties;
			if (props.IsLeftButtonPressed)
			{
				var legendPanel = this.FindControl<Border>("LegendPanel");
				if (legendPanel != null)
				{
					_isLegendDragging = true;
					_legendDragStart = e.GetPosition(this);
					_legendOriginalMargin = legendPanel.Margin;
					e.Pointer.Capture((IInputElement)sender!);
					e.Handled = true;
				}
			}
		}
		
		private void OnLegendPointerMoved(object? sender, PointerEventArgs e)
		{
			if (_isLegendDragging)
			{
				var legendPanel = this.FindControl<Border>("LegendPanel");
				if (legendPanel != null)
				{
					var currentPos = e.GetPosition(this);
					var deltaX = currentPos.X - _legendDragStart.X;
					var deltaY = currentPos.Y - _legendDragStart.Y;
					
					// 图例在右下角，所以移动方向相反
					var newRight = _legendOriginalMargin.Right - deltaX;
					var newBottom = _legendOriginalMargin.Bottom - deltaY;
					
					// 限制边界
					newRight = System.Math.Max(10, System.Math.Min(newRight, this.Bounds.Width - legendPanel.Bounds.Width - 10));
					newBottom = System.Math.Max(10, System.Math.Min(newBottom, this.Bounds.Height - legendPanel.Bounds.Height - 10));
					
					legendPanel.Margin = new Thickness(0, 0, newRight, newBottom);
					e.Handled = true;
				}
			}
		}
		
		private void OnLegendPointerReleased(object? sender, PointerReleasedEventArgs e)
		{
			if (_isLegendDragging)
			{
				_isLegendDragging = false;
				e.Pointer.Capture(null);
				e.Handled = true;
			}
		}

		/// <summary>
		/// 处理鼠标滚轮缩放
		/// </summary>
		private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
		{
			if (DataContext is MappingDocumentViewModelBase vm)
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
			if (_isPanning && DataContext is MappingDocumentViewModelBase vm)
			{
				var currentPosition = e.GetPosition(this);
				var deltaX = currentPosition.X - _lastPanPosition.X;
				var deltaY = currentPosition.Y - _lastPanPosition.Y;
				
				vm.ApplyPan(deltaX, deltaY);
				
				_lastPanPosition = currentPosition;
				e.Handled = true;
			}
			
			// 更新坐标信息
			if (DataContext is MappingDocumentViewModelBase vmCoord)
			{
				var pos = e.GetPosition(this);
				vmCoord.UpdateCoordinate(pos.X, pos.Y);
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
