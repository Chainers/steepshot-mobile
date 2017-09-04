using Android.Widget;
using Steepshot.Core;
using Steepshot.Core.Presenters;

namespace Steepshot.Base
{
    public class BaseFragmentWithPresenter<T> : BaseFragment where T : BasePresenter
    {
        protected T _presenter;
        public override void OnViewCreated(Android.Views.View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            CreatePresenter();
        }

        protected virtual void CreatePresenter()
        {
            _presenter.InternetConnectionWarning += () =>
            {
                Toast.MakeText(Context, Localization.Errors.InternetUnavailable, ToastLength.Long).Show();
            };
        }
    }
}
