using Android.Content;

namespace Steepshot.Base
{
    public abstract class BaseFragment : Android.Support.V4.App.Fragment, IBaseView
    {
        protected bool IsInitialized;
        protected Android.Views.View V;

        public override void OnViewCreated(Android.Views.View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            CreatePresenter();
            IsInitialized = true;
        }

        protected virtual void CreatePresenter() { }

        public Context GetContext()
        {
            return Context;
        }

        public virtual bool CustomUserVisibleHint
        {
            get;
            set;
        }
    }
}
