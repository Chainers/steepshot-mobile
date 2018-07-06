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
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class WalletFragment : BaseFragmentWithPresenter<WalletPresenter>
    {
#pragma warning disable 0649, 4014        
        [BindView(Resource.Id.title)] private TextView _fragmentTitle;
        [BindView(Resource.Id.wallet_pager)] private ViewPager _walletPager;
#pragma warning restore 0649

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
            var pagePadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 30, Resources.DisplayMetrics);
            _walletPager.SetPadding(pagePadding, 0, pagePadding, 0);
            _walletPager.PageMargin = pagePadding / 3;
            var walletPageAdapter = new WalletPagerAdapter(_walletPager);
            _walletPager.Adapter = walletPageAdapter;
            _walletPager.SetPageTransformer(false, walletPageAdapter, (int)LayerType.None);
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }
    }
}