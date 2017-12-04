using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;
using Steepshot.Utils;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using System.Linq;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateVisible | SoftInput.AdjustPan)]
    public class PreSignInActivity : BaseActivityWithPresenter<PreSignInPresenter>
    {
#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [InjectView(Resource.Id.input_username)] private EditText _username;
        [InjectView(Resource.Id.pre_sign_in_btn)] private Button _preSignInBtn;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.root_layout)] private RelativeLayout _rootLayout;
#pragma warning restore 0649


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_pre_sign_in);
            Cheeseknife.Inject(this);
#if DEBUG
            var di = AssetsHelper.GetDebugInfo(Assets);
            _username.Text = BasePresenter.Chain == KnownChains.Golos
                ? di.GolosTestLogin
                : di.SteemTestLogin;
#endif

            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBack;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = Localization.Messages.YourAccountName;

            _viewTitle.Typeface = Style.Semibold;
            _username.Typeface = Style.Regular;
            _preSignInBtn.Typeface = Style.Semibold;
            _preSignInBtn.Text = Localization.Messages.NextStep;
            _preSignInBtn.Click += SignInBtn_Click;
            _rootLayout.Click += HideKeyboard;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }
        
        public override async void OnBackPressed()
        {
            base.OnBackPressed();
            var currentUser = BasePresenter.User.GetAllAccounts().FirstOrDefault();
            if (currentUser != null)
                await BasePresenter.SwitchChain(currentUser.Chain);
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
                this.ShowAlert(Localization.Errors.EmptyLogin, ToastLength.Short);
                return;
            }

            _preSignInBtn.Text = string.Empty;
            _spinner.Visibility = ViewStates.Visible;

            var response = await Presenter.TryGetAccountInfo(login);
            if (IsFinishing || IsDestroyed)
                return;

            if (response != null && response.Success)
            {
                var intent = new Intent(this, typeof(SignInActivity));
                intent.PutExtra(SignInActivity.LoginExtraPath, login);
                intent.PutExtra(SignInActivity.AvatarUrlExtraPath, response.Result.ProfileImage);
                StartActivity(intent);
            }
            else
            {
                this.ShowAlert(response);
            }

            _spinner.Visibility = ViewStates.Invisible;
            _preSignInBtn.Text = Localization.Messages.NextStep;
        }

        private void HideKeyboard(object sender, EventArgs e)
        {
            HideKeyboard();
        }
    }
}
