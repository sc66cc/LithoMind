using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	public class DataManagerViewModel : PageViewModelBase
	{
		public DataManagerViewModel()
		{
			Id = "DataManager";
			Title = "数据管理";
			IconKey = "📂";
			Order = 1;
		}
	}
}