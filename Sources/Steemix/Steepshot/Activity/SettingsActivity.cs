using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
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

		[InjectView(Resource.Id.steem_text)]
		private TextView steem_text;

		[InjectView(Resource.Id.golos_text)]
		private TextView golos_text;

		[InjectView(Resource.Id.golosView)]
		private RelativeLayout golosView;

		[InjectView(Resource.Id.steemView)]
		private RelativeLayout steemView;

		[InjectView(Resource.Id.add_account)]
		private AppCompatButton addButton;

		UserInfo steemAcc;
		UserInfo golosAcc;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_settings);
            Cheeseknife.Inject(this);
            LoadAvatar();

			var accounts = UserPrincipal.Instance.GetAllAccounts();

            SetAddButton(accounts.Count);

			steemAcc = accounts.FirstOrDefault(a => a.Network == Constants.Steem);
			golosAcc = accounts.FirstOrDefault(a => a.Network == Constants.Golos);


			if (steemAcc != null)
			{
				steem_text.Text = steemAcc.Login;
				//Picasso.With(ApplicationContext).Load(steemAcc.).Into(stee);
			}
			else
				steemView.Visibility = ViewStates.Gone;


			if (golosAcc != null)
			{
				golos_text.Text = golosAcc.Login;
				//Picasso.With(ApplicationContext).Load(steemAcc.).Into(stee);
			}
			else
				golosView.Visibility = ViewStates.Gone;

			HighlightView();
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

		[InjectOnClick(Resource.Id.add_account)]
		public void AddAccountClick(object sender, EventArgs e)
		{
			Intent intent = new Intent(this, typeof(PreSignInActivity));
			intent.PutExtra("newNetwork", UserPrincipal.Instance.CurrentNetwork == Constants.Steem ? Constants.Golos : Constants.Steem);
			StartActivity(intent);
		}

		[InjectOnClick(Resource.Id.golosView)]
		public void GolosViewClick(object sender, EventArgs e)
		{
			SwitchNetwork(Constants.Golos);
		}

		[InjectOnClick(Resource.Id.steemView)]
		public void SteemViewClick(object sender, EventArgs e)
		{
			SwitchNetwork(Constants.Steem);
		}

		[InjectOnClick(Resource.Id.remove_steem)]
		public void RemoveSteem(object sender, EventArgs e)
		{
			UserPrincipal.Instance.DeleteUser(steemAcc);
			steemView.Visibility = ViewStates.Gone;
			RemoveNetwork(Constants.Steem);
		}

		[InjectOnClick(Resource.Id.remove_golos)]
		public void RemoveGolos(object sender, EventArgs e)
		{
			UserPrincipal.Instance.DeleteUser(golosAcc);
			golosView.Visibility = ViewStates.Gone;
			RemoveNetwork(Constants.Golos);
		}

		private void SwitchNetwork(string network)
		{
			if (UserPrincipal.Instance.CurrentNetwork != network)
			{
				UserPrincipal.Instance.ClearUser();
				UserPrincipal.Instance.CurrentNetwork = network;
				BasePresenter.SwitchNetwork();
				Intent i = new Intent(ApplicationContext, typeof(RootActivity));
				i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
				StartActivity(i);
			}
		}

		private void RemoveNetwork(string network)
		{
			presenter.Logout();
			var accounts = UserPrincipal.Instance.GetAllAccounts();
			if (accounts.Count == 0)
			{
				UserPrincipal.Instance.ClearUser();
				Intent i = new Intent(ApplicationContext, typeof(GuestActivity));
				i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
				StartActivity(i);
                Finish();
			}
			else 
			{
				var shouldRedirect = UserPrincipal.Instance.CurrentNetwork == network;
				UserPrincipal.Instance.CurrentNetwork = network == Constants.Steem? Constants.Golos : Constants.Steem;
				if (shouldRedirect)
				{
					Intent i = new Intent(ApplicationContext, typeof(RootActivity));
					i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
					StartActivity(i);
					Finish();
				}
				else
				{
					HighlightView();
					BasePresenter.SwitchNetwork();
					SetAddButton(accounts.Count);
				}
			}
		}

		private void HighlightView()
		{
			if (UserPrincipal.Instance.CurrentNetwork == Constants.Steem)
				steemView.SetBackgroundColor(Color.Cyan);
			else
				golosView.SetBackgroundColor(Color.Cyan);
		}

		private void SetAddButton(int accountsCount)
		{
			if (accountsCount == 2)
				addButton.Visibility = ViewStates.Gone;
		}

		protected override void CreatePresenter()
		{
			presenter = new SettingsPresenter(this);
		}
	}
}