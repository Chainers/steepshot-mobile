﻿using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Refractored.Controls;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public sealed class ActiveSignInActivity : BaseSignInActivity
    {
        [BindView(Resource.Id.privacy_politic)] private TextView _privacyPolitic;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Username = AppSettings.User.Login;
            AccountInfoResponse = AppSettings.User.AccountInfo;
            ProfileImageUrl = AccountInfoResponse.Metadata?.Profile?.ProfileImage;
            base.OnCreate(savedInstanceState);

            _viewTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ActivePasswordViewTitleText);
            _privacyPolitic.Typeface = Style.Light;
            _privacyPolitic.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ActiveKeyPrivacy);
            _privacyPolitic.SetTextColor(Color.LightGray);
            _privacyPolitic.Visibility = ViewStates.Visible;
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
                SetResult(Result.Ok);
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

        protected override void GoBack(object sender, EventArgs e)
        {
            SetResult(Result.Canceled);
            base.GoBack(sender, e);
        }
    }
}