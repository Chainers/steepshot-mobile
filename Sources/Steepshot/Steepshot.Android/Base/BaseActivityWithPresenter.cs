using Android.OS;
using Android.Widget;
using Steepshot.Core;
using Steepshot.Core.Presenters;

namespace Steepshot.Base
{
    public class BaseActivityWithPresenter<T> : BaseActivity where T : BasePresenter
    {
        protected T _presenter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CreatePresenter();
        }

        protected virtual void CreatePresenter()
        {
            _presenter.InternetConnectionWarning += () =>
            {
                Toast.MakeText(this, Localization.Errors.InternetUnavailable, ToastLength.Long).Show();
            };
        }
    }
}
