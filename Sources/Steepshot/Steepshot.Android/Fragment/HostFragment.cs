using Steepshot.Base;


namespace Steepshot.Fragment
{
	public class HostFragment : BackStackFragment
	{
		private Android.Support.V4.App.Fragment _fragment;

		public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);
			var view = inflater.Inflate(Resource.Layout.HostLayout, container, false);
			if (_fragment != null)
			{
				ReplaceFragment(_fragment, false);
			}
			return view;
		}

		public void ReplaceFragment(Android.Support.V4.App.Fragment fragment, bool addToBackstack)
		{
			if (addToBackstack)
			{
				ChildFragmentManager.BeginTransaction().Replace(Resource.Id.child_fragment_container, fragment).AddToBackStack(null).Commit();
			}
			else
			{
				ChildFragmentManager.BeginTransaction().Replace(Resource.Id.child_fragment_container, fragment).Commit();
			}
		}

		public static HostFragment NewInstance(Android.Support.V4.App.Fragment fragment)
		{
			HostFragment hostFragment = new HostFragment();
			hostFragment._fragment = fragment;
			return hostFragment;
		}

		public override bool UserVisibleHint
		{
			get
			{
				return base.UserVisibleHint;
			}
			set
			{
				((BaseFragment)_fragment).CustomUserVisibleHint = value;
				base.UserVisibleHint = value;
			}
		}
	}
}
