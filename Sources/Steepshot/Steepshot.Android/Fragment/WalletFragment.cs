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
using Ditch.Core.JsonRpc;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Authorization;
using Steepshot.Core.Extensions;
using Steepshot.Core.Facades;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class WalletFragment : BaseFragment
    {
        public const int WalletFragmentPowerUpOrDownRequestCode = 230;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.arrow_back)] private ImageButton _backBtn;
        [BindView(Resource.Id.claim_rewards)] private ImageView _claimBtn;
        [BindView(Resource.Id.title)] private TextView _fragmentTitle;
        [BindView(Resource.Id.trx_history)] private RecyclerView _trxHistory;
#pragma warning restore 0649

        private WalletFacade _walletFacade;
        private UserInfo _prevUser;
        private GradientDrawable _transferBtnBg;
        private ViewPager _walletPager;
        private LinearLayout _actions;
        private Button _transferBtn;
        private ImageButton _moreBtn;
        private TextView _trxHistoryTitle;

        private WalletAdapter _walletAdapter;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (_walletFacade == null)
            {
                _walletFacade = App.Container.GetFacade<WalletFacade>(App.MainChain);
            }
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

        public override async void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            ToggleTabBar(true);

            _trxHistory.SetLayoutManager(new LinearLayoutManager(Activity));

            var walletCardsLayout = (RelativeLayout)LayoutInflater.From(Activity).Inflate(Resource.Layout.lyt_wallet_cards, _trxHistory, false);
            _walletAdapter = new WalletAdapter(walletCardsLayout, _walletFacade);
            _walletAdapter.AutoLinkAction += AutoLinkAction;

            await _walletFacade.TryGetCurrencyRatesAsync().ConfigureAwait(true);

            _trxHistory.SetAdapter(_walletAdapter);
            _trxHistory.AddItemDecoration(new Adapter.DividerItemDecoration(Activity));

            var walletPagerIndicator = walletCardsLayout.FindViewById<TabLayout>(Resource.Id.page_indicator);
            var pageOffset = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, Resources.DisplayMetrics);
            var cardRatio = TypedValue.ApplyDimension(ComplexUnitType.Dip, 335, Resources.DisplayMetrics) / TypedValue.ApplyDimension(ComplexUnitType.Dip, 190, Resources.DisplayMetrics);

            _walletPager = walletCardsLayout.FindViewById<ViewPager>(Resource.Id.wallet_pager);
            _walletPager.LayoutParameters.Width = Style.ScreenWidth;
            _walletPager.LayoutParameters.Height = (int)((_walletPager.LayoutParameters.Width - pageOffset * 1.5) / cardRatio);
            _walletPager.RequestLayout();

            _walletPager.SetClipToPadding(false);
            _walletPager.SetPadding(pageOffset, 0, pageOffset, 0);
            _walletPager.PageMargin = pageOffset / 2;
            _walletPager.Adapter = new WalletPagerAdapter(_walletPager, _walletFacade);

            _walletPager.PageScrolled += OnPageScrolled;
            _walletPager.PageSelected += OnPageSelected;
            walletPagerIndicator.SetupWithViewPager(_walletPager, true);

            _transferBtn = walletCardsLayout.FindViewById<Button>(Resource.Id.transfer_btn);
            _transferBtn.Text = App.Localization.GetText(LocalizationKeys.Transfer);
            _transferBtn.Click += TransferBtnOnClick;

            _moreBtn = walletCardsLayout.FindViewById<ImageButton>(Resource.Id.more);
            _moreBtn.Click += MoreBtnOnClick;

            _trxHistoryTitle = walletCardsLayout.FindViewById<TextView>(Resource.Id.trx_history_title);
            _trxHistoryTitle.Typeface = Style.Semibold;
            _trxHistoryTitle.Text = App.Localization.GetText(LocalizationKeys.TransactionHistory);

            _actions = walletCardsLayout.FindViewById<LinearLayout>(Resource.Id.actions);
            var actionsLytParams = (RelativeLayout.LayoutParams)_actions.LayoutParameters;
            actionsLytParams.TopMargin = (int)(_walletPager.LayoutParameters.Height / 2f);

            var pagerLytParams = (LinearLayout.LayoutParams)walletPagerIndicator.LayoutParameters;
            pagerLytParams.TopMargin = actionsLytParams.TopMargin;

            _transferBtnBg = new GradientDrawable(GradientDrawable.Orientation.LeftRight, new int[] { Style.R255G121B4, Style.R255G22B5 });
            _transferBtnBg.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, 25, Resources.DisplayMetrics));

            _fragmentTitle.Typeface = Style.Semibold;
            _fragmentTitle.Text = App.Localization.GetText(LocalizationKeys.Wallet);

            _claimBtn.Click += ClaimBtnOnClick;
            _backBtn.Click += BackOnClick;

            _walletFacade.TryUpdateWallets();
        }

        private void BackOnClick(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).OnBackPressed();
        }

        private void OnPageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            _walletFacade.Selected = e.Position;
        }

        private void TransferBtnOnClick(object sender, EventArgs e)
        {
            var fragment = new TransferFragment(_walletFacade.SelectedWallet.UserInfo, _walletFacade.SelectedBalance.CurrencyType);
            ((BaseActivity)Activity).OpenNewContentFragment(fragment);
        }

        private void MoreBtnOnClick(object sender, EventArgs e)
        {
            var moreDialog = new WalletExtraDialog(Activity, _walletFacade.SelectedBalance);
            moreDialog.ExtraAction += ExtraAction;
            moreDialog.Show();
        }

        private void ExtraAction(PowerAction action)
        {
            switch (action)
            {
                case PowerAction.PowerUp:
                case PowerAction.PowerDown:
                    {
                        var powerUpOrDownFrag = new PowerUpDownFragment(_walletFacade.SelectedWallet.UserInfo, _walletFacade.SelectedBalance, action);
                        powerUpOrDownFrag.SetTargetFragment(this, WalletFragmentPowerUpOrDownRequestCode);
                        ((BaseActivity)Activity).OpenNewContentFragment(powerUpOrDownFrag);
                        break;
                    }
                case PowerAction.CancelPowerDown:
                    {
                        var alertAction = new ActionAlertDialog(Activity, string.Format(App.Localization.GetText(LocalizationKeys.CancelPowerDownAlert), _walletFacade.SelectedBalance.ToWithdraw.ToBalanceValueString()),
                            string.Empty, App.Localization.GetText(LocalizationKeys.Yes),
                            App.Localization.GetText(LocalizationKeys.No), AutoLinkAction);
                        alertAction.AlertAction += () =>
                        {
                            var userInfo = _walletFacade.SelectedWallet.UserInfo;
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
        }

        private async void CancelPowerDown()
        {
            var model = new PowerUpDownModel(_walletFacade.SelectedWallet.UserInfo)
            {
                Value = 0,
                CurrencyType = _walletFacade.SelectedBalance.CurrencyType,
                PowerAction = PowerAction.CancelPowerDown
            };

            await _walletFacade.TransferPresenter.TryPowerUpOrDownAsync(model);
            TryUpdateBalance(_walletFacade.SelectedWallet.UserInfo, _walletFacade.SelectedBalance);
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Result.Ok)
            {
                switch (requestCode)
                {
                    case WalletFragmentPowerUpOrDownRequestCode:
                        {
                            //Intent data?
                            TryUpdateBalance(_walletFacade.SelectedWallet.UserInfo, _walletFacade.SelectedBalance);
                            break;
                        }
                    case ActiveSignInActivity.ActiveKeyRequestCode:
                        CancelPowerDown();
                        break;
                }
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private async void TryUpdateBalance(UserInfo userInfo, BalanceModel balance)
        {
            var result = await _walletFacade.TryUpdateWallet(userInfo);

            if (result.IsSuccess)
            {
                var hasClaimRewards = balance.RewardSteem > 0 ||
                                     balance.RewardSp > 0 ||
                                     balance.RewardSbd > 0;
                _claimBtn.Visibility = hasClaimRewards ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        private void ClaimBtnOnClick(object sender, EventArgs e)
        {
            DoClaimReward();
        }

        private void DoClaimReward()
        {
            var claimRewardsDialog = new ClaimRewardsDialog(Activity, _walletFacade.SelectedWallet.UserInfo, _walletFacade.SelectedBalance);
            claimRewardsDialog.Claim += Claim;
            claimRewardsDialog.Show();
        }

        private async Task<OperationResult<VoidResponse>> Claim(UserInfo userInfo, BalanceModel balance)
        {
            var result = await _walletFacade.WalletPresenter.TryClaimRewardsAsync(userInfo, balance);
            if (result.IsSuccess)
                _claimBtn.Visibility = ViewStates.Gone;
            return result;
        }

        private float _pLast = 0;
        private void OnPageScrolled(object sender, ViewPager.PageScrolledEventArgs e)
        {
            if (_walletFacade.Balances.Length <= e.Position + 1)
                return;

            if (Math.Abs(_pLast - e.PositionOffset) < 0.01)
                return;

            _pLast = e.PositionOffset;

            var fB = _walletFacade.Balances[e.Position];
            var tB = _walletFacade.Balances[e.Position + 1];

            var fromHasMore = fB.CurrencyType == CurrencyType.Steem ||
                              fB.CurrencyType == CurrencyType.Golos;

            var toHasMore = tB.CurrencyType == CurrencyType.Steem ||
                            tB.CurrencyType == CurrencyType.Golos;

            if (fromHasMore && !toHasMore || !fromHasMore && toHasMore)
            {
                var w = Style.WalletBtnTransferMaxWidth - (Style.WalletBtnTransferMaxWidth - Style.WalletBtnTransferMinWidth) * _pLast;

                _transferBtn.LayoutParameters.Width = (int)w;
                _transferBtn.RequestLayout();

                _moreBtn.Alpha = _pLast;
            }

            var fromHasClaimRewards = fB.RewardSteem > 0 || fB.RewardSp > 0 || fB.RewardSbd > 0;
            var toHasClaimRewards = tB.RewardSteem > 0 || tB.RewardSp > 0 || tB.RewardSbd > 0;

            if (!fromHasClaimRewards && !toHasClaimRewards)
            {
                _claimBtn.Visibility = ViewStates.Gone;
                return;
            }

            if (fromHasClaimRewards && !toHasClaimRewards || !fromHasClaimRewards)
            {
                _claimBtn.Alpha = 1 - _pLast;
                _claimBtn.Visibility = _claimBtn.Alpha <= 0.1 ? ViewStates.Gone : ViewStates.Visible;
            }
            else
            {
                _claimBtn.Visibility = ViewStates.Visible;
            }
        }


        public override void OnDetach()
        {
            _walletFacade.TasksCancel();
            _trxHistory.SetAdapter(null);
            base.OnDetach();
        }
    }
}