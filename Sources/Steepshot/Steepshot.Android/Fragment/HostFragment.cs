﻿using Android.App;
using Android.Net;
using Android.OS;
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
                if (Fragment != null)
                    return Fragment.UserVisibleHint;
                else
                    return base.UserVisibleHint;
            }
            set
            {
                if (Fragment != null)
                    Fragment.UserVisibleHint = value;
                else
                    base.UserVisibleHint = value;
            }
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.HostLayout, container, false);

            if (Fragment != null)
                ReplaceFragment(Fragment, false);

            ChildFragmentManager.BackStackChanged -= BackStackChange;
            ChildFragmentManager.BackStackChanged += BackStackChange;

            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            if (Activity is BaseActivity activity)
            {
                var appLink = activity.Intent.GetStringExtra(BaseActivity.AppLinkingExtra);
                if (!string.IsNullOrEmpty(appLink))
                    activity.OpenUri(Uri.Parse(appLink));
                activity.Intent.RemoveExtra(BaseActivity.AppLinkingExtra);
            }
        }

        private void BackStackChange(object sender, System.EventArgs e)
        {
            if (IsPopped)
            {
                Fragment = (BaseFragment)ChildFragmentManager.Fragments[0];
                IsPopped = false;
            }
        }

        public void ReplaceFragment(BaseFragment fragment, bool addToBackstack)
        {
            var transaction = ChildFragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.child_fragment_container, fragment);
            Fragment = fragment;
            if (addToBackstack)
                transaction.AddToBackStack(null);

            transaction.Commit();
        }

        public static HostFragment NewInstance(BaseFragment fragment)
        {
            return new HostFragment { Fragment = fragment, _firstFragmentId = fragment.Id };
        }

        public void Clear()
        {
            IsPopped = true;
            ChildFragmentManager.PopBackStackImmediate(_firstFragmentId, (int)PopBackStackFlags.Inclusive);
        }
    }
}
