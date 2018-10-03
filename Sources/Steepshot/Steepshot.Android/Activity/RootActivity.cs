using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Com.OneSignal;
using CheeseBind;
using Newtonsoft.Json;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.CustomViews;
using Steepshot.Fragment;
using Steepshot.Interfaces;
using Steepshot.Utils;
using Steepshot.Core.Extensions;
using Steepshot.Core.Utils;
using System.Linq;
using Android;
using Android.Runtime;
using Newtonsoft.Json.Linq;
using Steepshot.Core;
using WebSocketSharp;

namespace Steepshot.Activity
{
    [Activity(Label = Constants.Steepshot, ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
    public sealed class RootActivity : BaseActivityWithPresenter<UserProfilePresenter>, IClearable
    {
        public const string NotificationData = "NotificationData";
        public const string SharingPhotoData = "SharingPhotoData";
        public const string PostCreateResumeExtra = "PostCreateResumeExtra";
        protected override HostFragment CurrentHostFragment => CurrentHostFragment = _adapter.GetItem(_viewPager.CurrentItem) as HostFragment;
        private Adapter.PagerAdapter _adapter;
        private TabLayout.Tab _prevTab;
        private int _tabHeight;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.view_pager)] private CustomViewPager _viewPager;
        [BindView(Resource.Id.tab_layout)] public TabLayout TabLayout;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_tab_host);
            Cheeseknife.Bind(this);

            _tabHeight = (int)BitmapUtils.DpToPixel(30, Resources);
            _adapter = new Adapter.PagerAdapter(SupportFragmentManager);
            _viewPager.Adapter = _adapter;
            InitTabs();

            TabLayout.TabSelected += OnTabLayoutOnTabSelected;
            TabLayout.TabReselected += OnTabLayoutOnTabReselected;

            if (AppSettings.User.HasPostingPermission)
                OneSignal.Current.IdsAvailable(OneSignalCallback);

            CheckForNewFeatures();
        }

        private async void OneSignalCallback(string playerId, string pushToken)
        {
            OneSignal.Current.DeleteTags(new List<string> { "username", "player_id" });
            OneSignal.Current.SendTag("username", AppSettings.User.Login);
            OneSignal.Current.SendTag("player_id", playerId);

            if (AppSettings.User.IsFirstRun || string.IsNullOrEmpty(AppSettings.User.PushesPlayerId) || !AppSettings.User.PushesPlayerId.Equals(playerId))
            {
                var model = new PushNotificationsModel(AppSettings.User.UserInfo, playerId, true)
                {
                    Subscriptions = PushSettings.All.FlagToStringList()
                };
                
                var response = await Presenter.TrySubscribeForPushesAsync(model).ConfigureAwait(false);

                if (response.IsSuccess)
                {
                    AppSettings.User.PushesPlayerId = playerId;
                    AppSettings.User.PushSettings = PushSettings.All;
                }
            }
        }

        public void HandleNotification(Intent intent)
        {
            var jsonData = intent.GetStringExtra(NotificationData);
            intent.RemoveExtra(NotificationData);
            if (!jsonData.IsNullOrEmpty())
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
                    if (data != null)
                    {
                        var type = data["type"];
                        var link = data["data"];
                        switch (type)
                        {
                            case string upvote when upvote.Equals(PushSettings.Upvote.GetEnumDescription()):
                            case string commentUpvote when commentUpvote.Equals(PushSettings.UpvoteComment.GetEnumDescription()):
                            case string comment when comment.Equals(PushSettings.Comment.GetEnumDescription()):
                            case string userPost when userPost.Equals(PushSettings.User.GetEnumDescription()):
                                OpenNewContentFragment(new PostViewFragment(link));
                                break;
                            case string follow when follow.Equals(PushSettings.Follow.GetEnumDescription()):
                            case string transfer when transfer.Equals(PushSettings.Transfer.GetEnumDescription()):
                                OpenNewContentFragment(new ProfileFragment(link));
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    AppSettings.Logger.ErrorAsync(e);
                }
            }
        }

        public void HandleSharingPhoto(Intent intent)
        {
            var uri = (Android.Net.Uri)intent.GetParcelableExtra(SharingPhotoData);
            intent.RemoveExtra(SharingPhotoData);
            if (uri != null)
            {
                var galleryModel = new GalleryMediaModel
                {
                    Path = BitmapUtils.GetUriRealPath(this, uri)
                };
                OpenNewContentFragment(new PreviewPostCreateFragment(galleryModel));
            }
        }

        public void HandlePostCreateResume(Intent intent)
        {
            var isEnable = intent.GetBooleanExtra(PostCreateResumeExtra, false);
            intent.RemoveExtra(PostCreateResumeExtra);

            if (!isEnable || !AppSettings.Temp.ContainsKey(PostCreateFragment.PostCreateGalleryTemp))
                return;

            var json = AppSettings.Temp[PostCreateFragment.PostCreateGalleryTemp];
            var media = JsonConvert.DeserializeObject<List<GalleryMediaModel>>(json);

            PreparePostModel model = null;
            if (AppSettings.Temp.ContainsKey(PostCreateFragment.PreparePostTemp))
            {
                json = AppSettings.Temp[PostCreateFragment.PreparePostTemp];
                model = JsonConvert.DeserializeObject<PreparePostModel>(json);
            }

            AppSettings.Temp.Remove(PostCreateFragment.PostCreateGalleryTemp);
            AppSettings.SaveTemp();

            OpenNewContentFragment(new PostCreateFragment(media, model));
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            Intent = intent;
        }

        public override void OnBackPressed()
        {
            CurrentHostFragment = _adapter.GetItem(_viewPager.CurrentItem) as HostFragment;
            var fragments = CurrentHostFragment?.ChildFragmentManager?.Fragments;
            if (fragments?.Count > 0)
            {
                var lastFragment = fragments.Last();
                if (lastFragment is ICanOpenPost openPostFrg && openPostFrg.ClosePost() ||
                    lastFragment is BaseFragment baseFrg && baseFrg.OnBackPressed())
                    return;
            }

            if (CurrentHostFragment == null || !CurrentHostFragment.HandleBackPressed(SupportFragmentManager))
                MinimizeApp();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            TabLayout.TabSelected -= OnTabLayoutOnTabSelected;
            TabLayout.TabReselected -= OnTabLayoutOnTabReselected;
            Cheeseknife.Reset(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (AppSettings.ProfileUpdateType != ProfileUpdateType.None)
            {
                SelectTab(_adapter.Count - 1);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == CommonPermissionsRequestCode && !grantResults.Any(x => x != Permission.Granted))
            {
                var intent = new Intent(this, typeof(CameraActivity));
                StartActivity(intent);
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void OnTabLayoutOnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            if (e.Tab.Position == 2)
            {
                _prevTab.Select();

                if (!RequestPermissions(CommonPermissionsRequestCode, Manifest.Permission.Camera, Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage))
                {
                    var intent = new Intent(this, typeof(CameraActivity));
                    StartActivity(intent);
                }
            }
            else
            {
                SelectTab(e.Tab.Position);
                _prevTab = e.Tab;
                AppSettings.SelectedTab = e.Tab.Position;
            }
        }

        private void OnTabLayoutOnTabReselected(object sender, TabLayout.TabReselectedEventArgs e)
        {
            if (TabLayout.GetTabAt(e.Tab.Position) == _prevTab)
            {
                var hostFragment = _adapter.GetItem(TabLayout.SelectedTabPosition) as HostFragment;
                hostFragment?.Clear();
                var fragments = hostFragment?.ChildFragmentManager.Fragments;
                if (fragments != null && fragments[fragments.Count - 1] is ICanOpenPost fragment)
                    fragment.ClosePost();
            }
        }

        private void InitTabs()
        {
            for (var i = 0; i < _adapter.TabIconsInactive.Length; i++)
            {
                var tab = TabLayout.NewTab();
                var tabView = new ImageView(this) { Id = Android.Resource.Id.Icon };
                tabView.SetScaleType(ImageView.ScaleType.CenterInside);
                tabView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, _tabHeight);
                tab.SetCustomView(tabView);

                TabLayout.AddTab(tab);
                if (i == _adapter.TabIconsInactive.Length - 1)
                    SetProfileChart(TabLayout.LayoutParameters.Height);
                tab.SetIcon(ContextCompat.GetDrawable(this, _adapter.TabIconsInactive[i]));
            }
            SelectTab(AppSettings.SelectedTab);
            _prevTab = TabLayout.GetTabAt(AppSettings.SelectedTab);
            _viewPager.OffscreenPageLimit = _adapter.Count - 1;
        }

        public void SelectTab(int position)
        {
            var tab = TabLayout.GetTabAt(position);
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
            for (var i = 0; i < TabLayout.TabCount - 1; i++)
            {
                var tab = TabLayout.GetTabAt(i);
                tab?.SetIcon(i == position
                             ? ContextCompat.GetDrawable(this, _adapter.TabIconsActive[i])
                             : ContextCompat.GetDrawable(this, _adapter.TabIconsInactive[i]));
            }

            SetProfileChart(TabLayout.LayoutParameters.Height);
            TryUpdateProfile();
        }

        public async Task TryUpdateProfile()
        {
            do
            {
                var result = await Presenter.TryGetUserInfoAsync(AppSettings.User.Login);
                if (IsDestroyed)
                    return;

                if (result.IsSuccess || result.Exception is System.OperationCanceledException)
                {
                    SetProfileChart(TabLayout.LayoutParameters.Height);
                    break;
                }

                await Task.Delay(5000);
                if (IsDestroyed)
                    return;

            } while (true);
        }

        private void SetProfileChart(int size)
        {
            var votingPowerFrame = new VotingPowerFrame(this)
            {
                Draw = true,
                VotingPower = Presenter.UserProfileResponse == null ? 0 : (float)Presenter.UserProfileResponse.VotingPower,
                VotingPowerWidth = BitmapUtils.DpToPixel(3, Resources)
            };
            var padding = (int)BitmapUtils.DpToPixel(7, Resources);
            votingPowerFrame.Layout(0, 0, size, size);
            var avatar = new CircleImageView(this);
            avatar.SetScaleType(ImageView.ScaleType.CenterCrop);
            avatar.Layout(padding, padding, size - padding, size - padding);
            avatar.SetImageResource(Resource.Drawable.ic_holder);
            votingPowerFrame.AddView(avatar);

            var profileTab = TabLayout.GetTabAt(TabLayout.TabCount - 1);
            if (!string.IsNullOrEmpty(Presenter.UserProfileResponse?.ProfileImage))
                Picasso.With(this).Load(Presenter.UserProfileResponse.ProfileImage).NoFade().Resize(size, size).CenterCrop()
                    .Placeholder(Resource.Drawable.ic_holder).Into(avatar,
                        () => { profileTab.SetIcon(BitmapUtils.GetViewDrawable(votingPowerFrame)); }, null);
            else
                profileTab.SetIcon(BitmapUtils.GetViewDrawable(votingPowerFrame));
        }

        protected void CheckForNewFeatures()
        {
            var lastVersion = AppSettings.Settings.BuildVersion;
            var currentVersion = AppSettings.AppInfo.GetBuildVersion();

            if (currentVersion.Equals(lastVersion))
                return;

            if (!string.IsNullOrEmpty(lastVersion))
            {
                //var handler = new Handler();
                //var saverService = new SaverService();

                //Action action = () =>
                //{
                //  var featureScreen = new FeatureDialog(this);
                //    featureScreen.Show();
                //};
                //handler.PostDelayed(action, 2000);
            }

            AppSettings.Settings.BuildVersion = currentVersion;
            AppSettings.SaveSettings();
        }
    }
}
