using Android.Support.V4.App;

namespace Steepshot.Fragment
{
    public class BackStackFragment : Android.Support.V4.App.Fragment
    {
        public bool HandleBackPressed(FragmentManager fm)
        {
            if (fm?.Fragments != null)
            {
                foreach (var frag in fm.Fragments)
                {
                    var backStack = frag as BackStackFragment;
                    if (backStack != null && backStack.IsVisible)
                    {
                        if (backStack.OnBackPressed())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected bool OnBackPressed()
        {
            if (HandleBackPressed(ChildFragmentManager))
                return true;
            else if (UserVisibleHint && ChildFragmentManager.BackStackEntryCount > 0)
            {
                ChildFragmentManager.PopBackStack();
                return true;
            }
            return false;
        }
    }
}
