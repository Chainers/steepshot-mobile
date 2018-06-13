using System;
using CheeseBind;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Steepshot.Base;
using Steepshot.Utils;
using Steepshot.Core.Utils;
using Steepshot.Core.Localization;
using Steepshot.Core;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class RegistrationActivity : BaseActivity
    {
        #pragma warning disable 0649, 4014
        [BindView(Resource.Id.steemit_btn)] private Button steemit;
        [BindView(Resource.Id.blocktrades_btn)] private Button blocktrades;
        [BindView(Resource.Id.steemcreate_btn)] private Button steemcreate;
        [BindView(Resource.Id.profile_login)] private TextView _viewTitle;
        [BindView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [BindView(Resource.Id.btn_settings)] private ImageButton _settings;
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        #pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_registration_alert);
            Cheeseknife.Bind(this);

            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBack;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;

            _viewTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.RegistrationWith);
            _viewTitle.Typeface = Style.Semibold;

            SetupView();
        }

        private void GoBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        private void SetupView()
        { 
            steemit.SetCompoundDrawablesWithIntrinsicBounds(SetupLogo(Resource.Drawable.ic_steem), null, null, null);
            blocktrades.SetCompoundDrawablesWithIntrinsicBounds(SetupLogo(Resource.Drawable.ic_blocktrade), null, null, null);
            steemcreate.SetCompoundDrawablesWithIntrinsicBounds(SetupLogo(Resource.Drawable.ic_steemcreate), null, null, null);

            steemit.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.RegisterThroughSteemit)} (free)";
            blocktrades.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.RegisterThroughBlocktrades);
            steemcreate.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.RegisterThroughSteemCreate);

            steemit.Click += (o, args) =>
            {
                OpenBrowser(Android.Net.Uri.Parse(Constants.SteemitRegUrl));
            };

            blocktrades.Click += (o, args) =>
            {
                OpenBrowser(Android.Net.Uri.Parse(Constants.BlocktradesRegUrl));
            };

            steemcreate.Click += (o, args) =>
            {
                OpenBrowser(Android.Net.Uri.Parse(Constants.SteemCreateRegUrl));
            };
        }

        private void OpenBrowser(Android.Net.Uri uri)
        { 
            var browserIntent = new Intent(Intent.ActionView, uri);
            StartActivity(browserIntent);
        }

        private BitmapDrawable SetupLogo(int drawable)
        {
            var logoSide = (int)BitmapUtils.DpToPixel(80, Resources);
            var originalImage = BitmapFactory.DecodeResource(Resources, drawable);
            var scaledBitmap = Bitmap.CreateScaledBitmap(originalImage, logoSide, logoSide, true);
            return new BitmapDrawable(Resources, scaledBitmap);
        }
    }
}
