using Avalonia.Data.Converters;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Globalization;

namespace DeepTime.LithoMind.Desktop.Converters;

public class EmojiIconConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string iconText && !string.IsNullOrWhiteSpace(iconText))
		{
			return new TextBlock
			{
				Text = iconText,
				FontFamily = new FontFamily("Segoe UI Emoji, Segoe UI Symbol, Apple Color Emoji, Noto Color Emoji"), 
				FontSize = 18, // 稍微大一点
				HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
				VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
			};
		}
		return null;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}