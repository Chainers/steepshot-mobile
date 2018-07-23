using Steepshot.Core;
using Steepshot.Core.Presenters;

namespace Steepshot.Base
{
    public abstract class BaseFragmentWithPresenter<T> : BaseFragment where T : BasePresenter, new()
    {
        protected T Presenter;

        public override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Presenter == null)
                CreatePresenter();
        }

        private void CreatePresenter()
        {
            Presenter = new T();
            switch (App.MainChain)
            {
                case KnownChains.Golos:
                    Presenter.SetClient(App.GolosClient);
                    break;
                case KnownChains.Steem:
                    Presenter.SetClient(App.SteemClient);
                    break;
            }
        }

        public override void OnDetach()
        {
            Presenter.TasksCancel();
            base.OnDetach();
        }
    }
}
