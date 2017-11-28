using Android.App;
using Steepshot.Base;

namespace Steepshot.Fragment
{
    public sealed class HostFragment : BackStackFragment
    {
        private BaseFragment _fragment;
        private string _firstFragmentName;

        public override bool UserVisibleHint
        {
            get => base.UserVisibleHint;
            set
            {
                if (_fragment is BaseFragment bf)
                    bf.CustomUserVisibleHint = value;
                base.UserVisibleHint = value;
            }
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.HostLayout, container, false);

            if (_fragment != null)
                ReplaceFragment(_fragment, false);

            return view;
        }

        public void ReplaceFragment(BaseFragment fragment, bool addToBackstack)
        {
            var transaction = ChildFragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.child_fragment_container, fragment);

            if (addToBackstack)
                transaction.AddToBackStack(fragment.Name);

            transaction.Commit();
        }

        public static HostFragment NewInstance(BaseFragment fragment)
        {
            return new HostFragment { _fragment = fragment, _firstFragmentName = fragment.Name };
        }

        public void Clear()
        {
            ChildFragmentManager.PopBackStackImmediate(_firstFragmentName, (int)PopBackStackFlags.Inclusive);
        }
    }
}
