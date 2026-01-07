using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using DeepTime.LithoMind.Desktop.ViewModels.Pages;

namespace DeepTime.LithoMind.Desktop.Views
{
	/// <summary>
	/// 单井综合柱状图视图代码后置
	/// </summary>
	public partial class WellColumnView : UserControl
	{
		private Canvas? _annotationCanvas;
		private Image? _columnImage;
		private Rectangle? _currentDrawingRect;
		private Point _drawStartPoint;
		private bool _isDrawing;

		public WellColumnView()
		{
			InitializeComponent();

			// 注册鼠标滚轮事件用于缩放
			this.PointerWheelChanged += OnPointerWheelChanged;

			// 加载完成后初始化Canvas事件
			this.Loaded += OnLoaded;
		}

		private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			_annotationCanvas = this.FindControl<Canvas>("AnnotationCanvas");
			_columnImage = this.FindControl<Image>("ColumnImage");

			if (_annotationCanvas != null)
			{
				_annotationCanvas.PointerPressed += OnCanvasPointerPressed;
				_annotationCanvas.PointerMoved += OnCanvasPointerMoved;
				_annotationCanvas.PointerReleased += OnCanvasPointerReleased;
			}

			// 监听图片尺寸变化
			if (_columnImage != null)
			{
				_columnImage.PropertyChanged += OnColumnImagePropertyChanged;
			}

			// 监听ViewModel变化
			this.DataContextChanged += OnDataContextChanged;
			SetupViewModelBindings();
		}

		private void OnDataContextChanged(object? sender, EventArgs e)
		{
			SetupViewModelBindings();
		}

		private void SetupViewModelBindings()
		{
			if (DataContext is WellColumnViewModel vm)
			{
				// 监听标注集合变化
				vm.Annotations.CollectionChanged += OnAnnotationsCollectionChanged;

				// 监听当前绘制标注变化
				vm.PropertyChanged += (s, e) =>
				{
					if (e.PropertyName == nameof(WellColumnViewModel.CurrentDrawingAnnotation))
					{
						UpdateCurrentDrawingRect();
					}
					else if (e.PropertyName == nameof(WellColumnViewModel.IsAnnotationMode))
					{
						UpdateCanvasCursor();
					}
					else if (e.PropertyName == nameof(WellColumnViewModel.ColumnImage))
					{
						// 当图片更新时，同步Canvas尺寸
						SyncCanvasSizeWithImage();
					}
				};

				// 初始更新
				UpdateCanvasCursor();
				RefreshAnnotationRects();

				// 初始化时同步Canvas尺寸
				SyncCanvasSizeWithImage();
			}
		}

		/// <summary>
		/// 同步Canvas尺寸与图片尺寸
		/// </summary>
		private void SyncCanvasSizeWithImage()
		{
			if (DataContext is WellColumnViewModel vm && _columnImage != null && _annotationCanvas != null)
			{
				if (_columnImage.Source is Avalonia.Media.Imaging.Bitmap bitmap)
				{
					vm.ImageActualWidth = bitmap.Size.Width;
					vm.ImageActualHeight = bitmap.Size.Height;
					_annotationCanvas.Width = bitmap.Size.Width;
					_annotationCanvas.Height = bitmap.Size.Height;
				}
			}
		}

		private void OnColumnImagePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (DataContext is WellColumnViewModel vm && _columnImage != null)
			{
				// 当Source属性变化时，从源图片获取尺寸
				if (e.Property == Image.SourceProperty && _columnImage.Source is Avalonia.Media.Imaging.Bitmap bitmap)
				{
					vm.ImageActualWidth = bitmap.Size.Width;
					vm.ImageActualHeight = bitmap.Size.Height;

					// 同步Canvas尺寸
					if (_annotationCanvas != null)
					{
						_annotationCanvas.Width = bitmap.Size.Width;
						_annotationCanvas.Height = bitmap.Size.Height;
					}
				}
				// 当Bounds变化时也尝试更新（作为备用方案）
				else if (e.Property == BoundsProperty && _columnImage.Bounds.Width > 0 && _columnImage.Bounds.Height > 0)
				{
					if (vm.ImageActualWidth <= 0 || vm.ImageActualHeight <= 0)
					{
						vm.ImageActualWidth = _columnImage.Bounds.Width;
						vm.ImageActualHeight = _columnImage.Bounds.Height;

						if (_annotationCanvas != null)
						{
							_annotationCanvas.Width = _columnImage.Bounds.Width;
							_annotationCanvas.Height = _columnImage.Bounds.Height;
						}
					}
				}
			}
		}

		private void UpdateCanvasCursor()
		{
			if (_annotationCanvas == null || DataContext is not WellColumnViewModel vm) return;

			_annotationCanvas.Cursor = vm.IsAnnotationMode ? new Cursor(StandardCursorType.Cross) : Cursor.Default;
		}

		private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			if (DataContext is not WellColumnViewModel vm || !vm.IsAnnotationMode) return;

			var point = e.GetPosition(_annotationCanvas);

			// 检查是否点击了已有标注
			if (e.GetCurrentPoint(_annotationCanvas).Properties.IsLeftButtonPressed)
			{
				// 开始绘制新标注
				_isDrawing = true;
				_drawStartPoint = point;

				vm.StartDrawingAnnotation(point.X, point.Y);

				// 创建临时绘制矩形
				_currentDrawingRect = new Rectangle
				{
					Stroke = new SolidColorBrush(Color.Parse("#E74C3C")),
					StrokeThickness = 2,
					StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 2 },
					Fill = new SolidColorBrush(Color.Parse("#40E74C3C")),
					IsHitTestVisible = false
				};

				_annotationCanvas?.Children.Add(_currentDrawingRect);

				e.Pointer.Capture(_annotationCanvas);
				e.Handled = true;
			}
		}

		private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
		{
			if (!_isDrawing || _currentDrawingRect == null || DataContext is not WellColumnViewModel vm) return;

			var point = e.GetPosition(_annotationCanvas);

			// 计算矩形位置和尺寸
			double left = Math.Min(_drawStartPoint.X, point.X);
			double top = Math.Min(_drawStartPoint.Y, point.Y);
			double width = Math.Abs(point.X - _drawStartPoint.X);
			double height = Math.Abs(point.Y - _drawStartPoint.Y);

			Canvas.SetLeft(_currentDrawingRect, left);
			Canvas.SetTop(_currentDrawingRect, top);
			_currentDrawingRect.Width = width;
			_currentDrawingRect.Height = height;

			vm.UpdateDrawingAnnotation(point.X, point.Y);
		}

		private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
		{
			if (!_isDrawing || DataContext is not WellColumnViewModel vm) return;

			_isDrawing = false;
			e.Pointer.Capture(null);

			// 移除临时绘制矩形
			if (_currentDrawingRect != null && _annotationCanvas != null)
			{
				_annotationCanvas.Children.Remove(_currentDrawingRect);
				_currentDrawingRect = null;
			}

			// 完成绘制
			vm.FinishDrawingAnnotation();

			// 刷新显示所有标注
			RefreshAnnotationRects();
		}

		private void OnAnnotationsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			RefreshAnnotationRects();
		}

		private void UpdateCurrentDrawingRect()
		{
			if (DataContext is not WellColumnViewModel vm || _currentDrawingRect == null) return;

			var annotation = vm.CurrentDrawingAnnotation;
			if (annotation == null) return;

			Canvas.SetLeft(_currentDrawingRect, annotation.CanvasLeft);
			Canvas.SetTop(_currentDrawingRect, annotation.CanvasTop);
			_currentDrawingRect.Width = annotation.CanvasWidth;
			_currentDrawingRect.Height = annotation.CanvasHeight;
		}

		/// <summary>
		/// 刷新所有标注矩形的显示
		/// </summary>
		private void RefreshAnnotationRects()
		{
			if (_annotationCanvas == null || DataContext is not WellColumnViewModel vm) return;

			// 清除所有标注矩形（保留临时绘制矩形）
			for (int i = _annotationCanvas.Children.Count - 1; i >= 0; i--)
			{
				if (_annotationCanvas.Children[i] is Rectangle rect && rect != _currentDrawingRect)
				{
					_annotationCanvas.Children.RemoveAt(i);
				}
				else if (_annotationCanvas.Children[i] is Border)
				{
					_annotationCanvas.Children.RemoveAt(i);
				}
			}

			// 添加所有标注矩形
			foreach (var annotation in vm.Annotations)
			{
				AddAnnotationRect(annotation);
			}
		}

		/// <summary>
		/// 添加标注矩形到Canvas
		/// </summary>
		private void AddAnnotationRect(WellAnnotation annotation)
		{
			if (_annotationCanvas == null) return;

			var color = Color.Parse(annotation.Color);

			// 创建标注矩形
			var rect = new Rectangle
			{
				Width = annotation.CanvasWidth,
				Height = annotation.CanvasHeight,
				Stroke = new SolidColorBrush(color),
				StrokeThickness = annotation.IsSelected ? 3 : 2,
				Fill = new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B)),
				Tag = annotation,
				Cursor = new Cursor(StandardCursorType.Hand)
			};

			Canvas.SetLeft(rect, annotation.CanvasLeft);
			Canvas.SetTop(rect, annotation.CanvasTop);

			// 点击选中标注
			rect.PointerPressed += (s, e) =>
			{
				if (DataContext is WellColumnViewModel vm && !_isDrawing)
				{
					vm.SelectAnnotation(annotation);
					RefreshAnnotationRects();
					e.Handled = true;
				}
			};

			_annotationCanvas.Children.Add(rect);

			// 添加深度标签
			var label = new Border
			{
				Background = new SolidColorBrush(color),
				CornerRadius = new CornerRadius(2),
				Padding = new Thickness(4, 2),
				Child = new TextBlock
				{
					Text = $"{annotation.DepthTop:F0}-{annotation.DepthBottom:F0}m",
					FontSize = 9,
					Foreground = Brushes.White
				},
				Tag = annotation
			};

			Canvas.SetLeft(label, annotation.CanvasLeft);
			Canvas.SetTop(label, annotation.CanvasTop - 18);

			_annotationCanvas.Children.Add(label);
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
