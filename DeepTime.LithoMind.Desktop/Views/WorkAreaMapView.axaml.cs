using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Views
{
	/// <summary>
	/// 工区平面图视图代码后置
	/// 处理鼠标滚轮缩放和拖拽平移交互
	/// </summary>
	public partial class WorkAreaMapView : UserControl
	{
		private bool _isDragging;
		private Point _lastMousePosition;
		private Canvas? _mapCanvas;

		public WorkAreaMapView()
		{
			InitializeComponent();
			
			// 绑定画布事件
			Loaded += OnLoaded;
		}

		private void OnLoaded(object? sender, RoutedEventArgs e)
		{
			_mapCanvas = this.FindControl<Canvas>("MapCanvas");
			
			if (_mapCanvas != null)
			{
				// 绑定鼠标事件
				_mapCanvas.PointerWheelChanged += OnPointerWheelChanged;
				_mapCanvas.PointerPressed += OnPointerPressed;
				_mapCanvas.PointerMoved += OnPointerMoved;
				_mapCanvas.PointerReleased += OnPointerReleased;
				_mapCanvas.PointerCaptureLost += OnPointerCaptureLost;
			}
		}

		/// <summary>
		/// 鼠标滚轮事件 - 处理缩放
		/// </summary>
		private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
		{
			if (DataContext is WorkAreaMapViewModel viewModel && _mapCanvas != null)
			{
				// 获取鼠标相对于画布的位置
				var mousePosition = e.GetPosition(_mapCanvas);
				
				// 根据滚轮方向调整缩放
				var delta = e.Delta.Y;
				viewModel.SetZoomLevel(delta, mousePosition);
				
				e.Handled = true;
			}
		}

		/// <summary>
		/// 鼠标按下事件 - 开始拖拽
		/// </summary>
		private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			if (_mapCanvas != null && e.GetCurrentPoint(_mapCanvas).Properties.IsLeftButtonPressed)
			{
				_isDragging = true;
				_lastMousePosition = e.GetPosition(_mapCanvas);
				
				// 捕获鼠标
				e.Pointer.Capture(_mapCanvas);
				
				// 设置拖拽光标
				_mapCanvas.Cursor = new Cursor(StandardCursorType.Hand);
				
				e.Handled = true;
			}
		}

		/// <summary>
		/// 鼠标移动事件 - 处理拖拽
		/// </summary>
		private void OnPointerMoved(object? sender, PointerEventArgs e)
		{
			if (_isDragging && DataContext is WorkAreaMapViewModel viewModel && _mapCanvas != null)
			{
				var currentPosition = e.GetPosition(_mapCanvas);
				
				// 计算偏移量
				var deltaX = currentPosition.X - _lastMousePosition.X;
				var deltaY = currentPosition.Y - _lastMousePosition.Y;
				
				// 应用平移
				viewModel.ApplyPanOffset(deltaX, deltaY);
				
				_lastMousePosition = currentPosition;
				
				e.Handled = true;
			}
		}

		/// <summary>
		/// 鼠标释放事件 - 结束拖拽
		/// </summary>
		private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
		{
			if (_isDragging)
			{
				EndDragging(e.Pointer);
				e.Handled = true;
			}
		}

		/// <summary>
		/// 鼠标捕获丢失事件
		/// </summary>
		private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
		{
			if (_isDragging)
			{
				EndDragging(null);
			}
		}

		/// <summary>
		/// 结束拖拽操作
		/// </summary>
		private void EndDragging(IPointer? pointer)
		{
			_isDragging = false;
			
			if (_mapCanvas != null)
			{
				_mapCanvas.Cursor = Cursor.Default;
			}
			
			// 释放鼠标捕获
			pointer?.Capture(null);
		}
	}
}
