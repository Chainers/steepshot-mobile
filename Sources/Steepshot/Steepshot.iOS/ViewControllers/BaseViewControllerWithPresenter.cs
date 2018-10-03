using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;

namespace Steepshot.iOS.ViewControllers
{
    public abstract class BaseViewControllerWithPresenter<T> : BaseViewController where T : BasePresenter
    {
        protected T Presenter;

        public override void ViewDidLoad()
        {
            CreatePresenter();
            base.ViewDidLoad();
        }

        protected virtual void CreatePresenter()
        {
            Presenter = AppSettings.GetPresenter<T>(AppSettings.MainChain);
        }
    }
}
