using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;

namespace Steepshot
{
	[Activity(NoHistory = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class PreSignInActivity : BaseActivity, PreSignInView
	{
		PreSignInPresenter presenter;

		[InjectView(Resource.Id.loading_spinner)]
		ProgressBar spinner;

		[InjectView(Resource.Id.input_username)]
		private EditText username;

		[InjectView(Resource.Id.network_switch)]
		private SwitchCompat switcher;

		[InjectView(Resource.Id.login_label)]
		private TextView loginLabel;

		[InjectView(Resource.Id.sign_up_label)]
		private TextView signupLabel;

		[InjectView(Resource.Id.steem_logo)]
		ImageView steem_logo;

		[InjectView(Resource.Id.golos_logo)]
		ImageView golos_logo;

		private string _newAccountNetwork;

		protected override void CreatePresenter()
		{
			presenter = new PreSignInPresenter(this);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.lyt_pre_sign_in);
			Cheeseknife.Inject(this);
#if DEBUG
			username.Text = "joseph.kalu";
#endif
			_newAccountNetwork = Intent.GetStringExtra("newNetwork");
			if (!string.IsNullOrEmpty(_newAccountNetwork))
			{
				switcher.Visibility = ViewStates.Gone;
				steem_logo.Visibility = ViewStates.Gone;
				golos_logo.Visibility = ViewStates.Gone;
				UserPrincipal.Instance.CurrentNetwork = _newAccountNetwork;

				BasePresenter.SwitchNetwork();
			}


			switcher.Checked = UserPrincipal.Instance.CurrentNetwork == Constants.Steem;
			switcher.CheckedChange += (sender, e) =>
			{
				UserPrincipal.Instance.CurrentNetwork = e.IsChecked ? Constants.Steem : Constants.Golos;
				BasePresenter.SwitchNetwork();
				SetLabelsText();
			};
			SetLabelsText();
		}

		protected override void OnDestroy()
		{
			if (!string.IsNullOrEmpty(_newAccountNetwork))
			{
				UserPrincipal.Instance.CurrentNetwork = _newAccountNetwork == Constants.Steem ? Constants.Golos : Constants.Steem;
				BasePresenter.SwitchNetwork();
			}
			base.OnDestroy();
		}

		[InjectOnClick(Resource.Id.sign_in_btn)]
		private async void SignInBtn_Click(object sender, System.EventArgs e)
		{
			var login = username.Text.ToLower();

			if (string.IsNullOrEmpty(login))
			{
				Toast.MakeText(this, "Invalid credentials", ToastLength.Short).Show();
				return;
			}

			spinner.Visibility = ViewStates.Visible;
			((AppCompatButton)sender).Visibility = ViewStates.Invisible;

			if (string.IsNullOrEmpty(login))
				return;

			var response = await presenter.GetAccountInfo(login);

			if (response != null)
			{
				if (response.Success)
				{
					_newAccountNetwork = null;
					var intent = new Intent(this, typeof(SignInActivity));
					intent.PutExtra("login", login);
					intent.PutExtra("avatar_url", response.Result.ProfileImage);
					StartActivity(intent);
				}
				else
				{
					ShowAlert(response.Errors[0]);
					spinner.Visibility = ViewStates.Invisible;
					((AppCompatButton)sender).Visibility = ViewStates.Visible;
				}
			}
			else
			{
				ShowAlert(Resource.String.error_connect_to_server);
				spinner.Visibility = ViewStates.Invisible;
				((AppCompatButton)sender).Visibility = ViewStates.Visible;
			}
		}

		private void SetLabelsText()
		{
			var currentNetwork = UserPrincipal.Instance.CurrentNetwork;
			loginLabel.Text = $"Log in with your {currentNetwork} Account";
			signupLabel.Text = $"Haven't {currentNetwork} account yet?";
		}
	}
}
