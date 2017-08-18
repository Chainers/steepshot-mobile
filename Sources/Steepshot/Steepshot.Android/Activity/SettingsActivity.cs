using System;
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
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Presenters;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SettingsActivity : BaseActivity
    {
        SettingsPresenter _presenter;
#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.civ_avatar)] private CircleImageView _avatar;
        [InjectView(Resource.Id.steem_text)] private TextView _steemText;
        [InjectView(Resource.Id.golos_text)] private TextView _golosText;
        [InjectView(Resource.Id.golosView)] private RelativeLayout _golosView;
        [InjectView(Resource.Id.steemView)] private RelativeLayout _steemView;
        [InjectView(Resource.Id.add_account)] private AppCompatButton _addButton;
        [InjectView(Resource.Id.nsfw_switch)] private SwitchCompat _nsfwSwitcher;
        [InjectView(Resource.Id.low_switch)] private SwitchCompat _lowRatedSwitcher;
#pragma warning restore 0649
        UserInfo _steemAcc;
        UserInfo _golosAcc;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_settings);
            Cheeseknife.Inject(this);
            LoadAvatar();

            var accounts = Base.BasePresenter.User.GetAllAccounts();

            SetAddButton(accounts.Count);

            _steemAcc = accounts.FirstOrDefault(a => a.Chain == KnownChains.Steem);
            _golosAcc = accounts.FirstOrDefault(a => a.Chain == KnownChains.Golos);


            if (_steemAcc != null)
            {
                _steemText.Text = _steemAcc.Login;
                //Picasso.With(ApplicationContext).Load(steemAcc.).Into(stee);
            }
            else
                _steemView.Visibility = ViewStates.Gone;

            if (_golosAcc != null)
            {
                _golosText.Text = _golosAcc.Login;
                //Picasso.With(ApplicationContext).Load(steemAcc.).Into(stee);
            }
            else
                _golosView.Visibility = ViewStates.Gone;

            _nsfwSwitcher.CheckedChange += (sender, e) =>
            {
                Base.BasePresenter.User.IsNsfw = _nsfwSwitcher.Checked;
            };

            _lowRatedSwitcher.CheckedChange += (sender, e) =>
            {
                Base.BasePresenter.User.IsLowRated = _lowRatedSwitcher.Checked;
            };

            HighlightView();
            _nsfwSwitcher.Checked = Base.BasePresenter.User.IsNsfw;
            _lowRatedSwitcher.Checked = Base.BasePresenter.User.IsLowRated;
        }

        private async void LoadAvatar()
        {
            var info = await _presenter.GetUserInfo();
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

        [InjectOnClick(Resource.Id.dtn_terms_of_service)]
        public void TermsOfServiceClick(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse("https://steepshot.org/terms-of-service");
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }

        [InjectOnClick(Resource.Id.add_account)]
        public void AddAccountClick(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(PreSignInActivity));
            intent.PutExtra("newChain", (int)(Base.BasePresenter.Chain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem));
            StartActivity(intent);
        }

        [InjectOnClick(Resource.Id.golosView)]
        public void GolosViewClick(object sender, EventArgs e)
        {
            SwitchChain(_golosAcc);
        }

        [InjectOnClick(Resource.Id.steemView)]
        public void SteemViewClick(object sender, EventArgs e)
        {
            SwitchChain(_steemAcc);
        }

        [InjectOnClick(Resource.Id.remove_steem)]
        public void RemoveSteem(object sender, EventArgs e)
        {
            Base.BasePresenter.User.Delete(_steemAcc);
            _steemView.Visibility = ViewStates.Gone;
            RemoveChain(KnownChains.Steem);
        }

        [InjectOnClick(Resource.Id.remove_golos)]
        public void RemoveGolos(object sender, EventArgs e)
        {
            Base.BasePresenter.User.Delete(_golosAcc);
            _golosView.Visibility = ViewStates.Gone;
            RemoveChain(KnownChains.Golos);
        }

        private void SwitchChain(UserInfo user)
        {
            if (Base.BasePresenter.Chain != user.Chain)
            {
                Base.BasePresenter.SwitchChain(user);
                Intent i = new Intent(ApplicationContext, typeof(RootActivity));
                i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                StartActivity(i);
            }
        }

        private void RemoveChain(KnownChains chain)
        {
            //_presenter.Logout();
            var accounts = Base.BasePresenter.User.GetAllAccounts();
            if (accounts.Count == 0)
            {
                Intent i = new Intent(ApplicationContext, typeof(GuestActivity));
                i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                StartActivity(i);
                Finish();
            }
            else
            {
                if (Base.BasePresenter.Chain == chain)
                {
                    Base.BasePresenter.SwitchChain(chain == KnownChains.Steem ? _golosAcc : _steemAcc);
                    Intent i = new Intent(ApplicationContext, typeof(RootActivity));
                    i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    StartActivity(i);
                    Finish();
                }
                else
                {
                    HighlightView();
                    SetAddButton(accounts.Count);
                }
            }
        }

        private void HighlightView()
        {
            if (Base.BasePresenter.Chain == KnownChains.Steem)
                _steemView.SetBackgroundColor(Color.Cyan);
            else
                _golosView.SetBackgroundColor(Color.Cyan);
        }

        private void SetAddButton(int accountsCount)
        {
            if (accountsCount == 2)
                _addButton.Visibility = ViewStates.Gone;
        }

        protected override void CreatePresenter()
        {
            _presenter = new SettingsPresenter();
        }
    }
}