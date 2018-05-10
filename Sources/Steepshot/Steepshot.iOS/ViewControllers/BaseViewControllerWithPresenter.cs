using System;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Views;

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

        protected virtual void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        protected void TagAction(string tag)
        {
            var myViewController = new PreSearchViewController();
            myViewController.CurrentPostCategory = tag;
            NavigationController.PushViewController(myViewController, true);
        }
    }
}
