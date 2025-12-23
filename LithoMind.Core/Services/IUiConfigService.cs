using System.Threading.Tasks;
using LithoMind.Core.Models.UI;

namespace LithoMind.Core.Services
{
	/// <summary>
	/// UI 配置服务接口
	/// 职责：负责获取 UI 的布局定义（菜单、工具栏等）
	/// </summary>
	public interface IUiConfigService
	{
		/// <summary>
		/// 异步加载 UI 布局配置
		/// </summary>
		Task<UiLayoutConfig?> LoadConfigAsync();
	}
}