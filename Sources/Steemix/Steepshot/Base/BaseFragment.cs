using System;
using Android.Content;
using Android.Support.V4.App;

namespace Steepshot
{
	public abstract class BaseFragment : Fragment, BaseView
	{
		public BaseFragment()
		{
		}

		public override void OnViewCreated(Android.Views.View view, Android.OS.Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			CreatePresenter();
		}

		protected abstract void CreatePresenter();

		public Context GetContext()
		{
			return this.Context;
		}
	}
}
