using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class WalletFragment : BaseFragmentWithPresenter<WalletPresenter>
    {
#pragma warning disable 0649, 4014        
        [BindView(Resource.Id.coordinator)] private CoordinatorLinearLayout _coordinator;
        [BindView(Resource.Id.title)] private TextView _fragmentTitle;
        [BindView(Resource.Id.wallet_pager)] private ViewPager _walletPager;
        [BindView(Resource.Id.actions)] private LinearLayout _actions;
        [BindView(Resource.Id.page_indicator)] private TabLayout _walletPagerIndicator;
        [BindView(Resource.Id.transfer_btn)] private Button _transferBtn;
        [BindView(Resource.Id.trx_history_title)] private TextView _trxHistoryTitle;
        [BindView(Resource.Id.history_spinner)] private ProgressBar _historySpinner;
        [BindView(Resource.Id.trx_history)] private CoordinatorRecyclerView _trxHistory;
#pragma warning restore 0649

        private int _pageOffset;
        private float _cardRatio;
        private string _prevUser;
        private GradientDrawable _transferBtnBg;
        private TrxHistoryAdapter _trxHistoryAdapter;
        private TrxHistoryAdapter TrxHistoryAdapter => _trxHistoryAdapter ?? (_trxHistoryAdapter = new TrxHistoryAdapter());

        private WalletPagerAdapter _walletPagerAdapter;
        private WalletPagerAdapter WalletPagerAdapter => _walletPagerAdapter ?? (_walletPagerAdapter = new WalletPagerAdapter(_walletPager, Presenter));

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
            _trxHistoryTitle.Text = "Transaction history";
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
            _trxHistoryTitle.ViewTreeObserver.GlobalLayout += ViewTreeObserverOnGlobalLayout;
        }

        private void ViewTreeObserverOnGlobalLayout(object sender, EventArgs e)
        {
            _coordinator.LayoutParameters.Height = ((View)_trxHistoryTitle.Parent).Bottom + Resources.DisplayMetrics.HeightPixels;
            _coordinator.SetTopViewParam(Resources.DisplayMetrics.WidthPixels, -(int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 30, Resources.DisplayMetrics));
            _trxHistoryTitle.ViewTreeObserver.GlobalLayout -= ViewTreeObserverOnGlobalLayout;
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }

        private void WalletPagerOnPageScrolled(object sender, ViewPager.PageScrolledEventArgs e)
        {
            if (e.Position == Presenter.Balances.Count)
            {
                _transferBtnBg.SetColors(new int[] { Style.R230G230B230, Style.R230G230B230 });
                TrxHistoryAdapter.SetAccountHistory(null);
                LoadNextAccount();
            }
            else
            {
                _transferBtnBg.SetColors(new int[] { Style.R255G121B4, Style.R255G22B5 });
            }


            if (_walletPager.CurrentItem < Presenter.Balances.Count &&
                !Presenter.Balances[_walletPager.CurrentItem].UserName.Equals(_prevUser))
            {
                _prevUser = Presenter.Balances[_walletPager.CurrentItem].UserName;
                var history = AppSettings.DataProvider.Select()
                    .Find(x => x.Login.Equals(_prevUser, StringComparison.OrdinalIgnoreCase))?.AccountHistory;
                if (history != null)
                {
                    _trxHistoryAdapter.SetAccountHistory(history);
                    _historySpinner.Visibility = ViewStates.Gone;
                }
            }

            _transferBtn.Background = _transferBtnBg;
        }

        private void LoadNextAccount() => Task.Run(async () =>
        {
            var currAcc = Presenter.ConnectedUsers.Current;
            Presenter.SetClient(currAcc?.Chain == Core.KnownChains.Steem ? App.SteemClient : App.GolosClient);
            var error = await Presenter.TryLoadNextAccountInfo();
            if (error == null)
                Activity.RunOnUiThread(() =>
                {
                    WalletPagerAdapter.NotifyDataSetChanged();
                });
        });

        private void TransferBtnOnClick(object sender, EventArgs e)
        {
        }

        private void OnPageTransforming(TokenCardHolder fromCard, TokenCardHolder toCard, float progress)
        {

        }
    }
}