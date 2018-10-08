using Android.OS;
using Steepshot.Core.Extensions;
using Steepshot.Core.Presenters;

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
            Presenter = App.Container.GetPresenter<T>(App.MainChain);
        }

        protected override void OnDestroy()
        {
            Presenter.TasksCancel();
            base.OnDestroy();
        }
    }
}
