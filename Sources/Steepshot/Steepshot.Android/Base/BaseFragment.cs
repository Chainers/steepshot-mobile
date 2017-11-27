using Steepshot.Core.Utils;

namespace Steepshot.Base
{
    public abstract class BaseFragment : Android.Support.V4.App.Fragment
    {
        protected bool IsInitialized;
        protected Android.Views.View InflatedView;

        public override void OnViewCreated(Android.Views.View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            IsInitialized = true;
        }

        public override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            BaseActivity.InitIoC();
            base.OnCreate(savedInstanceState);
        }

        public virtual bool CustomUserVisibleHint
        {
            get;
            set;
        }

        public override void OnDetach()
        {
            IsInitialized = false;
            base.OnDetach();
        }
    }
}
