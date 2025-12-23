using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace LithoMind.Core.Models.UI
{
	public class MenuItemModel
	{
		public string Id { get; set; } = string.Empty;
		public string Header { get; set; } = string.Empty;
		public string? Icon { get; set; }
		public string? CommandId { get; set; }
		public string? InputGesture { get; set; }
		public string? ToolTip { get; set; }
		public MenuItemType Type { get; set; } = MenuItemType.Button;

		public List<MenuItemModel> Children { get; set; } = new List<MenuItemModel>();

		[JsonIgnore]
		public ICommand? Command { get; set; }

		// 辅助属性
		public bool IsSeparator => Type == MenuItemType.Separator;
		public bool IsButton => Type == MenuItemType.Button;
	}
}