using Android.Support.V4.App;

namespace Steepshot.Fragment
{
	public class BackStackFragment : Android.Support.V4.App.Fragment
	{
		public bool HandleBackPressed(FragmentManager fm)
		{
			if (fm?.Fragments != null)
			{
				foreach (Android.Support.V4.App.Fragment frag in fm.Fragments)
				{
					if (frag != null && frag.IsVisible && frag is BackStackFragment)
					{
						if (((BackStackFragment)frag).OnBackPressed())
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
			FragmentManager fm = ChildFragmentManager;
			if (HandleBackPressed(fm))
			{
				return true;
			}
			else if (UserVisibleHint && fm.BackStackEntryCount > 0)
			{
				fm.PopBackStack();
				return true;
			}
			return false;
		}
	}
}
