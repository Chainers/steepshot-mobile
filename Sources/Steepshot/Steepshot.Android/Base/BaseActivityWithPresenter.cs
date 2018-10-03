using Android.OS;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;

namespace Steepshot.Base
{
    public abstract class BaseActivityWithPresenter<T> : BaseActivity where T : BasePresenter
    {
        protected T Presenter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Presenter == null)
                CreatePresenter();
        }

        private void CreatePresenter()
        {
            Presenter = AppSettings.GetPresenter<T>(AppSettings.MainChain);
        }

        protected override void OnDestroy()
        {
            Presenter.TasksCancel();
            base.OnDestroy();
        }
    }
}
