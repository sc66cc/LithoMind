using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	public class DataManagerViewModel : PageViewModelBase
	{
		public DataManagerViewModel()
		{
			Id = "DataManager";
			Title = "多源数据解析与融合";
			IconKey = "📂";
			Order = 1;
		}
	}
}