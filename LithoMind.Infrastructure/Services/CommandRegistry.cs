using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Windows.Input;

namespace LithoMind.Infrastructure.Services;

/// <summary>
/// 负责全局命令的注册与查找
/// </summary>
public class CommandRegistry
{
	private readonly Dictionary<string, ICommand> _commands = new();

	public void Register(string id, ICommand command)
	{
		if (string.IsNullOrWhiteSpace(id) || command is null) return;
		_commands[id] = command;
	}

	// 批量注册，用于注册那些只有日志功能的占位命令
	public void RegisterDummies(IEnumerable<string> ids, Action<string> actionFactory)
	{
		foreach (var id in ids)
		{
			Register(id, new RelayCommand(() => actionFactory(id)));
		}
	}

	public ICommand? Get(string id) => _commands.TryGetValue(id, out var cmd) ? cmd : null;

	// 专门为 NotifyCanExecuteChangedFor 获取强类型命令
	public IRelayCommand? GetRelayCommand(string id) => Get(id) as IRelayCommand;
}