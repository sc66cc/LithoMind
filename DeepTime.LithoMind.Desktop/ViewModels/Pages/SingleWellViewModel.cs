using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	public class SingleWellViewModel : PageViewModelBase
	{
		public SingleWellViewModel ()
		{
			Id = "Wells";
			Title = "单井相智能分析";
			IconKey = "📊";
			Order = 2;
		}
	}
}