using Steepshot.Core.Extensions;
using Steepshot.Core.Presenters;
﻿using Steepshot.Core.Utils;

namespace Steepshot.iOS.ViewControllers
{
    public abstract class BaseViewControllerWithPresenter<T> : BaseViewController where T : BasePresenter
    {
        protected T Presenter;

        protected BaseViewControllerWithPresenter()
        {
            CreatePresenter();
        }

        protected void CreatePresenter()
        {
            Presenter = AppDelegate.Container.GetPresenter<T>(AppDelegate.MainChain);
        }
    }
}