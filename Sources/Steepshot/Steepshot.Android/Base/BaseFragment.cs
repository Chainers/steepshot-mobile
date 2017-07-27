using Android.Content;

namespace Steepshot.Base
{
	public abstract class BaseFragment : Android.Support.V4.App.Fragment, BaseView
	{
		protected bool _isInitialized;
		protected Android.Views.View v;

		public override void OnViewCreated(Android.Views.View view, Android.OS.Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			CreatePresenter();
			_isInitialized = true;
		}

		protected abstract void CreatePresenter();

		public Context GetContext()
		{
			return Context;
		}

		public virtual bool CustomUserVisibleHint
		{
			get;
			set;
		}
	}
}
