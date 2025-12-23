namespace DeepTime.LithoMind.Desktop.ViewModels.Base
{
	/// <summary>
	/// 所有功能切页（如单井、地震、制图）的抽象基类
	/// </summary>
	public abstract class PageViewModelBase : ViewModelBase
	{
		// 页面标题（显示在导航栏）
		public abstract string Title { get; }

		public abstract string IconKey { get; }

		// 页面索引/排序权重（可选，用于排序）
		public virtual int Order => 0;

		/// <summary>
		/// 当导航进入该页面时触发（例如：加载数据、开始动画）
		/// </summary>
		public virtual void OnNavigatedTo()
		{
			// 默认不做任何事，子类可按需重写
		}

		/// <summary>
		/// 当离开该页面时触发（例如：保存临时状态、暂停视频播放）
		/// </summary>
		public virtual void OnNavigatedFrom()
		{
			// 默认不做任何事，子类可按需重写
		}
	}
}