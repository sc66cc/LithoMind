using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 布尔值转背景色转换器
	/// </summary>
	public class BoolToBackgroundConverter : IValueConverter
	{
		public static readonly BoolToBackgroundConverter Instance = new();

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is bool isChecked && isChecked)
				return new SolidColorBrush(Color.Parse("#E3F2FD"));
			return Brushes.Transparent;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return false;
		}
	}
	/// <summary>
	/// 可用资源项（用于添加图层）
	/// </summary>
	public partial class AvailableResource : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private string _icon = "🗺️";

		[ObservableProperty]
		private string _category = string.Empty;

		[ObservableProperty]
		private bool _isChecked;
	}
	/// <summary>
	/// 图层节点 - 类似ArcGIS图层管理器
	/// </summary>
	public partial class MapLayerItem : ObservableObject
	{
		/// <summary>
		/// 图层名称
		/// </summary>
		[ObservableProperty]
		private string _name = string.Empty;

		/// <summary>
		/// 图层类型图标
		/// </summary>
		[ObservableProperty]
		private string _icon = "🗺️";

		/// <summary>
		/// 图层类型：Raster, Vector, Point, Line, Polygon, Label
		/// </summary>
		[ObservableProperty]
		private string _layerType = "Vector";

		/// <summary>
		/// 是否可见
		/// </summary>
		[ObservableProperty]
		private bool _isVisible = true;

		/// <summary>
		/// 是否选中（用于属性显示）
		/// </summary>
		[ObservableProperty]
		private bool _isSelected;

		/// <summary>
		/// 透明度 0-100
		/// </summary>
		[ObservableProperty]
		private int _opacity = 100;

		/// <summary>
		/// 图层顺序（越大越上层）
		/// </summary>
		[ObservableProperty]
		private int _zOrder;

		/// <summary>
		/// 符号颜色
		/// </summary>
		[ObservableProperty]
		private string _symbolColor = "#3498DB";

		/// <summary>
		/// 是否可编辑
		/// </summary>
		[ObservableProperty]
		private bool _isEditable;

		/// <summary>
		/// 是否展开
		/// </summary>
		[ObservableProperty]
		private bool _isExpanded = true;

		/// <summary>
		/// 子图层
		/// </summary>
		public ObservableCollection<MapLayerItem> Children { get; } = new();

		/// <summary>
		/// 是否有子图层
		/// </summary>
		public bool HasChildren => Children.Count > 0;

		/// <summary>
		/// 是否是分组图层
		/// </summary>
		public bool IsGroup => LayerType == "Group";
	}

	/// <summary>
	/// 岩相古地理智能编图 - 图层管理器ViewModel（类似ArcGIS）
	/// </summary>
	public partial class MappingLayerViewModel : PageViewModelBase
	{
		/// <summary>
		/// 图层集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<MapLayerItem> _layers = new();

		/// <summary>
		/// 当前选中的图层
		/// </summary>
		[ObservableProperty]
		private MapLayerItem? _selectedLayer;

		/// <summary>
		/// 可用资源列表（用于添加图层）
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<AvailableResource> _availableResources = new();

		/// <summary>
		/// 是否显示添加图层面板
		/// </summary>
		[ObservableProperty]
		private bool _showAddLayerPanel;

		/// <summary>
		/// 图层可见性改变事件
		/// </summary>
		public event Action<string, bool>? LayerVisibilityChanged;

		/// <summary>
		/// 图层顺序改变事件
		/// </summary>
		public event Action<string, int>? LayerOrderChanged;

		/// <summary>
		/// 图层选择事件
		/// </summary>
		public event Action<MapLayerItem>? LayerSelected;

		public MappingLayerViewModel()
		{
			Id = "MappingLayer";
			Title = "图层管理";
			IconKey = "🗂️";
			Order = 1;

			LoadSampleLayers();
			LoadAvailableResources();
		}

		/// <summary>
		/// 加载可用资源列表
		/// </summary>
		private void LoadAvailableResources()
		{
			AvailableResources.Clear();
			
			// 地震体数据
			AvailableResources.Add(new AvailableResource { Name = "地震体数据-主工区", Icon = "🌊", Category = "地震数据", IsChecked = false });
			AvailableResources.Add(new AvailableResource { Name = "地震层位-T1", Icon = "〰️", Category = "地震数据", IsChecked = false });
			AvailableResources.Add(new AvailableResource { Name = "地震层位-T2", Icon = "〰️", Category = "地震数据", IsChecked = false });
			
			// 井数据
			AvailableResources.Add(new AvailableResource { Name = "井位图-全部", Icon = "🛢️", Category = "井数据", IsChecked = false });
			AvailableResources.Add(new AvailableResource { Name = "井位图-探井", Icon = "⚫", Category = "井数据", IsChecked = false });
			AvailableResources.Add(new AvailableResource { Name = "井位图-开发井", Icon = "🔵", Category = "井数据", IsChecked = false });
			AvailableResources.Add(new AvailableResource { Name = "测井曲线数据", Icon = "📈", Category = "井数据", IsChecked = false });
			
			// 解释结果
			AvailableResources.Add(new AvailableResource { Name = "断层解释结果", Icon = "⚡", Category = "解释结果", IsChecked = false });
			AvailableResources.Add(new AvailableResource { Name = "沉积相解释结果", Icon = "🎨", Category = "解释结果", IsChecked = false });
			AvailableResources.Add(new AvailableResource { Name = "砂体等厚图", Icon = "📊", Category = "解释结果", IsChecked = false });
			AvailableResources.Add(new AvailableResource { Name = "砂地比分布图", Icon = "🟤", Category = "解释结果", IsChecked = false });
		}

		/// <summary>
		/// 显示添加图层面板
		/// </summary>
		[RelayCommand]
		public void ShowAddLayer()
		{
			ShowAddLayerPanel = true;
		}

		/// <summary>
		/// 取消添加图层
		/// </summary>
		[RelayCommand]
		public void CancelAddLayer()
		{
			ShowAddLayerPanel = false;
			// 重置选中状态
			foreach (var res in AvailableResources)
			{
				res.IsChecked = false;
			}
		}

		/// <summary>
		/// 确认添加图层
		/// </summary>
		[RelayCommand]
		public void ConfirmAddLayer()
		{
			var selectedResources = AvailableResources.Where(r => r.IsChecked).ToList();
			
			foreach (var res in selectedResources)
			{
				// 检查是否已存在
				if (!LayerExists(res.Name))
				{
					var newLayer = new MapLayerItem
					{
						Name = res.Name,
						Icon = res.Icon,
						LayerType = GetLayerTypeFromCategory(res.Category),
						ZOrder = Layers.Count * 10 + 100,
						IsVisible = true
					};
					Layers.Insert(0, newLayer); // 插入到顶部
				}
				res.IsChecked = false;
			}
			
			ShowAddLayerPanel = false;
		}

		private bool LayerExists(string name)
		{
			return Layers.Any(l => l.Name == name) || 
			       Layers.Any(l => l.Children.Any(c => c.Name == name));
		}

		private string GetLayerTypeFromCategory(string category)
		{
			return category switch
			{
				"地震数据" => "Raster",
				"井数据" => "Point",
				"解释结果" => "Polygon",
				_ => "Vector"
			};
		}

		/// <summary>
		/// 加载示例图层数据
		/// </summary>
		private void LoadSampleLayers()
		{
			Layers.Clear();

			// 基础底图组
			var baseMapGroup = new MapLayerItem
			{
				Name = "基础底图",
				Icon = "🗺️",
				LayerType = "Group",
				ZOrder = 0,
				IsExpanded = true
			};
			baseMapGroup.Children.Add(new MapLayerItem
			{
				Name = "行政边界",
				Icon = "📍",
				LayerType = "Polygon",
				ZOrder = 1,
				SymbolColor = "#95A5A6",
				Opacity = 80
			});
			baseMapGroup.Children.Add(new MapLayerItem
			{
				Name = "等高线",
				Icon = "〰️",
				LayerType = "Line",
				ZOrder = 2,
				SymbolColor = "#8B4513",
				Opacity = 60
			});
			baseMapGroup.Children.Add(new MapLayerItem
			{
				Name = "水系",
				Icon = "💧",
				LayerType = "Line",
				ZOrder = 3,
				SymbolColor = "#3498DB"
			});

			// 井位图层组
			var wellGroup = new MapLayerItem
			{
				Name = "井位数据",
				Icon = "🛢️",
				LayerType = "Group",
				ZOrder = 10,
				IsExpanded = true
			};
			wellGroup.Children.Add(new MapLayerItem
			{
				Name = "探井",
				Icon = "⚫",
				LayerType = "Point",
				ZOrder = 11,
				SymbolColor = "#E74C3C"
			});
			wellGroup.Children.Add(new MapLayerItem
			{
				Name = "开发井",
				Icon = "🔵",
				LayerType = "Point",
				ZOrder = 12,
				SymbolColor = "#3498DB"
			});
			wellGroup.Children.Add(new MapLayerItem
			{
				Name = "评价井",
				Icon = "🟢",
				LayerType = "Point",
				ZOrder = 13,
				SymbolColor = "#27AE60"
			});

			// 沉积相图层组
			var faciesGroup = new MapLayerItem
			{
				Name = "沉积相分析",
				Icon = "🎨",
				LayerType = "Group",
				ZOrder = 20,
				IsExpanded = true
			};
			faciesGroup.Children.Add(new MapLayerItem
			{
				Name = "砂体等厚线",
				Icon = "📊",
				LayerType = "Line",
				ZOrder = 21,
				SymbolColor = "#F39C12"
			});
			faciesGroup.Children.Add(new MapLayerItem
			{
				Name = "砂地比分布",
				Icon = "🟫",
				LayerType = "Raster",
				ZOrder = 22,
				SymbolColor = "#E67E22",
				Opacity = 70
			});
			faciesGroup.Children.Add(new MapLayerItem
			{
				Name = "沉积相边界",
				Icon = "🔲",
				LayerType = "Polygon",
				ZOrder = 23,
				SymbolColor = "#9B59B6"
			});

			// 构造图层组
			var structureGroup = new MapLayerItem
			{
				Name = "构造要素",
				Icon = "📐",
				LayerType = "Group",
				ZOrder = 30,
				IsExpanded = true
			};
			structureGroup.Children.Add(new MapLayerItem
			{
				Name = "断层线",
				Icon = "⚡",
				LayerType = "Line",
				ZOrder = 31,
				SymbolColor = "#E74C3C",
				IsEditable = true
			});
			structureGroup.Children.Add(new MapLayerItem
			{
				Name = "等深线",
				Icon = "🌀",
				LayerType = "Line",
				ZOrder = 32,
				SymbolColor = "#1ABC9C"
			});
			structureGroup.Children.Add(new MapLayerItem
			{
				Name = "圈闭边界",
				Icon = "⭕",
				LayerType = "Polygon",
				ZOrder = 33,
				SymbolColor = "#E74C3C",
				Opacity = 50
			});

			// 标注图层
			var labelLayer = new MapLayerItem
			{
				Name = "标注",
				Icon = "🏷️",
				LayerType = "Label",
				ZOrder = 100
			};

			Layers.Add(labelLayer);
			Layers.Add(structureGroup);
			Layers.Add(faciesGroup);
			Layers.Add(wellGroup);
			Layers.Add(baseMapGroup);
		}

		/// <summary>
		/// 选择图层
		/// </summary>
		partial void OnSelectedLayerChanged(MapLayerItem? value)
		{
			if (value != null)
			{
				LayerSelected?.Invoke(value);
			}
		}

		/// <summary>
		/// 切换图层可见性
		/// </summary>
		[RelayCommand]
		public void ToggleLayerVisibility(MapLayerItem? layer)
		{
			if (layer != null)
			{
				layer.IsVisible = !layer.IsVisible;
				LayerVisibilityChanged?.Invoke(layer.Name, layer.IsVisible);
				
				// 如果是分组，递归切换子图层
				if (layer.IsGroup)
				{
					foreach (var child in layer.Children)
					{
						child.IsVisible = layer.IsVisible;
						LayerVisibilityChanged?.Invoke(child.Name, child.IsVisible);
					}
				}
			}
		}

		/// <summary>
		/// 上移图层
		/// </summary>
		[RelayCommand]
		public void MoveLayerUp(MapLayerItem? layer)
		{
			if (layer == null) return;
			
			var index = Layers.IndexOf(layer);
			if (index > 0)
			{
				Layers.Move(index, index - 1);
				UpdateZOrders();
				LayerOrderChanged?.Invoke(layer.Name, layer.ZOrder);
			}
		}

		/// <summary>
		/// 下移图层
		/// </summary>
		[RelayCommand]
		public void MoveLayerDown(MapLayerItem? layer)
		{
			if (layer == null) return;
			
			var index = Layers.IndexOf(layer);
			if (index < Layers.Count - 1)
			{
				Layers.Move(index, index + 1);
				UpdateZOrders();
				LayerOrderChanged?.Invoke(layer.Name, layer.ZOrder);
			}
		}

		/// <summary>
		/// 更新所有图层的Z顺序
		/// </summary>
		private void UpdateZOrders()
		{
			for (int i = 0; i < Layers.Count; i++)
			{
				Layers[i].ZOrder = (Layers.Count - i) * 10;
			}
		}

		/// <summary>
		/// 全部显示
		/// </summary>
		[RelayCommand]
		public void ShowAllLayers()
		{
			SetAllLayersVisibility(Layers, true);
		}

		/// <summary>
		/// 全部隐藏
		/// </summary>
		[RelayCommand]
		public void HideAllLayers()
		{
			SetAllLayersVisibility(Layers, false);
		}

		private void SetAllLayersVisibility(ObservableCollection<MapLayerItem> layers, bool visible)
		{
			foreach (var layer in layers)
			{
				layer.IsVisible = visible;
				LayerVisibilityChanged?.Invoke(layer.Name, visible);
				
				if (layer.HasChildren)
				{
					SetAllLayersVisibility(layer.Children, visible);
				}
			}
		}

		/// <summary>
		/// 设置图层透明度
		/// </summary>
		[RelayCommand]
		public void SetLayerOpacity(MapLayerItem? layer)
		{
			// 透明度已通过双向绑定更新
		}

		/// <summary>
		/// 缩放到图层
		/// </summary>
		[RelayCommand]
		public void ZoomToLayer(MapLayerItem? layer)
		{
			if (layer != null)
			{
				// 实际应用中会发送缩放事件
				System.Diagnostics.Debug.WriteLine($"缩放到图层: {layer.Name}");
			}
		}
	}
}
