using System.ComponentModel;
using Android.Graphics;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using IO.SuperCharge.ShimmerLayoutLib;
using Steepshot.Base;
using Steepshot.Core.Authorization;
using Steepshot.Core.Extensions;
using Steepshot.Core.Facades;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Utils;
using Object = Java.Lang.Object;

namespace Steepshot.Adapter
{
    public class WalletPagerAdapter : Android.Support.V4.View.PagerAdapter
    {
        private const int CachedPagesCount = 5;
        private readonly ViewPager _pager;
        private readonly TokenCardHolder[] _holders;
        private View _shimmerLoading;
        private readonly WalletFacade _walletFacade;


        public override int Count => _walletFacade.BalanceCount;


        public WalletPagerAdapter(ViewPager pager, WalletFacade walletFacade)
        {
            _pager = pager;
            _walletFacade = walletFacade;
            _holders = new TokenCardHolder[CachedPagesCount];
        }


        public override Object InstantiateItem(ViewGroup container, int position)
        {
            if (position == _walletFacade.BalanceCount)
            {
                if (_shimmerLoading == null)
                {
                    _shimmerLoading = LayoutInflater.From(_pager.Context).Inflate(Resource.Layout.lyt_wallet_card_shimmer, container, false);

                    var shimmerContainer = _shimmerLoading.FindViewById<ShimmerLayout>(Resource.Id.shimmer_container);
                    shimmerContainer.SetShimmerColor(Color.Argb(80, 255, 255, 255));
                    shimmerContainer.SetMaskWidth(0.8f);
                    shimmerContainer.StartShimmerAnimation();

                    var shimmerBalanceTitle = _shimmerLoading.FindViewById<ShimmerLayout>(Resource.Id.shimmer_balance_title);
                    shimmerBalanceTitle.SetShimmerColor(Color.White);
                    shimmerBalanceTitle.SetMaskWidth(0.8f);
                    shimmerBalanceTitle.StartShimmerAnimation();

                    var shimmerBalance = _shimmerLoading.FindViewById<ShimmerLayout>(Resource.Id.shimmer_balance);
                    shimmerBalance.SetShimmerColor(Color.White);
                    shimmerBalance.SetMaskWidth(0.8f);
                    shimmerBalance.StartShimmerAnimation();

                    var shimmerTokenBalanceTitle1 = _shimmerLoading.FindViewById<ShimmerLayout>(Resource.Id.shimmer_token_balance_title1);
                    shimmerTokenBalanceTitle1.SetShimmerColor(Color.White);
                    shimmerTokenBalanceTitle1.SetMaskWidth(0.8f);
                    shimmerTokenBalanceTitle1.StartShimmerAnimation();

                    var shimmerTokenBalance1 = _shimmerLoading.FindViewById<ShimmerLayout>(Resource.Id.shimmer_token_balance1);
                    shimmerTokenBalance1.SetShimmerColor(Color.White);
                    shimmerTokenBalance1.SetMaskWidth(0.8f);
                    shimmerTokenBalance1.StartShimmerAnimation();

                    var shimmerTokenBalanceTitle2 = _shimmerLoading.FindViewById<ShimmerLayout>(Resource.Id.shimmer_token_balance_title2);
                    shimmerTokenBalanceTitle2.SetShimmerColor(Color.White);
                    shimmerTokenBalanceTitle2.SetMaskWidth(0.8f);
                    shimmerTokenBalanceTitle2.StartShimmerAnimation();

                    var shimmerTokenBalance2 = _shimmerLoading.FindViewById<ShimmerLayout>(Resource.Id.shimmer_token_balance2);
                    shimmerTokenBalance2.SetShimmerColor(Color.White);
                    shimmerTokenBalance2.SetMaskWidth(0.8f);
                    shimmerTokenBalance2.StartShimmerAnimation();

                    container.AddView(_shimmerLoading);
                }

                return _shimmerLoading;
            }

            var cardId = position % CachedPagesCount;
            if (_holders[cardId] == null)
            {
                var itemView = LayoutInflater.From(_pager.Context).Inflate(Resource.Layout.lyt_wallet_card, container, false);
                _holders[cardId] = new TokenCardHolder(itemView);
                container.AddView(itemView);
            }

            if (_walletFacade.BalanceCount > 0)
                UpdateData(position);

            return _holders[cardId].ItemView;
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            if (@object == _shimmerLoading)
            {
                container.RemoveView(_shimmerLoading);
                _shimmerLoading.Dispose();
                _shimmerLoading = null;
            }
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return @object == view;
        }

        public override int GetItemPosition(Object @object)
        {
            if (@object != _shimmerLoading)
                return PositionUnchanged;
            return PositionNone;
        }

        public void UpdateData(int position)
        {
            var i = position;
            position %= CachedPagesCount;

            foreach (var wallet in _walletFacade.Wallets)
            {
                if (wallet.UserInfo.AccountInfo.Balances.Length <= i)
                {
                    i -= wallet.UserInfo.AccountInfo.Balances.Length;
                    continue;
                }

                var balance = wallet.UserInfo.AccountInfo.Balances[i];
                var cr = _walletFacade.GetCurrencyRate(balance.CurrencyType);
                _holders[position]?.UpdateData(wallet.UserInfo, balance, cr, position);
                break;
            }
        }
        
        //public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
        //{
        //    base.OnDetachedFromRecyclerView(recyclerView);
        //    _holders.ForEach(h => h.OnDetached());
        //}
    }

    public class TokenCardHolder : RecyclerView.ViewHolder
    {
        public BalanceModel Balance { get; private set; }
        private CurrencyRate _currencyRate { get; set; }
        private readonly ImageView _holderImage;
        private readonly TextView _username;
        private readonly TextView _balanceTitle;
        private readonly TextView _balance;
        private readonly TextView _tokenBalanceTitle;
        private readonly TextView _tokenBalance;
        private readonly TextView _tokenBalanceTitle2;
        private readonly TextView _tokenBalance2;
        private readonly int _cornerRadius;

        public TokenCardHolder(View itemView) : base(itemView)
        {
            _cornerRadius = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 16, ItemView.Resources.DisplayMetrics);
            _holderImage = itemView.FindViewById<ImageView>(Resource.Id.token_logo);
            _username = itemView.FindViewById<TextView>(Resource.Id.login);
            _balanceTitle = itemView.FindViewById<TextView>(Resource.Id.balance_title);
            _balance = itemView.FindViewById<TextView>(Resource.Id.balance);
            _tokenBalanceTitle = itemView.FindViewById<TextView>(Resource.Id.token_balance_title);
            _tokenBalance = itemView.FindViewById<TextView>(Resource.Id.token_balance);
            _tokenBalanceTitle2 = itemView.FindViewById<TextView>(Resource.Id.token_balance_title2);
            _tokenBalance2 = itemView.FindViewById<TextView>(Resource.Id.token_balance2);

            _username.Typeface = Style.Semibold;
            _balanceTitle.Typeface = Style.Semibold;
            _balance.Typeface = Style.Semibold;
            _tokenBalanceTitle.Typeface = Style.Semibold;
            _tokenBalance.Typeface = Style.Semibold;
            _tokenBalanceTitle2.Typeface = Style.Semibold;
            _tokenBalance2.Typeface = Style.Semibold;
        }

        public void UpdateData(UserInfo userInfo, BalanceModel balance, CurrencyRate currencyRate, int position)
        {
            if (Balance != null)
                Balance.PropertyChanged -= OnPropertyChanged;

            Balance = balance;
            Balance.PropertyChanged += OnPropertyChanged;

            _currencyRate = currencyRate;
            SetBalance(Balance);

            var firstCardId = Resource.Drawable.wallet_card_bg1;
            var dr = RoundedBitmapDrawableFactory.Create(ItemView.Resources, BitmapFactory.DecodeResource(ItemView.Resources, firstCardId + position));
            dr.CornerRadius = _cornerRadius;
            _holderImage.SetImageDrawable(dr);

            _username.Text = $"@{userInfo.Login}";
        }


        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var balanceModel = (BalanceModel)sender;
            if (balanceModel != Balance)
                return;

            SetBalance(balanceModel);
        }

        private void SetBalance(BalanceModel balance)
        {
            var usdBalance = 0d;
            switch (balance.CurrencyType)
            {
                case CurrencyType.Steem:
                case CurrencyType.Golos:
                    {
                        _balanceTitle.Text = $"{balance.CurrencyType.ToString()} {App.Localization.GetText(LocalizationKeys.Balance).ToLower()}";
                        _tokenBalanceTitle2.Text = $"{balance.CurrencyType.ToString()} Power".ToUpper();
                        usdBalance = (balance.Value + balance.EffectiveSp) * (_currencyRate?.UsdRate ?? 1);
                        break;
                    }
                case CurrencyType.Sbd:
                case CurrencyType.Gbg:
                    {
                        _balanceTitle.Text = $"{balance.CurrencyType.ToString().ToUpper()} {App.Localization.GetText(LocalizationKeys.Balance).ToLower()}";
                        _tokenBalanceTitle2.Visibility = ViewStates.Gone;
                        _tokenBalance2.Visibility = ViewStates.Gone;
                        usdBalance = balance.Value * (_currencyRate?.UsdRate ?? 1);
                        break;
                    }
            }

            _tokenBalanceTitle.Text = balance.CurrencyType.ToString().ToUpper();
            _tokenBalance.Text = balance.Value.ToBalanceValueString();
            _tokenBalance2.Text = balance.EffectiveSp.ToBalanceValueString();

            _balance.Text = $"$ {usdBalance.ToBalanceValueString()}".ToUpper();
        }

        public void OnDetached()
        {
            Balance.PropertyChanged -= OnPropertyChanged;
        }
    }
}