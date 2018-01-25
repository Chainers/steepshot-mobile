using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Views;
using Com.Lilarcor.Cheeseknife;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Fragment;
using Steepshot.Interfaces;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(Label = Core.Constants.Steepshot, ScreenOrientation = ScreenOrientation.Portrait)]
    public sealed class RootActivity : BaseActivityWithPresenter<UserProfilePresenter>, IClearable
    {
        private Adapter.PagerAdapter _adapter;
        private TabLayout.Tab _prevTab;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.view_pager)] private CustomViewPager _viewPager;
        [InjectView(Resource.Id.tab_layout)] public TabLayout _tabLayout;
#pragma warning restore 0649


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (BasePresenter.User.IsAuthenticated && !BasePresenter.User.IsNeedRewards)
                BasePresenter.User.IsNeedRewards = true; // for android users set true by default

            SetContentView(Resource.Layout.lyt_tab_host);
            Cheeseknife.Inject(this);
            _adapter = new Adapter.PagerAdapter(SupportFragmentManager);
            _viewPager.Adapter = _adapter;
            InitTabs();

            _tabLayout.TabSelected += OnTabLayoutOnTabSelected;
        }

        public override void OpenNewContentFragment(BaseFragment frag)
        {
            CurrentHostFragment = _adapter.GetItem(_viewPager.CurrentItem) as HostFragment;
            base.OpenNewContentFragment(frag);
        }

        public override void OnBackPressed()
        {
            CurrentHostFragment = _adapter.GetItem(_viewPager.CurrentItem) as HostFragment;
            if (CurrentHostFragment != null)
            {
                var fragments = CurrentHostFragment.ChildFragmentManager.Fragments;
                if (fragments[fragments.Count - 1] is ICanOpenPost fragment)
                    if (fragment.ClosePost())
                        return;
            }

            if (CurrentHostFragment == null || !CurrentHostFragment.HandleBackPressed(SupportFragmentManager))
                MinimizeApp();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (BasePresenter.ProfileUpdateType != ProfileUpdateType.None)
            {
                SelectTab(_adapter.Count - 1);
            }
        }

        private void OnTabLayoutOnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            if (e.Tab.Position == 2)
            {
                if (PermissionChecker.CheckSelfPermission(this, Android.Manifest.Permission.Camera) == (int)Permission.Granted
                    && PermissionChecker.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
                {
                    _prevTab.Select();
                    var intent = new Intent(this, typeof(CameraActivity));
                    StartActivity(intent);
                }
                else
                {
                    //Replace for Permission request
                    this.ShowAlert("Check your app permissions");
                }
            }
            else
            {
                SelectTab(e.Tab.Position);
                _prevTab = e.Tab;
                BasePresenter.User.SelectedTab = e.Tab.Position;
            }
        }

        private void InitTabs()
        {
            for (var i = 0; i < _adapter.TabIconsInactive.Length; i++)
            {
                var tab = _tabLayout.NewTab();
                if (i == 0)
                    _prevTab = tab;
                _tabLayout.AddTab(tab);
                tab.SetIcon(ContextCompat.GetDrawable(this, _adapter.TabIconsInactive[i]));
            }
            SelectTab(BasePresenter.User.SelectedTab);
            _viewPager.OffscreenPageLimit = _adapter.Count - 1;
        }

        public void SelectTab(int position)
        {
            var tab = _tabLayout.GetTabAt(position);
            tab.Select();
            OnTabSelected(position);
        }

        public void SelectTabWithClearing(int position)
        {
            SelectTab(position);
            var hostFragment = _adapter.GetItem(position) as HostFragment;
            hostFragment?.Clear();
        }

        private void OnTabSelected(int position)
        {
            _viewPager.SetCurrentItem(position, false);
            for (var i = 0; i < _tabLayout.TabCount - 1; i++)
            {
                var tab = _tabLayout.GetTabAt(i);
                tab?.SetIcon(i == position
                             ? ContextCompat.GetDrawable(this, _adapter.TabIconsActive[i])
                             : ContextCompat.GetDrawable(this, _adapter.TabIconsInactive[i]));
            }

            SetProfileChart(_tabLayout.LayoutParameters.Height);
            TryUpdateProfile();
        }

        public async Task TryUpdateProfile()
        {
            do
            {
                var error = await Presenter.TryGetUserInfo(BasePresenter.User.Login);
                if (IsDestroyed)
                    return;

                if (error == null || error is TaskCanceledError)
                {
                    SetProfileChart(_tabLayout.LayoutParameters.Height);
                    break;
                }

                this.ShowAlert(error);
                await Task.Delay(5000);
                if (IsDestroyed)
                    return;

            } while (true);
        }

        private void SetProfileChart(int size)
        {
            var isEmpty = Presenter.UserProfileResponse == null;
            var votingPowerFrame = new VotingPowerFrame(this)
            {
                VotingPower = isEmpty ? 0 : (float)Presenter.UserProfileResponse.VotingPower,
                VotingPowerWidth = BitmapUtils.DpToPixel(3, Resources)
            };
            var padding = (int)BitmapUtils.DpToPixel(7, Resources);
            votingPowerFrame.Layout(0, 0, size, size);
            var avatar = new CircleImageView(this);
            avatar.Layout(padding, padding, size - padding, size - padding);
            avatar.SetImageResource(Resource.Drawable.ic_holder);
            votingPowerFrame.AddView(avatar);

            var profileTab = _tabLayout.GetTabAt(_tabLayout.TabCount - 1);
            if (!isEmpty)
                Picasso.With(this).Load(Presenter.UserProfileResponse.ProfileImage).NoFade().Resize(size, size)
                    .Placeholder(Resource.Drawable.ic_holder).Into(avatar,
                        () => { profileTab?.SetIcon(BitmapUtils.GetViewDrawable(votingPowerFrame)); }, null);
            else
                profileTab?.SetIcon(BitmapUtils.GetViewDrawable(votingPowerFrame));
        }
    }
}
