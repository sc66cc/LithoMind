using DeepTime.LithoMind.Desktop.Handlers;
using LithoMind.Core.Services;
using LithoMind.Infrastructure.Services;
using System.Threading.Tasks;

namespace DeepTime.LithoMind.Desktop.Bootstrappers;

public class AppCommandBootstrapper
{
	private readonly CommandRegistry _registry;
	private readonly GlobalMenuHandler _globalHandler;
	private readonly IUiConfigService _configService; // 需要注入配置服务

	public AppCommandBootstrapper(
		CommandRegistry registry,
		GlobalMenuHandler globalHandler,
		IUiConfigService configService)
	{
		_registry = registry;
		_globalHandler = globalHandler;
		_configService = configService;
	}

	// 改为异步方法，因为要读取 JSON 文件
	public async Task RegisterAllAsync()
	{
		// 1. 先注册“有血有肉”的真实业务命令
		RegisterCoreCommands();

		// 2. 剩下的 ID，统统从 JSON 里读出来注册为“哑巴命令”
		await RegisterDynamicDummiesAsync();
	}

	private void RegisterCoreCommands()
	{
		_registry.Register("Cmd_OpenProject", _globalHandler.OpenProjectCommand);
		_registry.Register("Cmd_SaveProject", _globalHandler.SaveProjectCommand);
		_registry.Register("Cmd_Undo", _globalHandler.UndoCommand);
		_registry.Register("Cmd_Redo", _globalHandler.RedoCommand);
		_registry.Register("Cmd_ExitApp", _globalHandler.ExitAppCommand);
	}

	private async Task RegisterDynamicDummiesAsync()
	{
		// 加载配置
		var config = await _configService.LoadConfigAsync();
		if (config == null) return;

		// 获取 JSON 里写过的所有 ID
		var allIdsInJson = config.GetAllCommandIds();

		foreach (var id in allIdsInJson)
		{
			// 如果注册表里没有这个 ID (说明程序员还没来得及写具体功能)
			// 那就给它自动注册一个“占位符”，保证按钮能点，且控制台有日志
			if (_registry.Get(id) == null)
			{
				_registry.RegisterDummies(new[] { id },
					x => System.Diagnostics.Debug.WriteLine($"[TODO] 功能尚未实现: {x}"));
			}
		}
	}
}