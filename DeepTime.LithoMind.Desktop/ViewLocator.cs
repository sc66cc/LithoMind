using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var fullName = param.GetType().FullName!;
            
            // 步骤1：先处理命名空间映射 ViewModels.Pages -> Views
            // 步骤2：再处理类名后缀 ViewModel -> View
            string name;
            
            if (fullName.Contains(".ViewModels.Pages."))
            {
                // ViewModels.Pages.XXXViewModel -> Views.XXXView
                name = fullName.Replace(".ViewModels.Pages.", ".Views.", StringComparison.Ordinal);
            }
            else if (fullName.Contains(".ViewModels."))
            {
                // ViewModels.XXXViewModel -> Views.XXXView  
                name = fullName.Replace(".ViewModels.", ".Views.", StringComparison.Ordinal);
            }
            else
            {
                name = fullName;
            }
            
            // 最后处理类名后缀
            name = name.Replace("ViewModel", "View", StringComparison.Ordinal);
            
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
