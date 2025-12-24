using System.Collections.Generic;

namespace DeepTime.LithoMind.Desktop.Models
{
	// 对应 json 根
	public class UiLayoutConfig
	{
		public List<MenuItemModel> globalMenu { get; set; }
		public List<ModuleToolbar> moduleToolbars { get; set; }
	}

	// 对应 moduleToolbars 里的每一项
	public class ModuleToolbar
	{
		public string id { get; set; }       // 例如 "Module_Seismic"
		public string moduleId { get; set; } // 例如 "Seismic"
		public List<MenuItemModel> items { get; set; }
	}

	// 对应菜单项
	public class MenuItemModel
	{
		public string id { get; set; }
		public string header { get; set; }
		public string icon { get; set; }
		public string commandId { get; set; }
		public string type { get; set; } // "Button", "SubMenu", "Separator"
		public List<MenuItemModel> children { get; set; }
	}
}