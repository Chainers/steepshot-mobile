using Android.Support.V4.App;
using Steepshot.Base;

namespace Steepshot.Fragment
{
    public class BackStackFragment : Android.Support.V4.App.Fragment
    {
        protected BaseFragment Fragment;
        protected bool IsPopped;

        public bool HandleBackPressed(FragmentManager fm)
        {
            if (fm?.Fragments == null)
                return false;

            foreach (var frag in fm.Fragments)
            {
                var backStack = frag as BackStackFragment;
                if (backStack != null && backStack.UserVisibleHint)
                {
                    if (backStack.OnBackPressed())
                        return true;
                }
            }

            return false;
        }

        public bool OnBackPressed()
        {
            if (HandleBackPressed(ChildFragmentManager))
                return true;

            if (Fragment != null && Fragment.UserVisibleHint && ChildFragmentManager.BackStackEntryCount > 0)
            {
                IsPopped = true;
                ChildFragmentManager.PopBackStack();
                return true;
            }

            return false;
        }
    }
}
