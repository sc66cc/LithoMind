using CommunityToolkit.Mvvm.ComponentModel;

namespace DeepTime.LithoMind.Desktop.ViewModels.Base
{
	// 继承链: PageViewModelBase -> ViewModelBase -> DockableBase
	public abstract partial class PageViewModelBase : ViewModelBase
	{
		[ObservableProperty]
		private string _iconKey = "Document";
		[ObservableProperty]
		private int _order;

		public PageViewModelBase()
		{
		}
	}
}