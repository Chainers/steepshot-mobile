using Steepshot.Core.Extensions;
using Steepshot.Core.Presenters;

namespace Steepshot.Base
{
    public abstract class BaseFragmentWithPresenter<T> : BaseFragment
        where T : BasePresenter
    {
        protected T Presenter;

        public override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Presenter == null)
                CreatePresenter();
        }

        protected virtual void CreatePresenter()
        {
            Presenter = App.Container.GetPresenter<T>(App.MainChain);
        }

        public override void OnDetach()
        {
            Presenter.TasksCancel();
            base.OnDetach();
        }
    }
}
