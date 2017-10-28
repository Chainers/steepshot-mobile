using System;
using Android.Support.V7.App;
using Steepshot.Fragment;

namespace Steepshot.Base
{
    public abstract class BaseActivity : AppCompatActivity
    {
        protected HostFragment CurrentHostFragment;

        public override void OnBackPressed()
        {
            if (CurrentHostFragment == null || !CurrentHostFragment.HandleBackPressed(SupportFragmentManager))
                base.OnBackPressed();
        }

        public virtual void OpenNewContentFragment(Android.Support.V4.App.Fragment frag)
        {
            CurrentHostFragment?.ReplaceFragment(frag, true);
        }

        public override void OnTrimMemory(Android.Content.TrimMemory level)
        {
            GC.Collect();
            base.OnTrimMemory(level);
        }
    }
}
