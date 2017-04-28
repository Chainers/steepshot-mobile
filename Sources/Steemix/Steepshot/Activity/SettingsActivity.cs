using System;
using Android.App;
using Android.Content;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Refractored.Controls;
using Square.Picasso;

namespace Steepshot
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SettingsActivity : BaseActivity, SettingsView
    {
		SettingsPresenter presenter;

        [InjectView(Resource.Id.civ_avatar)]
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
			var info = await presenter.GetUserInfo();
            if (info.Success && !string.IsNullOrEmpty(info.Result.ProfileImage))
            {
                Picasso.With(ApplicationContext).Load(info.Result.ProfileImage).Into(_avatar);
            }
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            OnBackPressed();
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

		protected override void CreatePresenter()
		{
			presenter = new SettingsPresenter(this);
		}
	}
}