using Steepshot.Core.Presenters;

namespace Steepshot.Base
{
    public abstract class BaseFragmentWithPresenter<T> : BaseFragment where T : BasePresenter
    {
        protected T _presenter;
        public override void OnViewCreated(Android.Views.View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            CreatePresenter();
        }

        protected abstract void CreatePresenter();
    }
}
