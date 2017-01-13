using System;
using Android.App;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Refractored.Controls;
using Steemix.Droid.Activity;
using Steemix.Droid.ViewModels;
using Square.Picasso;

namespace Steemix.Droid.Views
{
    [Activity]
    public class SettingsActivity : BaseActivity<SettingsViewModel>
    {
        [InjectView(Resource.Id.avatar)]
        private CircleImageView _avatar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_settings);
            Cheeseknife.Inject(this);
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