using Android.Support.V4.App;
using Steepshot.Base;

namespace Steepshot.Fragment
{
    public class BackStackFragment : Android.Support.V4.App.Fragment
    {
        protected BaseFragment _fragment;
        protected bool _isPopped;

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

            if (_fragment != null && _fragment.UserVisibleHint && ChildFragmentManager.BackStackEntryCount > 0)
            {
                ChildFragmentManager.PopBackStack();
                _isPopped = true;
                return true;
            }

            return false;
        }
    }
}
