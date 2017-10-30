using Android.Support.V4.App;

namespace Steepshot.Fragment
{
    public class BackStackFragment : Android.Support.V4.App.Fragment
    {
        public bool HandleBackPressed(FragmentManager fm)
        {
            if (fm?.Fragments == null)
                return false;

            foreach (var frag in fm.Fragments)
            {
                var backStack = frag as BackStackFragment;
                if (backStack != null && backStack.IsVisible)
                {
                    if (backStack.OnBackPressed())
                        return true;
                }
            }

            return false;
        }

        private bool OnBackPressed()
        {
            if (HandleBackPressed(ChildFragmentManager))
                return true;

            if (UserVisibleHint && ChildFragmentManager.BackStackEntryCount > 0)
            {
                ChildFragmentManager.PopBackStack();
                return true;
            }

            return false;
        }
    }
}
