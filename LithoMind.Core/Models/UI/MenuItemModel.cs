using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace LithoMind.Core.Models.UI
{
	public class MenuItemModel
	{
		[JsonPropertyName("id")]
		public string Id { get; set; } = string.Empty;
		
		[JsonPropertyName("header")]
		public string Header { get; set; } = string.Empty;
		
		[JsonPropertyName("icon")]
		public string? Icon { get; set; }
		
		[JsonPropertyName("commandId")]
		public string? CommandId { get; set; }
		
		[JsonPropertyName("inputGesture")]
		public string? InputGesture { get; set; }
		
		[JsonPropertyName("toolTip")]
		public string? ToolTip { get; set; }
		
		[JsonPropertyName("type")]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public MenuItemType Type { get; set; } = MenuItemType.Button;

		[JsonPropertyName("children")]
		public List<MenuItemModel>? Children { get; set; }

		[JsonIgnore]
		public ICommand? Command { get; set; }

		// 辅助属性
		[JsonIgnore]
		public bool IsSeparator => Type == MenuItemType.Separator;
		
		[JsonIgnore]
		public bool IsButton => Type == MenuItemType.Button;
		
		[JsonIgnore]
		public bool IsSubMenu => Type == MenuItemType.SubMenu;
		
		[JsonIgnore]
		public bool HasChildren => Children != null && Children.Count > 0;
	}
}