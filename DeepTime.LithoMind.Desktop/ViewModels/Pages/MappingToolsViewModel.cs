using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// GIS工具项
	/// </summary>
	public partial class GisToolItem : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private string _icon = string.Empty;

		[ObservableProperty]
		private string _category = string.Empty;

		[ObservableProperty]
		private string _description = string.Empty;

		[ObservableProperty]
		private bool _isEnabled = true;
	}

	/// <summary>
	/// 符号化设置
	/// </summary>
	public partial class SymbolSettings : ObservableObject
	{
		[ObservableProperty]
		private string _fillColor = "#3498DB";

		[ObservableProperty]
		private string _strokeColor = "#2C3E50";

		[ObservableProperty]
		private double _strokeWidth = 1.0;

		[ObservableProperty]
		private int _opacity = 100;

		[ObservableProperty]
		private string _symbolType = "Simple"; // Simple, Graduated, Categorical

		[ObservableProperty]
		private double _pointSize = 8.0;
	}

	/// <summary>
	/// 编图制图 - GIS工具栏和属性窗口
	/// </summary>
	public partial class MappingToolsViewModel : PageViewModelBase
	{
		/// <summary>
		/// 当前工具分类
		/// </summary>
		[ObservableProperty]
		private string _currentCategory = "空间分析";

		/// <summary>
		/// 空间分析工具
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<GisToolItem> _spatialAnalysisTools = new();

		/// <summary>
		/// 绘图工具
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<GisToolItem> _drawingTools = new();

		/// <summary>
		/// 可视化工具
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<GisToolItem> _visualizationTools = new();

		/// <summary>
		/// 当前选中的图层名称
		/// </summary>
		[ObservableProperty]
		private string _selectedLayerName = "未选择图层";

		/// <summary>
		/// 符号化设置
		/// </summary>
		[ObservableProperty]
		private SymbolSettings _symbolSettings = new();

		/// <summary>
		/// 是否显示属性面板
		/// </summary>
		[ObservableProperty]
		private bool _showPropertyPanel = true;

		/// <summary>
		/// 是否有选中图层
		/// </summary>
		[ObservableProperty]
		private bool _hasSelectedLayer;
		
		/// <summary>
		/// 当前选中的图层
		/// </summary>
		[ObservableProperty]
		private MapLayerItem? _selectedLayer;
		
		/// <summary>
		/// 工具栏标签页是否激活
		/// </summary>
		[ObservableProperty]
		private bool _isToolsTabActive = true;
		
		/// <summary>
		/// 属性栏标签页是否激活
		/// </summary>
		[ObservableProperty]
		private bool _isPropertiesTabActive;
		
		/// <summary>
		/// 当工具栏标签页激活状态改变时
		/// </summary>
		partial void OnIsToolsTabActiveChanged(bool value)
		{
			if (value)
				IsPropertiesTabActive = false;
		}
		
		/// <summary>
		/// 当属性栏标签页激活状态改变时
		/// </summary>
		partial void OnIsPropertiesTabActiveChanged(bool value)
		{
			if (value)
				IsToolsTabActive = false;
		}

		public MappingToolsViewModel()
		{
			Id = "MappingTools";
			Title = "工具栏";
			IconKey = "🛠️";
			Order = 3;

			LoadTools();
		}

		private void LoadTools()
		{
			// 空间分析工具
			SpatialAnalysisTools.Add(new GisToolItem { Name = "缓冲区分析", Icon = "⭕", Category = "空间分析", Description = "创建要素周围的缓冲区" });
			SpatialAnalysisTools.Add(new GisToolItem { Name = "叠置分析", Icon = "🔲", Category = "空间分析", Description = "多图层叠加分析" });
			SpatialAnalysisTools.Add(new GisToolItem { Name = "邻近分析", Icon = "📍", Category = "空间分析", Description = "分析要素间的邻近关系" });
			SpatialAnalysisTools.Add(new GisToolItem { Name = "插值分析", Icon = "📈", Category = "空间分析", Description = "点数据插值生成栅格" });
			SpatialAnalysisTools.Add(new GisToolItem { Name = "密度分析", Icon = "🎯", Category = "空间分析", Description = "计算点或线的密度分布" });
			SpatialAnalysisTools.Add(new GisToolItem { Name = "等值线提取", Icon = "〰️", Category = "空间分析", Description = "从栅格提取等值线" });
			SpatialAnalysisTools.Add(new GisToolItem { Name = "裁剪", Icon = "✂️", Category = "空间分析", Description = "按边界裁剪数据" });
			SpatialAnalysisTools.Add(new GisToolItem { Name = "合并", Icon = "🔗", Category = "空间分析", Description = "合并多个图层" });

			// 绘图工具
			DrawingTools.Add(new GisToolItem { Name = "绘制点", Icon = "⚫", Category = "绘图", Description = "绘制点要素" });
			DrawingTools.Add(new GisToolItem { Name = "绘制线", Icon = "📏", Category = "绘图", Description = "绘制线要素" });
			DrawingTools.Add(new GisToolItem { Name = "绘制多边形", Icon = "⬛", Category = "绘图", Description = "绘制多边形要素" });
			DrawingTools.Add(new GisToolItem { Name = "绘制圆", Icon = "⭕", Category = "绘图", Description = "绘制圆形" });
			DrawingTools.Add(new GisToolItem { Name = "绘制矩形", Icon = "▢", Category = "绘图", Description = "绘制矩形" });
			DrawingTools.Add(new GisToolItem { Name = "添加文字", Icon = "🔤", Category = "绘图", Description = "添加文字标注" });
			DrawingTools.Add(new GisToolItem { Name = "编辑节点", Icon = "✏️", Category = "绘图", Description = "编辑要素节点" });
			DrawingTools.Add(new GisToolItem { Name = "删除要素", Icon = "🗑️", Category = "绘图", Description = "删除选中要素" });

			// 可视化工具
			VisualizationTools.Add(new GisToolItem { Name = "分级渲染", Icon = "🎨", Category = "可视化", Description = "按数值分级显示" });
			VisualizationTools.Add(new GisToolItem { Name = "唯一值渲染", Icon = "🔵", Category = "可视化", Description = "按类别唯一值显示" });
			VisualizationTools.Add(new GisToolItem { Name = "密度图", Icon = "🌡️", Category = "可视化", Description = "生成热力密度图" });
			VisualizationTools.Add(new GisToolItem { Name = "等值面图", Icon = "🗺️", Category = "可视化", Description = "生成等值面填充图" });
			VisualizationTools.Add(new GisToolItem { Name = "3D视图", Icon = "🏔️", Category = "可视化", Description = "切换到3D视图" });
			VisualizationTools.Add(new GisToolItem { Name = "图例设置", Icon = "📋", Category = "可视化", Description = "配置图例样式" });
			VisualizationTools.Add(new GisToolItem { Name = "标注设置", Icon = "🏷️", Category = "可视化", Description = "配置标注样式" });
			VisualizationTools.Add(new GisToolItem { Name = "导出地图", Icon = "📤", Category = "可视化", Description = "导出地图图片" });
		}

		/// <summary>
		/// 选择工具
		/// </summary>
		[RelayCommand]
		public void SelectTool(GisToolItem? tool)
		{
			if (tool != null)
			{
				System.Diagnostics.Debug.WriteLine($"选择工具: {tool.Name}");
			}
		}

		/// <summary>
		/// 切换分类
		/// </summary>
		[RelayCommand]
		public void SwitchCategory(string category)
		{
			CurrentCategory = category;
		}

		/// <summary>
		/// 应用符号化
		/// </summary>
		[RelayCommand]
		public void ApplySymbol()
		{
			System.Diagnostics.Debug.WriteLine($"应用符号化设置");
		}

		/// <summary>
		/// 重置符号化
		/// </summary>
		[RelayCommand]
		public void ResetSymbol()
		{
			SymbolSettings = new SymbolSettings();
		}

		/// <summary>
		/// 设置选中的图层
		/// </summary>
		public void SetSelectedLayer(MapLayerItem? layer)
		{
			if (layer != null)
			{
				SelectedLayer = layer;
				SelectedLayerName = layer.Name;
				HasSelectedLayer = true;
				SymbolSettings.FillColor = layer.SymbolColor;
				SymbolSettings.Opacity = layer.Opacity;
				// 切换到属性标签页
				IsPropertiesTabActive = true;
			}
			else
			{
				SelectedLayerName = "未选择图层";
				HasSelectedLayer = false;
			}
		}
	}
}
