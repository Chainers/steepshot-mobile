using Android.App;
using Steepshot.Base;

namespace Steepshot.Fragment
{
    public sealed class HostFragment : BackStackFragment
    {
        private int _firstFragmentId;

        public override bool UserVisibleHint
        {
            get
            {
                if (_fragment != null)
                    return _fragment.UserVisibleHint;
                else
                    return base.UserVisibleHint;
            }
            set
            {
                if (_fragment != null)
                    _fragment.UserVisibleHint = value;
                else
                    base.UserVisibleHint = value;
            }
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.HostLayout, container, false);

            if (_fragment != null)
                ReplaceFragment(_fragment, false);

            ChildFragmentManager.BackStackChanged -= BackStackChange;
            ChildFragmentManager.BackStackChanged += BackStackChange;

            return view;
        }

        private void BackStackChange(object sender, System.EventArgs e)
        {
            if (_isPopped)
            {
                _fragment = (BaseFragment)ChildFragmentManager.Fragments[0];
                _isPopped = false;
            }
        }

        public void ReplaceFragment(BaseFragment fragment, bool addToBackstack)
        {
            var transaction = ChildFragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.child_fragment_container, fragment);
            _fragment = fragment;
            if (addToBackstack)
                transaction.AddToBackStack(null);

            transaction.Commit();
        }

        public static HostFragment NewInstance(BaseFragment fragment)
        {
            return new HostFragment { _fragment = fragment, _firstFragmentId = fragment.Id };
        }

        public void Clear()
        {
            ChildFragmentManager.PopBackStackImmediate(_firstFragmentId, (int)PopBackStackFlags.Inclusive);
        } 
    }
}
