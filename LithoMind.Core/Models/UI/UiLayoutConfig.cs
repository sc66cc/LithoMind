using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace LithoMind.Core.Models.UI;

public class UiLayoutConfig
{
	[JsonPropertyName("version")]
	public string Version { get; set; } = "1.0";
	
	[JsonPropertyName("comment")]
	public string? Comment { get; set; }

	[JsonPropertyName("globalMenu")]
	public List<MenuItemModel> GlobalMenu { get; set; } = new();
	
	[JsonPropertyName("globalToolbar")]
	public List<MenuItemModel> GlobalToolbar { get; set; } = new();
	
	[JsonPropertyName("contextToolbars")]
	public Dictionary<string, List<MenuItemModel>> ContextToolbars { get; set; } = new();

	/// <summary>
	/// 根据ViewModel类型名获取对应的模块菜单
	/// </summary>
	public List<MenuItemModel>? GetModuleMenus(string viewModelTypeName)
	{
		if (ContextToolbars.ContainsKey(viewModelTypeName))
		{
			return ContextToolbars[viewModelTypeName];
		}
		return null;
	}

	/// <summary>
	/// 递归获取整个配置中所有出现过的 CommandId (去重)
	/// </summary>
	public HashSet<string> GetAllCommandIds()
	{
		var ids = new HashSet<string>();

		// 1. 扫描全局菜单
		foreach (var item in GlobalMenu) CollectIds(item, ids);

		// 2. 扫描全局工具栏
		foreach (var item in GlobalToolbar) CollectIds(item, ids);

		// 3. 扫描所有场景工具栏
		foreach (var list in ContextToolbars.Values)
		{
			foreach (var item in list) CollectIds(item, ids);
		}

		return ids;
	}

	// 递归采集器
	private void CollectIds(MenuItemModel item, HashSet<string> ids)
	{
		if (!string.IsNullOrWhiteSpace(item.CommandId)) ids.Add(item.CommandId);
		if (item.Children != null)
		{
			foreach (var child in item.Children) CollectIds(child, ids);
		}
	}
}