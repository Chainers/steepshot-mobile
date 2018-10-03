using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;

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
            Presenter = AppSettings.GetPresenter<T>(AppSettings.MainChain);
        }
        
        public override void OnDetach()
        {
            Presenter.TasksCancel();
            base.OnDetach();
        }
    }
}
