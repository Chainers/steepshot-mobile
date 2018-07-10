using System;
using Android.OS;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Requests;
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
        [BindView(Resource.Id.actions)] private RelativeLayout _actionsBlock;
        [BindView(Resource.Id.token_actions)] private LinearLayout _tokenActions;
        [BindView(Resource.Id.transfer_btn)] private Button _transferBtn;
#pragma warning restore 0649

        private int _pageOffset;
        private int _actionsBlockHeight;
        private int _extraActionsBlockHeight;

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

            _fragmentTitle.Typeface = Style.Semibold;

            _fragmentTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Wallet);

            _walletPager.SetClipToPadding(false);
            _pageOffset = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 30, Resources.DisplayMetrics);
            _walletPager.SetPadding(_pageOffset, 0, _pageOffset, 0);
            _walletPager.PageMargin = _pageOffset / 3;
            var walletPageAdapter = new WalletPagerAdapter(_walletPager);
            walletPageAdapter.OnPageTransforming += OnPageTransforming;
            _walletPager.Adapter = walletPageAdapter;

            _actionsBlockHeight = 810;
            _extraActionsBlockHeight = 150;

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

        private void OnPageTransforming(TokenCardHolder from, TokenCardHolder to, float position)
        {
            if (from.HasExtraButtons && !to.HasExtraButtons || !from.HasExtraButtons && to.HasExtraButtons)
                _actionsBlock.LayoutParameters.Height = (int)(_actionsBlockHeight + _extraActionsBlockHeight * position);
            _actionsBlock.RequestLayout();
        }
    }
}