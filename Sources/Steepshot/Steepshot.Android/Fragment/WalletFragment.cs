using System;
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
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class WalletFragment : BaseFragmentWithPresenter<PreSignInPresenter>
    {
#pragma warning disable 0649, 4014        
        [BindView(Resource.Id.title)] private TextView _fragmentTitle;
        [BindView(Resource.Id.wallet_pager)] private ViewPager _walletPager;
        [BindView(Resource.Id.page_indicator)] private TabLayout _walletPagerIndicator;
        [BindView(Resource.Id.transfer_btn)] private Button _transferBtn;
        [BindView(Resource.Id.trx_history_title)] private TextView _trxHistoryTitle;
        [BindView(Resource.Id.trx_history)] private RecyclerView _trxHistory;
#pragma warning restore 0649

        private int _pageOffset;

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

            _walletPager.SetClipToPadding(false);
            _pageOffset = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, Resources.DisplayMetrics);
            _walletPager.SetPadding(_pageOffset, 0, _pageOffset, 0);
            _walletPager.PageMargin = _pageOffset / 2;
            _walletPagerIndicator.SetupWithViewPager(_walletPager, true);
            var walletPageAdapter = new WalletPagerAdapter(_walletPager);
            _walletPager.Adapter = walletPageAdapter;

            _trxHistory.SetAdapter(new TrxHistoryAdapter());
            _trxHistory.SetLayoutManager(new LinearLayoutManager(Activity));
            _trxHistory.AddItemDecoration(new Adapter.DividerItemDecoration(Activity));

            _transferBtn.Click += TransferBtnOnClick;

            UpdateAccountInfo();
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }

        private async void UpdateAccountInfo()
        {
            var response = await Presenter.TryGetAccountInfo(AppSettings.User.Login);
            if (response.IsSuccess)
            {
                AppSettings.User.AccountInfo = response.Result;
                ((WalletPagerAdapter)_walletPager.Adapter).UpdateData(AppSettings.User.AccountInfo.Balances);
            }
        }

        private void TransferBtnOnClick(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).OpenNewContentFragment(new TransferFragment());
        }
    }
}