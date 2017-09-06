using Android.OS;
using Steepshot.Core.Presenters;

namespace Steepshot.Base
{
    public abstract class BaseActivityWithPresenter<T> : BaseActivity where T : BasePresenter
    {
        protected T _presenter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CreatePresenter();
        }

        protected abstract void CreatePresenter();
    }
}
