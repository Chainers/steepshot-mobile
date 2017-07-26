using Android.Content;
using Android.Support.V4.App;
using Android.Views;

namespace Steepshot
{
	public abstract class BaseFragment : Fragment, BaseView
	{
		protected bool _isInitialized;
		protected View v;

		public override void OnViewCreated(View view, Android.OS.Bundle savedInstanceState)
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
