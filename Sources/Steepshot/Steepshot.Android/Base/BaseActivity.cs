using System;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views.InputMethods;
using Autofac;
using Square.Picasso;
using Steepshot.Core.Authority;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.Fragment;
using Steepshot.Services;

namespace Steepshot.Base
{
    public abstract class BaseActivity : AppCompatActivity
    {
        protected HostFragment CurrentHostFragment;
        public static LruCache Cache;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (AppSettings.Container == null)
                Construct();
        }

        private void Construct()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new AppInfo()).As<IAppInfo>().SingleInstance();
            builder.RegisterType<DataProvider>().As<IDataProvider>().SingleInstance();
            builder.RegisterInstance(new SaverService()).As<ISaverService>().SingleInstance();
            builder.RegisterInstance(new ConnectionService()).As<IConnectionService>().SingleInstance();
#if DEBUG
            builder.RegisterType<StubReporterService>().As<IReporterService>().SingleInstance();
#else
            builder.RegisterType<ReporterService>().As<IReporterService>().SingleInstance();
#endif

            var d = new Picasso.Builder(this);
            Cache = new LruCache(this);
            d.MemoryCache(Cache);
            Picasso.SetSingletonInstance(d.Build());

            AppSettings.Container = builder.Build();
        }

        public override void OnBackPressed()
        {
            if (CurrentHostFragment == null || !CurrentHostFragment.HandleBackPressed(SupportFragmentManager))
                base.OnBackPressed();
        }

        public virtual void OpenNewContentFragment(Android.Support.V4.App.Fragment frag)
        {
            CurrentHostFragment?.ReplaceFragment(frag, true);
        }

        public override void OnTrimMemory(Android.Content.TrimMemory level)
        {
            if (level == TrimMemory.Complete)
            {
                if(AppSettings.Container != null)
                {
                    AppSettings.Container.Dispose();
                    AppSettings.Container = null;
                }
            }

            GC.Collect();
            base.OnTrimMemory(level);
        }

        protected void HideKeyboard()
        {
            var imm = GetSystemService(InputMethodService) as InputMethodManager;
            imm?.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);
        }

        protected void MinimizeApp()
        {
            var intent = new Intent(Intent.ActionMain);
            intent.AddCategory(Intent.CategoryHome);
            intent.SetFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            Finish();
        }
    }
}
