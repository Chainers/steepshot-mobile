using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Autofac;
using Square.Picasso;
using Steepshot.Core.Authority;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.Fragment;
using Steepshot.Services;
using Steepshot.Utils;
using LruCache = Square.Picasso.LruCache;
using Steepshot.Core.Localization;
using Steepshot.Core.Sentry;

namespace Steepshot.Base
{
    public abstract class BaseActivity : AppCompatActivity
    {
        public const string AppLinkingExtra = "appLinkingExtra";
        protected HostFragment CurrentHostFragment;
        protected static LruCache Cache;
        public static Func<MotionEvent, bool> TouchEvent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            InitIoC(Assets);
            base.OnCreate(savedInstanceState);
            InitPicassoCache();
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            return (TouchEvent?.Invoke(ev) ?? false) || base.DispatchTouchEvent(ev);
        }

        public override View OnCreateView(View parent, string name, Context context, IAttributeSet attrs)
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Window.DecorView.SystemUiVisibility |= (StatusBarVisibility)SystemUiFlags.LightStatusBar;
                Window.SetStatusBarColor(Color.White);
            }
            return base.OnCreateView(parent, name, context, attrs);
        }

        private void InitPicassoCache()
        {
            if (Cache == null)
            {
                Cache = new LruCache(this);
                var d = new Picasso.Builder(this);
                d.MemoryCache(Cache);
                Picasso.SetSingletonInstance(d.Build());
            }
        }

        public static void InitIoC(AssetManager assetManagerssets)
        {
            if (AppSettings.Container == null)
            {
                var builder = new ContainerBuilder();
                var saverService = new SaverService();
                var dataProvider = new DataProvider(saverService);
                var appInfo = new AppInfo();
                var assetsHelper = new AssetsHelper(assetManagerssets);
                var connectionService = new ConnectionService();

                var localization = dataProvider.SelectLocalization("en-us") ?? assetsHelper.GetLocalization("en-us");
                var localizationManager = new LocalizationManager(localization);

                builder.RegisterInstance(assetsHelper).As<IAssetsHelper>().SingleInstance();
                builder.RegisterInstance(appInfo).As<IAppInfo>().SingleInstance();
                builder.RegisterInstance(saverService).As<ISaverService>().SingleInstance();
                builder.RegisterInstance(dataProvider).As<IDataProvider>().SingleInstance();
                builder.RegisterInstance(connectionService).As<IConnectionService>().SingleInstance();
                builder.RegisterInstance(connectionService).As<IConnectionService>().SingleInstance();
                builder.RegisterInstance(localizationManager).As<LocalizationManager>().SingleInstance();
                var configInfo = assetsHelper.GetConfigInfo();
                var reporterService = new ReporterService(appInfo, configInfo.RavenClientDsn);
                builder.RegisterInstance(reporterService).As<IReporterService>().SingleInstance();
                AppSettings.Container = builder.Build();
            }
        }

        public override void OnBackPressed()
        {
            if (CurrentHostFragment?.ChildFragmentManager?.Fragments.Count > 0)
            {
                if (CurrentHostFragment.ChildFragmentManager.Fragments?[CurrentHostFragment.ChildFragmentManager.Fragments.Count - 1] is BaseFragment currentFragment &&
                    (currentFragment.OnBackPressed() || CurrentHostFragment.HandleBackPressed(SupportFragmentManager)))
                    return;
            }

            base.OnBackPressed();
        }

        public virtual void OpenNewContentFragment(BaseFragment frag)
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
                    OpenNewContentFragment(new SinglePostFragment(appLink));
                else if (path.StartsWith("/@"))
                    OpenNewContentFragment(new ProfileFragment(appLink));
            }
        }
    }
}
