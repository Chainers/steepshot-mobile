using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
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
        [InjectView(Resource.Id.civ_avatar)]
        private CircleImageView _avatar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_settings);
            Cheeseknife.Inject(this);

            // TODO:KOA-COM: NotReadyYet
            var changeAvatar = FindViewById<AppCompatButton>(Resource.Id.dtn_change_avatar);
            changeAvatar.Visibility = ViewStates.Invisible;
            var deleteSteemixAccount = FindViewById<AppCompatButton>(Resource.Id.dtn_delete_steemix_account);
            deleteSteemixAccount.Visibility = ViewStates.Invisible;

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

        [InjectOnClick(Resource.Id.go_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Finish();
        }

        [InjectOnClick(Resource.Id.dtn_change_password)]
        public void ChangePasswordClick(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(ChangePasswordActivity));
            StartActivity(intent);
        }

        [InjectOnClick(Resource.Id.dtn_terms_of_service)]
        public void TermsOfServiceClick(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(TermsOfServiceActivity));
            StartActivity(intent);
        }
    }
}