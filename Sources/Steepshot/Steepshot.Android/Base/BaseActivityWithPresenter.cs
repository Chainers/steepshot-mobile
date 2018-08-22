using Android.OS;
using Steepshot.Core;
using Steepshot.Core.Presenters;

namespace Steepshot.Base
{
    public abstract class BaseActivityWithPresenter<T> : BaseActivity where T : BasePresenter, new()
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

        protected override void OnDestroy()
        {
            Presenter.TasksCancel();
            base.OnDestroy();
        }
    }
}
