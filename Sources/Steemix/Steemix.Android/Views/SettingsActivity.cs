using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.Activity;
using Steemix.Droid.ViewModels;
using Square.Picasso;

namespace Steemix.Droid.Views
{
    [Activity(NoHistory = true)]
    public class SettingsActivity : BaseActivity<SettingsViewModel>
    {
        [InjectView(Resource.Id.avatar)]
        private ImageView _avatar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_settings);
            Cheeseknife.Inject(this);
            //_avatar = FindViewById<CircleImageView>(Resource.Id.avatar);
            LoadAvatar();
        }

        private async void LoadAvatar()
        {
            var info = await ViewModel.GetUserInfo();
            if (!string.IsNullOrEmpty(info.ImageUrl))
            {
                Picasso.With(ApplicationContext).Load(info.ImageUrl).Into(_avatar);
            }
        }

        [InjectOnClick(Resource.Id.settings)]
        public void OnSettingsClick(object sender, EventArgs e)
        {
            Finish();
        }
    }
}