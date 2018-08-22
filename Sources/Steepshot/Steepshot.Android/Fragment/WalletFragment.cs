using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Authorization;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class WalletFragment : BaseFragmentWithPresenter<WalletPresenter>
    {
        public const int WalletFragmentPowerUpOrDownRequestCode = 230;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.arrow_back)] private ImageButton _backBtn;
        [BindView(Resource.Id.claim_rewards)] private ImageView _claimBtn;
        [BindView(Resource.Id.title)] private TextView _fragmentTitle;
        [BindView(Resource.Id.trx_history)] private RecyclerView _trxHistory;
#pragma warning restore 0649

        private int _pageOffset, _transferBtnFullWidth, _transferBtnCollapsedWidth;
        private float _cardRatio;

        private UserInfo _prevUser;
        private GradientDrawable _transferBtnBg;
        private RelativeLayout _walletCardsLayout;
        private ViewPager _walletPager;
        private LinearLayout _actions;
        private TabLayout _walletPagerIndicator;
        private Button _transferBtn;
        private ImageButton _moreBtn;
        private TextView _trxHistoryTitle;

        private WalletAdapter _walletAdapter;
        private WalletAdapter WalletAdapter => _walletAdapter ?? (_walletAdapter = new WalletAdapter(_walletCardsLayout));

        private WalletPagerAdapter _walletPagerAdapter;
        private WalletPagerAdapter WalletPagerAdapter => _walletPagerAdapter ?? (_walletPagerAdapter = new WalletPagerAdapter(_walletPager, Presenter));

        public override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            await Presenter.TryGetCurrencyRates();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_wallet, null);
                Cheeseknife.Bind(this, InflatedView);
            }

            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);
            ToggleTabBar(true);
            _pageOffset = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, Resources.DisplayMetrics);

            _trxHistory.SetLayoutManager(new LinearLayoutManager(Activity));
            _walletCardsLayout = (RelativeLayout)LayoutInflater.From(Activity).Inflate(Resource.Layout.lyt_wallet_cards, _trxHistory, false);
            _trxHistory.SetAdapter(WalletAdapter);
            _trxHistory.AddItemDecoration(new Adapter.DividerItemDecoration(Activity));

            _walletPager = _walletCardsLayout.FindViewById<ViewPager>(Resource.Id.wallet_pager);
            _actions = _walletCardsLayout.FindViewById<LinearLayout>(Resource.Id.actions);
            _walletPagerIndicator = _walletCardsLayout.FindViewById<TabLayout>(Resource.Id.page_indicator);
            _transferBtn = _walletCardsLayout.FindViewById<Button>(Resource.Id.transfer_btn);
            _moreBtn = _walletCardsLayout.FindViewById<ImageButton>(Resource.Id.more);
            _trxHistoryTitle = _walletCardsLayout.FindViewById<TextView>(Resource.Id.trx_history_title);

            _cardRatio = TypedValue.ApplyDimension(ComplexUnitType.Dip, 335, Resources.DisplayMetrics) / TypedValue.ApplyDimension(ComplexUnitType.Dip, 190, Resources.DisplayMetrics);
            _walletPager.LayoutParameters.Width = Resources.DisplayMetrics.WidthPixels;
            _walletPager.LayoutParameters.Height = (int)((_walletPager.LayoutParameters.Width - _pageOffset * 1.5) / _cardRatio);
            _walletPager.RequestLayout();

            _walletPager.SetClipToPadding(false);
            _walletPager.SetPadding(_pageOffset, 0, _pageOffset, 0);
            _walletPager.PageMargin = _pageOffset / 2;
            _walletPagerIndicator.SetupWithViewPager(_walletPager, true);
            _walletPager.Adapter = WalletPagerAdapter;

            var actionsLytParams = (RelativeLayout.LayoutParams)_actions.LayoutParameters;
            actionsLytParams.TopMargin = (int)(_walletPager.LayoutParameters.Height / 2f);

            var pagerLytParams = (LinearLayout.LayoutParams)_walletPagerIndicator.LayoutParameters;
            pagerLytParams.TopMargin = actionsLytParams.TopMargin;

            _transferBtnBg = new GradientDrawable(GradientDrawable.Orientation.LeftRight, new int[] { Style.R255G121B4, Style.R255G22B5 });
            _transferBtnBg.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, 25, Resources.DisplayMetrics));

            _fragmentTitle.Typeface = Style.Semibold;
            _trxHistoryTitle.Typeface = Style.Semibold;

            _fragmentTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Wallet);
            _trxHistoryTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransactionHistory);
            _transferBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Transfer);

            _walletPager.PageScrolled += WalletPagerOnPageScrolled;
            WalletPagerAdapter.OnPageTransforming += OnPageTransforming;
            WalletAdapter.AutoLinkAction += AutoLinkAction;
            _transferBtn.Click += TransferBtnOnClick;
            _trxHistory.ViewTreeObserver.GlobalLayout += RecyclerLayedOut;
            _moreBtn.Click += MoreBtnOnClick;
            _claimBtn.Click += ClaimBtnOnClick;
            _backBtn.Click += BackOnClick;
        }

        private void RecyclerLayedOut(object sender, EventArgs e)
        {
            _transferBtnFullWidth = _transferBtn.Width;
            var moreBtnLytParams = (RelativeLayout.LayoutParams)_moreBtn.LayoutParameters;
            _transferBtnCollapsedWidth = _transferBtnFullWidth - moreBtnLytParams.Width - moreBtnLytParams.LeftMargin;
            _transferBtn.ViewTreeObserver.GlobalLayout -= RecyclerLayedOut;
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }

        private void BackOnClick(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).OnBackPressed();
        }

        private void WalletPagerOnPageScrolled(object sender, ViewPager.PageScrolledEventArgs e)
        {
            if (e.Position == Presenter.Balances.Count)
            {
                _transferBtn.Enabled = false;
                _claimBtn.Enabled = false;
                _transferBtnBg.SetColors(new int[] { Style.R230G230B230, Style.R230G230B230 });
                WalletAdapter.SetAccountHistory(null);
                LoadNextAccount();
            }
            else
            {
                _transferBtn.Enabled = true;
                _claimBtn.Enabled = true;
                _transferBtnBg.SetColors(new int[] { Style.R255G121B4, Style.R255G22B5 });
            }

            _transferBtn.Background = _transferBtnBg;

            if (_walletPager.CurrentItem < Presenter.Balances.Count &&
                Presenter.Balances[_walletPager.CurrentItem].UserInfo != _prevUser)
            {
                _prevUser = Presenter.Balances[_walletPager.CurrentItem].UserInfo;
                if (Presenter.ConnectedUsers.ContainsKey(_prevUser.Id))
                {
                    _trxHistory.Post(() =>
                        _walletAdapter.SetAccountHistory(_prevUser.AccountHistory));
                }
            }
        }

        private void LoadNextAccount() => Task.Run(async () =>
        {
            var currAcc = Presenter.Current;
            Presenter.SetClient(currAcc?.Chain == Core.KnownChains.Steem ? App.SteemClient : App.GolosClient);
            var exception = await Presenter.TryLoadNextAccountInfo();
            if (exception == null && IsInitialized && !IsDetached)
                Activity.RunOnUiThread(() =>
                {
                    WalletPagerAdapter?.NotifyDataSetChanged();
                });
        });

        private void TransferBtnOnClick(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).OpenNewContentFragment(new TransferFragment(Presenter.Balances[_walletPager.CurrentItem].UserInfo, Presenter.Balances[_walletPager.CurrentItem].CurrencyType));
        }

        private void MoreBtnOnClick(object sender, EventArgs e)
        {
            var moreDialog = new WalletExtraDialog(Activity, Presenter.Balances[_walletPager.CurrentItem]);
            moreDialog.ExtraAction += ExtraAction;
            moreDialog.Show();
        }

        private void ExtraAction(PowerAction action)
        {
            switch (action)
            {
                case PowerAction.PowerUp:
                case PowerAction.PowerDown:
                    var powerUpOrDownFrag = new PowerUpDownFragment(Presenter.Balances[_walletPager.CurrentItem], action);
                    powerUpOrDownFrag.SetTargetFragment(this, WalletFragmentPowerUpOrDownRequestCode);
                    ((BaseActivity)Activity).OpenNewContentFragment(powerUpOrDownFrag);
                    break;
                case PowerAction.CancelPowerDown:
                    var alertAction = new ActionAlertDialog(Activity, string.Format(AppSettings.LocalizationManager.GetText(LocalizationKeys.CancelPowerDownAlert), Presenter.Balances[_walletPager.CurrentItem].ToWithdraw.ToBalanceValueString()),
                        string.Empty, AppSettings.LocalizationManager.GetText(LocalizationKeys.Yes),
                        AppSettings.LocalizationManager.GetText(LocalizationKeys.No));
                    alertAction.AlertAction += () =>
                    {
                        var userInfo = Presenter.Balances[_walletPager.CurrentItem].UserInfo;
                        if (string.IsNullOrEmpty(userInfo.ActiveKey))
                        {
                            var intent = new Intent(Activity, typeof(ActiveSignInActivity));
                            intent.PutExtra(ActiveSignInActivity.ActiveSignInUserName, userInfo.Login);
                            intent.PutExtra(ActiveSignInActivity.ActiveSignInChain, (int)userInfo.Chain);
                            StartActivityForResult(intent, ActiveSignInActivity.ActiveKeyRequestCode);
                            return;
                        }

                        CancelPowerDown();
                    };
                    alertAction.Show();
                    break;
            }
        }

        private async void CancelPowerDown()
        {
            var balance = Presenter.Balances[_walletPager.CurrentItem];
            var model = new BalanceModel(0, balance.MaxDecimals, balance.CurrencyType)
            {
                UserInfo = balance.UserInfo
            };

            await Presenter.TryPowerUpOrDown(model, PowerAction.CancelPowerDown);
            TryUpdateBalance(balance);
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Result.Ok)
            {
                switch (requestCode)
                {
                    case WalletFragmentPowerUpOrDownRequestCode:
                        TryUpdateBalance(Presenter.Balances[_walletPager.CurrentItem]);
                        break;
                    case ActiveSignInActivity.ActiveKeyRequestCode:
                        CancelPowerDown();
                        break;
                }
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private async void TryUpdateBalance(BalanceModel balance)
        {
            var exception = await Presenter.TryUpdateAccountInfo(balance.UserInfo);
            if (exception == null)
            {
                WalletPagerAdapter.NotifyItemChanged(_walletPager.CurrentItem);
                WalletAdapter.SetAccountHistory(balance.UserInfo.AccountHistory);
                var hasClaimRewards = Presenter.Balances[_walletPager.CurrentItem].RewardSteem > 0 ||
                                      Presenter.Balances[_walletPager.CurrentItem].RewardSp > 0 ||
                                      Presenter.Balances[_walletPager.CurrentItem].RewardSbd > 0;
                _claimBtn.Visibility = hasClaimRewards ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        private void ClaimBtnOnClick(object sender, EventArgs e)
        {
            DoClaimReward();
        }

        private void DoClaimReward()
        {
            var claimRewardsDialog = new ClaimRewardsDialog(Activity, Presenter.Balances[_walletPager.CurrentItem]);
            claimRewardsDialog.Claim += Claim;
            claimRewardsDialog.Show();
        }

        private async Task<Exception> Claim(BalanceModel balance)
        {
            var error = await Presenter.TryClaimRewards(balance);
            if (error == null)
                _claimBtn.Visibility = ViewStates.Gone;
            return error;
        }

        private void OnPageTransforming(TokenCardHolder fromCard, TokenCardHolder toCard, float progress)
        {
            var fromHasMore = fromCard.Balance.CurrencyType == CurrencyType.Steem ||
                              fromCard.Balance.CurrencyType == CurrencyType.Golos;

            var toHasMore = toCard.Balance.CurrencyType == CurrencyType.Steem ||
                            toCard.Balance.CurrencyType == CurrencyType.Golos;

            if (fromHasMore && !toHasMore || !fromHasMore && toHasMore)
            {
                _transferBtn.LayoutParameters.Width = _transferBtnFullWidth - (int)((_transferBtnFullWidth - _transferBtnCollapsedWidth) * progress);
                _transferBtn.RequestLayout();
                _moreBtn.Alpha = progress;
            }

            var fromHasClaimRewards = fromCard.Balance.RewardSteem > 0 || fromCard.Balance.RewardSp > 0 ||
                                      fromCard.Balance.RewardSbd > 0;

            var toHasClaimRewards = toCard.Balance.RewardSteem > 0 || toCard.Balance.RewardSp > 0 ||
                                    toCard.Balance.RewardSbd > 0;

            if (!fromHasClaimRewards && !toHasClaimRewards)
            {
                _claimBtn.Visibility = ViewStates.Gone;
                return;
            }

            if (fromHasClaimRewards && !toHasClaimRewards || !fromHasClaimRewards)
            {
                _claimBtn.Alpha = 1 - progress;
                _claimBtn.Visibility = _claimBtn.Alpha <= 0.1 ? ViewStates.Gone : ViewStates.Visible;
            }
            else
            {
                _claimBtn.Visibility = ViewStates.Visible;
            }
        }
    }
}