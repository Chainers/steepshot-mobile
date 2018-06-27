using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public sealed class ActiveSignInActivity : BaseSignInActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Username = AppSettings.User.Login;
            AccountInfoResponse = AppSettings.User.AccountInfo;
            ProfileImageUrl = AccountInfoResponse.Metadata?.Profile?.ProfileImage;
            base.OnCreate(savedInstanceState);

            _viewTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ActivePasswordViewTitleText);
        }

        protected override void SignIn(object sender, EventArgs e)
        {
            var appCompatButton = (AppCompatButton)sender;

            var pass = _password?.Text;

            if (string.IsNullOrEmpty(pass))
            {
                this.ShowAlert(LocalizationKeys.EmptyPostingKey, ToastLength.Short);
                return;
            }

            _spinner.Visibility = ViewStates.Visible;
            appCompatButton.Text = string.Empty;
            appCompatButton.Enabled = false;

            var isvalid = KeyHelper.ValidatePrivateKey(pass, AccountInfoResponse.PublicActiveKeys);
            if (isvalid)
            {
                AppSettings.User.AddActiveKey(pass);
                Finish();
            }
            else
            {
                this.ShowAlert(LocalizationKeys.WrongPrivateActimeKey);
            }

            appCompatButton.Enabled = true;
            appCompatButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.EnterAccountText);
            _spinner.Visibility = ViewStates.Invisible;
        }
    }
}