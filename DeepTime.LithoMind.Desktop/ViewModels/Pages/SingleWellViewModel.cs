using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	public class SingleWellViewModel : PageViewModelBase
	{
		public SingleWellViewModel ()
		{
			Id = "Wells";
			Title = "井数据综合";
			IconKey = "📊";
			Order = 2;
		}
	}
}