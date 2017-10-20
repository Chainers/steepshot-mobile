using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;

namespace Steepshot.Activity
{
    [Activity(NoHistory = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateVisible | SoftInput.AdjustPan)]
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
#pragma warning restore 0649

        private KnownChains _newChain;
        protected override void CreatePresenter()
        {
            _presenter = new PreSignInPresenter();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_pre_sign_in);
            Cheeseknife.Inject(this);
#if DEBUG
            _username.Text = "joseph.kalu";
#endif

            var font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            var semibold_font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");

            _backButton.Visibility = ViewStates.Visible;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = "Your account name";

            _viewTitle.Typeface = semibold_font;
            _username.Typeface = font;
            _preSignInBtn.Typeface = semibold_font;

            _newChain = (KnownChains)Intent.GetIntExtra("newChain", (int)KnownChains.None);
            if (_newChain != KnownChains.None)
            {
                BasePresenter.SwitchChain(_newChain);
            }
        }

        [InjectOnClick(Resource.Id.btn_back)]
        private void GoBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        [InjectOnClick(Resource.Id.pre_sign_in_btn)]
        private async void SignInBtn_Click(object sender, EventArgs e)
        {
            var login = _username.Text?.ToLower().Trim();

            if (string.IsNullOrEmpty(login))
            {
                ShowAlert(Localization.Errors.EmptyLogin, ToastLength.Short);
                return;
            }

            _preSignInBtn.Text = string.Empty;
            _spinner.Visibility = ViewStates.Visible;

            var response = await _presenter.TryGetAccountInfo(login);
            if (response != null && response.Success)
            {
                var intent = new Intent(this, typeof(SignInActivity));
                intent.PutExtra("login", login);
                intent.PutExtra("avatar_url", response.Result.ProfileImage);
                intent.PutExtra("newChain", (int)_newChain);
                _newChain = KnownChains.None;
                StartActivity(intent);
            }
            else
            {
                ShowAlert(response);
            }
            _spinner.Visibility = ViewStates.Invisible;
            _preSignInBtn.Text = "Next step";
        }

        protected override void OnDestroy()
        {

            if (_newChain != KnownChains.None)
            {
                BasePresenter.SwitchChain(_newChain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem);
            }
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }
    }
}
