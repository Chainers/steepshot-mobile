using Steepshot.Core;
using Steepshot.Core.Presenters;

namespace Steepshot.iOS.ViewControllers
{
    public abstract class BaseViewControllerWithPresenter<T> : BaseViewController where T : BasePresenter, new()
    {
        protected T _presenter;

        protected BaseViewControllerWithPresenter()
        {
            CreatePresenter();
        }

        protected void CreatePresenter()
        {
            _presenter = new T();

            switch (AppDelegate.MainChain)
            {
                case KnownChains.Golos:
                    _presenter.SetClient(AppDelegate.GolosClient);
                    break;
                case KnownChains.Steem:
                    _presenter.SetClient(AppDelegate.SteemClient);
                    break;
            }
        }
    }
}
