﻿using Android.App;
using Android.OS;
using Steepshot.Base;
using Steepshot.Fragment;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class GuestActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var fragmentTransaction = SupportFragmentManager.BeginTransaction();
            CurrentHostFragment = HostFragment.NewInstance(new PreSearchFragment());
            fragmentTransaction.Add(Android.Resource.Id.Content, CurrentHostFragment);
            fragmentTransaction.Commit();
        }
    }
}
