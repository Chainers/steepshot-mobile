using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Base;
using Steepshot.Utils;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using System.Linq;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Newtonsoft.Json;
using Steepshot.Core.Extensions;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateVisible | SoftInput.AdjustPan)]
    public sealed class PreSignInActivity : BaseActivityWithPresenter<PreSignInPresenter>
    {
#pragma warning disable 0649, 4014
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [BindView(Resource.Id.input_username)] private EditText _username;
        [BindView(Resource.Id.pre_sign_in_btn)] private Button _preSignInBtn;
        [BindView(Resource.Id.profile_login)] private TextView _viewTitle;
        [BindView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [BindView(Resource.Id.btn_settings)] private ImageButton _settings;
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [BindView(Resource.Id.root_layout)] private RelativeLayout _rootLayout;
#pragma warning restore 0649


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_pre_sign_in);
            Cheeseknife.Bind(this);
#if DEBUG
            var assetHelper = App.Container.GetAssetHelper();
            var di = assetHelper.GetDebugInfo();
            _username.Text = App.User.Chain == KnownChains.Golos
                ? di.GolosTestLogin
                : di.SteemTestLogin;
#endif

            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBack;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = App.Localization.GetText(LocalizationKeys.YourAccountName);

            _viewTitle.Typeface = Style.Semibold;
            _username.Typeface = Style.Regular;
            _preSignInBtn.Typeface = Style.Semibold;
            _preSignInBtn.Text = App.Localization.GetText(LocalizationKeys.NextStep);
            _preSignInBtn.Click += SignInBtn_Click;
            _rootLayout.Click += HideKeyboard;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            var currentUser = App.User.GetAllAccounts().FirstOrDefault();
            if (currentUser != null)
                App.MainChain = currentUser.Chain;
        }

        private void GoBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        private async void SignInBtn_Click(object sender, EventArgs e)
        {
            var login = _username.Text?.Trim().ToLower();

            if (string.IsNullOrEmpty(login))
            {
                this.ShowAlert(LocalizationKeys.EmptyLogin, ToastLength.Short);
                return;
            }

            _preSignInBtn.Text = string.Empty;
            _spinner.Visibility = ViewStates.Visible;

            var response = await Presenter.TryGetAccountInfoAsync(login);
            if (IsFinishing || IsDestroyed)
                return;

            if (response.IsSuccess)
            {
                var intent = new Intent(this, typeof(SignInActivity));
                intent.PutExtra(SignInActivity.LoginExtraPath, login);
                intent.PutExtra(SignInActivity.AccountInfoResponseExtraPath, JsonConvert.SerializeObject(response.Result));
                StartActivity(intent);
            }
            else
            {
                this.ShowAlert(response.Exception);
            }

            _spinner.Visibility = ViewStates.Invisible;
            _preSignInBtn.Text = App.Localization.GetText(LocalizationKeys.NextStep);
        }

        private void HideKeyboard(object sender, EventArgs e)
        {
            HideKeyboard();
        }
    }
}
