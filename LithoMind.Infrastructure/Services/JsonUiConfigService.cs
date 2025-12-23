using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LithoMind.Core.Models.UI;
using LithoMind.Core.Services;

namespace LithoMind.Infrastructure.Services;

public class JsonUiConfigService : IUiConfigService
{
	private const string ConfigRelativePath = @"Assets\config\ui_layout.json";

	// 将配置项静态化，提升复用效率并保持代码整洁
	private static readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		AllowTrailingCommas = true,
		IgnoreReadOnlyProperties = true,
		Converters = { new JsonStringEnumConverter() } // 关键：支持字符串转枚举
	};

	public async Task<UiLayoutConfig?> LoadConfigAsync()
	{
		var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigRelativePath);

		if (!File.Exists(fullPath))
		{
			return CreateFallbackConfig("配置文件未找到");
		}

		try
		{
			using var stream = File.OpenRead(fullPath);
			return await JsonSerializer.DeserializeAsync<UiLayoutConfig>(stream, _jsonOptions);
		}
		catch
		{
			// 生产环境建议在此处记录日志 (Logger.LogError)
			return CreateFallbackConfig("配置解析异常");
		}
	}

	private static UiLayoutConfig CreateFallbackConfig(string errorMessage)
	{
		var config = new UiLayoutConfig();

		// 构建一个显眼的错误提示菜单
		var errorMenu = new MenuItemModel
		{
			Header = $"⚠️ {errorMessage}",
			Type = MenuItemType.SubMenu
		};

		errorMenu.Children.Add(new MenuItemModel { Header = "请检查程序完整性" });
		config.GlobalMenu.Add(errorMenu);

		return config;
	}
}