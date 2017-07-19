using Android.Support.V4.App;

namespace Steepshot
{
	public class BackStackFragment : Fragment
	{
		public bool HandleBackPressed(FragmentManager fm)
		{
			if (fm?.Fragments != null)
			{
				foreach (Fragment frag in fm.Fragments)
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
