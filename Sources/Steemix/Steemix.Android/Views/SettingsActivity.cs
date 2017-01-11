using Android.App;
using Android.OS;
using Android.Widget;
using Refractored.Controls;
using Steemix.Droid.Activity;
using Steemix.Droid.ViewModels;
using Square.Picasso;

namespace Steemix.Droid.Views
{
    [Activity(NoHistory = true)]
    public class SettingsActivity : BaseActivity<SettingsViewModel>
    {
        private ImageView Avatar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var info = ViewModel.GetUserInfo();
            info.Start();

            SetContentView(Resource.Layout.lyt_settings);
            Avatar = FindViewById<CircleImageView>(Resource.Id.avatar);
            LoadAvatar();
        }

        private async void LoadAvatar()
        {
            var info = await ViewModel.GetUserInfo();
            if (!string.IsNullOrEmpty(info.ImageUrl))
            {
                Picasso.With(this.ApplicationContext).Load(info.ImageUrl).Into(Avatar);
            }
        }
    }
}