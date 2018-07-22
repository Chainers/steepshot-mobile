using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public sealed class SignInActivity : BaseSignInActivity
    {
        public const string LoginExtraPath = "login";
        public const string AccountInfoResponseExtraPath = "account_info_response";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Username = Intent.GetStringExtra(LoginExtraPath);
            AccountInfoResponse = JsonConvert.DeserializeObject<AccountInfoResponse>(Intent.GetStringExtra(AccountInfoResponseExtraPath));
            ProfileImageUrl = AccountInfoResponse.Metadata?.Profile?.ProfileImage;
            base.OnCreate(savedInstanceState);
        }

        protected override void SignIn(object sender, EventArgs e)
        {
            var appCompatButton = (AppCompatButton)sender;

            var login = Username;
            var pass = _password?.Text;

            if (string.IsNullOrEmpty(login))
            {
                this.ShowAlert(LocalizationKeys.EmptyLogin, ToastLength.Short);
                return;
            }

            if (string.IsNullOrEmpty(pass))
            {
                this.ShowAlert(LocalizationKeys.EmptyPostingKey, ToastLength.Short);
                return;
            }

            _spinner.Visibility = ViewStates.Visible;
            appCompatButton.Text = string.Empty;
            appCompatButton.Enabled = false;

            var isvalid = KeyHelper.ValidatePrivateKey(pass, AccountInfoResponse.PublicPostingKeys);
            if (isvalid)
            {
                AppSettings.User.AddAndSwitchUser(login, pass, AccountInfoResponse, BasePresenter.Chain);
                var intent = new Intent(this, typeof(RootActivity));
                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                StartActivity(intent);
            }
            else
            {
                this.ShowAlert(LocalizationKeys.WrongPrivatePostingKey);
            }

            appCompatButton.Enabled = true;
            appCompatButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.EnterAccountText);
            _spinner.Visibility = ViewStates.Invisible;
        }
    }
}
