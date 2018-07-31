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
        [BindView(Resource.Id.coordinator)] private CoordinatorLinearLayout _coordinator;
        [BindView(Resource.Id.title)] private TextView _fragmentTitle;
        [BindView(Resource.Id.wallet_pager)] private ViewPager _walletPager;
        [BindView(Resource.Id.actions)] private LinearLayout _actions;
        [BindView(Resource.Id.page_indicator)] private TabLayout _walletPagerIndicator;
        [BindView(Resource.Id.transfer_btn)] private Button _transferBtn;
        [BindView(Resource.Id.more)] private ImageButton _moreBtn;
        [BindView(Resource.Id.trx_history_title)] private TextView _trxHistoryTitle;
        [BindView(Resource.Id.trx_history)] private CoordinatorRecyclerView _trxHistory;
#pragma warning restore 0649

        private int _pageOffset, _transferBtnFullWidth, _transferBtnCollapsedWidth;
        private float _cardRatio;
        private string _prevUser;
        private GradientDrawable _transferBtnBg;
        private TrxHistoryAdapter _trxHistoryAdapter;
        private TrxHistoryAdapter TrxHistoryAdapter => _trxHistoryAdapter ?? (_trxHistoryAdapter = new TrxHistoryAdapter());

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

            _fragmentTitle.Typeface = Style.Semibold;
            _trxHistoryTitle.Typeface = Style.Semibold;

            _fragmentTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Wallet);
            _trxHistoryTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransactionHistory);
            _transferBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Transfer);

            _cardRatio = TypedValue.ApplyDimension(ComplexUnitType.Dip, 335, Resources.DisplayMetrics) / TypedValue.ApplyDimension(ComplexUnitType.Dip, 190, Resources.DisplayMetrics);
            _walletPager.LayoutParameters.Width = Resources.DisplayMetrics.WidthPixels;
            _walletPager.LayoutParameters.Height = (int)((_walletPager.LayoutParameters.Width - _pageOffset * 3) / _cardRatio);
            _walletPager.RequestLayout();

            _walletPager.SetClipToPadding(false);
            _pageOffset = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, Resources.DisplayMetrics);
            _walletPager.SetPadding(_pageOffset, 0, _pageOffset, 0);
            _walletPager.PageMargin = _pageOffset / 2;
            _walletPagerIndicator.SetupWithViewPager(_walletPager, true);
            _walletPager.Adapter = WalletPagerAdapter;

            var actionsLytParams = (RelativeLayout.LayoutParams)_actions.LayoutParameters;
            actionsLytParams.TopMargin = (int)(_walletPager.LayoutParameters.Height / 2f);

            var pagerLytParams = (LinearLayout.LayoutParams)_walletPagerIndicator.LayoutParameters;
            pagerLytParams.TopMargin = actionsLytParams.TopMargin;

            _trxHistory.SetAdapter(TrxHistoryAdapter);
            _trxHistory.SetLayoutManager(new LinearLayoutManager(Activity));
            _trxHistory.AddItemDecoration(new Adapter.DividerItemDecoration(Activity));
            _trxHistory.SetCoordinatorListener(_coordinator);

            _transferBtnBg = new GradientDrawable(GradientDrawable.Orientation.LeftRight, new int[] { Style.R255G121B4, Style.R255G22B5 });
            _transferBtnBg.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, 25, Resources.DisplayMetrics));

            _walletPager.PageScrolled += WalletPagerOnPageScrolled;
            WalletPagerAdapter.OnPageTransforming += OnPageTransforming;
            _transferBtn.Click += TransferBtnOnClick;
            _transferBtn.ViewTreeObserver.GlobalLayout += TransferBtnOnGlobalLayout;
            _trxHistoryTitle.ViewTreeObserver.GlobalLayout += HistoryLabelOnGlobalLayout;
            _moreBtn.Click += MoreBtnOnClick;
            _claimBtn.Click += ClaimBtnOnClick;
            _backBtn.Click += BackOnClick;
        }

        private void TransferBtnOnGlobalLayout(object sender, EventArgs e)
        {
            _transferBtnFullWidth = _transferBtn.Width;
            var moreBtnLytParams = (RelativeLayout.LayoutParams)_moreBtn.LayoutParameters;
            _transferBtnCollapsedWidth = _transferBtnFullWidth - moreBtnLytParams.Width - moreBtnLytParams.LeftMargin;
            _transferBtn.ViewTreeObserver.GlobalLayout -= TransferBtnOnGlobalLayout;
        }

        private void HistoryLabelOnGlobalLayout(object sender, EventArgs e)
        {
            _coordinator.LayoutParameters.Height = _trxHistoryTitle.Bottom + Resources.DisplayMetrics.HeightPixels;
            _coordinator.SetTopViewParam(Resources.DisplayMetrics.WidthPixels, -(int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 30, Resources.DisplayMetrics));
            _trxHistoryTitle.ViewTreeObserver.GlobalLayout -= HistoryLabelOnGlobalLayout;
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
                _coordinator.Enabled = false;
                _transferBtn.Enabled = false;
                _claimBtn.Enabled = false;
                _transferBtnBg.SetColors(new int[] { Style.R230G230B230, Style.R230G230B230 });
                TrxHistoryAdapter.SetAccountHistory(null);
                LoadNextAccount();
            }
            else
            {
                _coordinator.Enabled = true;
                _transferBtn.Enabled = true;
                _claimBtn.Enabled = true;
                _transferBtnBg.SetColors(new int[] { Style.R255G121B4, Style.R255G22B5 });
            }

            _transferBtn.Background = _transferBtnBg;

            if (_walletPager.CurrentItem < Presenter.Balances.Count &&
                !Presenter.Balances[_walletPager.CurrentItem].UserInfo.Login.Equals(_prevUser))
            {
                _prevUser = Presenter.Balances[_walletPager.CurrentItem].UserInfo.Login;
                if (Presenter.ConnectedUsers.ContainsKey(_prevUser))
                {
                    _trxHistoryAdapter.SetAccountHistory(Presenter.ConnectedUsers[_prevUser].AccountHistory);
                }
            }
        }

        private void LoadNextAccount() => Task.Run(async () =>
        {
            var currAcc = Presenter.Current;
            Presenter.SetClient(currAcc?.Chain == Core.KnownChains.Steem ? App.SteemClient : App.GolosClient);
            var error = await Presenter.TryLoadNextAccountInfo();
            if (error == null && IsInitialized && !IsDetached)
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
            var moreDialog = new WalletExtraDialog((BaseActivity)Activity);
            moreDialog.PowerUp += PowerUp;
            moreDialog.PowerDown += PowerDown;
            moreDialog.Show();
        }

        private void PowerUp()
        {
            var powerUpFrag = new PowerUpDownFragment(Presenter.Balances[_walletPager.CurrentItem], PowerAction.PowerUp);
            powerUpFrag.SetTargetFragment(this, WalletFragmentPowerUpOrDownRequestCode);
            ((BaseActivity)Activity).OpenNewContentFragment(powerUpFrag);
        }

        private void PowerDown()
        {
            var powerDownFrag = new PowerUpDownFragment(Presenter.Balances[_walletPager.CurrentItem], PowerAction.PowerDown);
            powerDownFrag.SetTargetFragment(this, WalletFragmentPowerUpOrDownRequestCode);
            ((BaseActivity)Activity).OpenNewContentFragment(powerDownFrag);
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
                        DoClaimReward();
                        break;
                }
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private async void TryUpdateBalance(BalanceModel balance)
        {
            var userName = balance.UserInfo.Login;
            var error = await Presenter.TryUpdateAccountInfo(userName);
            if (error == null)
            {
                WalletPagerAdapter.NotifyItemChanged(_walletPager.CurrentItem);
                TrxHistoryAdapter.SetAccountHistory(Presenter.ConnectedUsers[userName].AccountHistory);
                var hasClaimRewards = Presenter.Balances[_walletPager.CurrentItem].RewardSteem > 0 ||
                                      Presenter.Balances[_walletPager.CurrentItem].RewardSp > 0 ||
                                      Presenter.Balances[_walletPager.CurrentItem].RewardSbd > 0;
                _claimBtn.Visibility = hasClaimRewards ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        private void ClaimBtnOnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Presenter.Balances[_walletPager.CurrentItem].UserInfo.ActiveKey))
            {
                var intent = new Intent(Activity, typeof(ActiveSignInActivity));
                StartActivityForResult(intent, ActiveSignInActivity.ActiveKeyRequestCode);
                return;
            }

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
            {
                TryUpdateBalance(balance);
                return null;
            }

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

            if (fromHasClaimRewards && !toHasClaimRewards || !fromHasClaimRewards && toHasClaimRewards)
            {
                _claimBtn.Alpha = 1 - progress;
                _claimBtn.Visibility = _claimBtn.Alpha <= 0.1 ? ViewStates.Gone : ViewStates.Visible;
            }
            else
            {
                _claimBtn.Visibility = ViewStates.Gone;
            }
        }
    }
}