using Steepshot.Core.Presenters;

namespace Steepshot.iOS.ViewControllers
{
    public abstract class BaseViewControllerWithPresenter<T> : BaseViewController where T : BasePresenter, new()
    {
        protected T _presenter;

        public override void ViewDidLoad()
        {
            CreatePresenter();
            base.ViewDidLoad();
        }

        protected virtual void CreatePresenter()
        {
            _presenter = new T();
        }
    }
}
