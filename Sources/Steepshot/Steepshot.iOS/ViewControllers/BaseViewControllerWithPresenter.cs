using Steepshot.Core.Presenters;

namespace Steepshot.iOS.ViewControllers
{
    public abstract class BaseViewControllerWithPresenter<T> : BaseViewController where T : BasePresenter
    {
        protected T _presenter;

        public override void ViewDidLoad()
        {
            CreatePresenter();
            base.ViewDidLoad();
        }

        protected abstract void CreatePresenter();
    }
}
