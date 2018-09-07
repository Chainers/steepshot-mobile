using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Steepshot.Core.Utils;
using Steepshot.Fragment;

namespace Steepshot.Base
{
    public abstract class BaseActivity : AppCompatActivity
    {
        public const string AppLinkingExtra = "appLinkingExtra";
        public static int CommonPermissionsRequestCode = 888;
        protected virtual HostFragment CurrentHostFragment { get; set; }
        public static event Func<MotionEvent, bool> TouchEvent;

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            return (TouchEvent?.Invoke(ev) ?? false) || base.DispatchTouchEvent(ev);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Window.DecorView.SystemUiVisibility |= (StatusBarVisibility)SystemUiFlags.LightStatusBar;
                Window.SetStatusBarColor(Color.White);
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.SetStatusBarColor(Color.Black);
            }
        }

        public override void OnBackPressed()
        {
            var fragments = CurrentHostFragment?.ChildFragmentManager?.Fragments;
            if (fragments?.Count > 0)
            {
                if (fragments.Last() is BaseFragment currentFragment &&
                    (currentFragment.OnBackPressed() || CurrentHostFragment.HandleBackPressed(SupportFragmentManager)))
                    return;
            }

            base.OnBackPressed();
        }

        public void OpenNewContentFragment(BaseFragment frag)
        {
            CurrentHostFragment?.ReplaceFragment(frag, true);
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            if (level == TrimMemory.Complete)
            {
                if (AppSettings.Container != null)
                {
                    AppSettings.Container.Dispose();
                    AppSettings.Container = null;
                }
            }

            GC.Collect();
            GC.Collect(GC.MaxGeneration);
            base.OnTrimMemory(level);
        }

        public void HideKeyboard()
        {
            if (CurrentFocus != null)
            {
                var imm = GetSystemService(InputMethodService) as InputMethodManager;
                imm?.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);
            }
        }

        public void OpenKeyboard(View view)
        {
            var imm = GetSystemService(InputMethodService) as InputMethodManager;
            imm?.ShowSoftInput(view, ShowFlags.Implicit);
        }

        protected void MinimizeApp()
        {
            var intent = new Intent(Intent.ActionMain);
            intent.AddCategory(Intent.CategoryHome);
            intent.SetFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            Finish();
        }

        public void HandleLink(Intent intent)
        {
            var path = intent.GetStringExtra(AppLinkingExtra);
            intent.RemoveExtra(AppLinkingExtra);

            if (string.IsNullOrEmpty(path))
                return;

            int index = path.IndexOf("@", StringComparison.Ordinal) + 1;

            if (index < path.Length)
            {
                var appLink = path.Substring(index, path.Length - index);

                if (path.StartsWith("/post"))
                    OpenNewContentFragment(new PostViewFragment(appLink));
                else if (path.StartsWith("/@"))
                    OpenNewContentFragment(new ProfileFragment(appLink));
            }
        }

        public bool RequestPermissions(int requestCode, params string[] permissions)
        {
            var missingPermissions = new List<string>();
            foreach (var permission in permissions)
            {
                if (ContextCompat.CheckSelfPermission(this, permission) != Permission.Granted)
                    missingPermissions.Add(permission);
            }
            if (missingPermissions.Any())
            {
                ActivityCompat.RequestPermissions(this, missingPermissions.ToArray(), requestCode);
                return true;
            }
            return false;
        }
    }
}
