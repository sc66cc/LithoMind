using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DeepTime.LithoMind.Desktop.ViewModels;
using DeepTime.LithoMind.Desktop.Views;
using System.Linq; // [修复] 必须添加这个引用才能使用 OfType

namespace DeepTime.LithoMind.Desktop
{
	public partial class App : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				// 移除 Avalonia 自带的数据验证，避免干扰 MVVM
				DisableAvaloniaDataAnnotationValidation();

				// 直接初始化 MainViewModel
				var mainViewModel = new MainViewModel();

				desktop.MainWindow = new MainWindow
				{
					DataContext = mainViewModel
				};
			}

			base.OnFrameworkInitializationCompleted();
		}

		private void DisableAvaloniaDataAnnotationValidation()
		{
			// 获取并移除自带的数据验证插件
			var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
			foreach (var plugin in dataValidationPluginsToRemove)
			{
				BindingPlugins.DataValidators.Remove(plugin);
			}
		}
	}
}