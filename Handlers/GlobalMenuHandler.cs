using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace DeepTime.LithoMind.Desktop.Handlers;

public partial class GlobalMenuHandler : ObservableObject
{
	// 这里未来注入 IProjectService, IDataService 等

	[RelayCommand]
	private async Task OpenProject()
	{
		await Task.Delay(50);
		System.Diagnostics.Debug.WriteLine("[Handler] Open Project");
	}

	[RelayCommand]
	private async Task SaveProject()
	{
		await Task.Delay(50);
		System.Diagnostics.Debug.WriteLine("[Handler] Save Project");
	}

	[RelayCommand]
	private void ExitApp()
	{
		if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.Shutdown();
		}
	}

	[RelayCommand] private void Undo() => System.Diagnostics.Debug.WriteLine("[Handler] Undo");
	[RelayCommand] private void Redo() => System.Diagnostics.Debug.WriteLine("[Handler] Redo");

}