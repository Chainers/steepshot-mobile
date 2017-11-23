using Steepshot.Core.Presenters;

namespace Steepshot.Base
{
    public abstract class BaseFragmentWithPresenter<T> : BaseFragment where T : BasePresenter, new()
    {
        protected static T Presenter;
        public override void OnViewCreated(Android.Views.View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            if (Presenter == null)
                CreatePresenter();
        }

        private void CreatePresenter()
        {
            Presenter = new T();
        }

        public override void OnDetach()
        {
            Presenter.TasksCancel();
            base.OnDetach();
        }
    }
}
