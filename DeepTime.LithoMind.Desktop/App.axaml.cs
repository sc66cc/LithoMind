using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using LithoMind.Core.Services;
using LithoMind.Infrastructure.Services;
using DeepTime.LithoMind.Desktop.ViewModels;
using DeepTime.LithoMind.Desktop.Views;
using DeepTime.LithoMind.Desktop.Handlers;
using DeepTime.LithoMind.Desktop.Bootstrappers;
using System.Threading.Tasks; // 必须引用 Task

namespace DeepTime.LithoMind.Desktop;

public partial class App : Application
{
	public new static App? Current => (App?)Application.Current;
	public IServiceProvider? Services { get; private set; }

	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			try
			{
				var services = new ServiceCollection();
				ConfigureServices(services);
				Services = services.BuildServiceProvider();

				DisableAvaloniaDataAnnotationValidation();

				var mainViewModel = Services.GetRequiredService<MainViewModel>();
				desktop.MainWindow = new MainWindow
				{
					DataContext = mainViewModel
				};

				InitializeCommandsAsync();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[FATAL] 容器初始化失败: {ex}");
				throw;
			}
		}

		base.OnFrameworkInitializationCompleted();
	}

	private async void InitializeCommandsAsync()
	{
		try
		{
			// 从容器获取引导器
			var bootstrapper = Services.GetRequiredService<AppCommandBootstrapper>();

			// 异步等待注册完成，不卡界面
			await bootstrapper.RegisterAllAsync();

			System.Diagnostics.Debug.WriteLine("[App] 命令注册完成");
		}
		catch (Exception ex)
		{
			// 记录错误，方便调试
			System.Diagnostics.Debug.WriteLine($"[Error] 命令注册失败: {ex.Message}");
		}
	}

	private void ConfigureServices(IServiceCollection services)
	{
		// Infrastructure
		services.AddSingleton<CommandRegistry>();
		services.AddTransient<IUiConfigService, JsonUiConfigService>();

		// Handlers & Bootstrappers
		services.AddSingleton<GlobalMenuHandler>();
		services.AddTransient<AppCommandBootstrapper>();

		// ViewModels
		services.AddTransient<MainViewModel>();
	}

	private void DisableAvaloniaDataAnnotationValidation()
	{
		var validators = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
		foreach (var v in validators) BindingPlugins.DataValidators.Remove(v);
	}
}