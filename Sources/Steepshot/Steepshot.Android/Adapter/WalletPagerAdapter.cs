using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using IO.SuperCharge.ShimmerLayoutLib;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Object = Java.Lang.Object;

namespace Steepshot.Adapter
{
    public class WalletPagerAdapter : Android.Support.V4.View.PagerAdapter
    {
        public Action<TokenCardHolder, TokenCardHolder, float> OnPageTransforming;
        private const int CachedPagesCount = 5;
        private readonly ViewPager _pager;
        private readonly List<TokenCardHolder> _tokenCards;
        private readonly WalletPresenter _presenter;
        private View _shimmerLoading;
        private int _itemsCount = 1;

        public WalletPagerAdapter(ViewPager pager, WalletPresenter presenter)
        {
            _pager = pager;
            _presenter = presenter;
            _pager.PageScrolled += PageScrolled;
            _tokenCards = new List<TokenCardHolder>();
            _tokenCards.AddRange(Enumerable.Repeat<TokenCardHolder>(null, CachedPagesCount));
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            position %= CachedPagesCount;

            if (position == _presenter.Balances.Count)
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

            if (_tokenCards[position] == null)
            {
                var itemView = LayoutInflater.From(_pager.Context).Inflate(Resource.Layout.lyt_wallet_card, container, false);
                _tokenCards[position] = new TokenCardHolder(itemView);
                container.AddView(itemView);
            }
            if (_presenter.Balances?.Count > 0)
                _tokenCards[position].UpdateData(_presenter.Balances[position], _presenter.GetCurrencyRate(_presenter.Balances[position].CurrencyType), position);

            return _tokenCards[position].ItemView;
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

        public override void NotifyDataSetChanged()
        {
            _itemsCount = _presenter.Balances.Count;
            if (_presenter.HasNext)
                _itemsCount += 1;
            _pager.ViewTreeObserver.GlobalLayout += CardsLayedOut;
            base.NotifyDataSetChanged();
        }

        public void NotifyItemChanged(int position)
        {
            position %= CachedPagesCount;
            _tokenCards[position]?.UpdateData(_presenter.Balances[position], _presenter.GetCurrencyRate(_presenter.Balances[position].CurrencyType), position);
        }

        private void CardsLayedOut(object sender, EventArgs e)
        {
            PageScrolled(null, null);
            _pager.ViewTreeObserver.GlobalLayout -= CardsLayedOut;
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

        public override int Count => _itemsCount;

        private void PageScrolled(object sender, ViewPager.PageScrolledEventArgs e)
        {
            var scrollingCards = _tokenCards.FindAll(x => x != null);
            if (!scrollingCards.Any())
                return;

            var leftOffset = _pager.PaddingLeft + _pager.PageMargin;
            scrollingCards.Sort((x, y) => (Math.Abs(x.ItemView.Left - _pager.ScrollX - leftOffset) / (double)_pager.Width).CompareTo(Math.Abs(y.ItemView.Left - _pager.ScrollX - leftOffset) / (double)_pager.Width));
            var visibilityPercentage = Math.Abs(scrollingCards[0].ItemView.Right - scrollingCards[0].ItemView.Left - Math.Abs(_pager.ScrollX)) / (float)scrollingCards[0].ItemView.Width;
            if (visibilityPercentage >= 0 && visibilityPercentage <= 1)
                OnPageTransforming?.Invoke(scrollingCards[0], scrollingCards[1], visibilityPercentage);
        }
    }

    public class TokenCardHolder : RecyclerView.ViewHolder
    {
        public BalanceModel Balance { get; private set; }
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

            _balanceTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.AccountBalance);
        }

        public void UpdateData(BalanceModel balance, CurrencyRate currencyRate, int position)
        {
            Balance = balance;
            var usdBalance = 0d;
            var firstCardId = Resource.Drawable.wallet_card_bg1;
            var dr = RoundedBitmapDrawableFactory.Create(ItemView.Resources, BitmapFactory.DecodeResource(ItemView.Resources, firstCardId + position));
            dr.CornerRadius = _cornerRadius;
            _holderImage.SetImageDrawable(dr);
            switch (balance.CurrencyType)
            {
                case CurrencyType.Steem:
                case CurrencyType.Golos:
                    {
                        _tokenBalanceTitle2.Text = $"{balance.CurrencyType.ToString()} Power".ToUpper();
                        usdBalance = (balance.Value + balance.EffectiveSp) * (currencyRate?.UsdRate ?? 1);
                        break;
                    }
                case CurrencyType.Sbd:
                case CurrencyType.Gbg:
                    {
                        _tokenBalanceTitle2.Visibility = ViewStates.Gone;
                        _tokenBalance2.Visibility = ViewStates.Gone;
                        usdBalance = balance.Value * (currencyRate?.UsdRate ?? 1);
                        break;
                    }
            }

            _username.Text = balance.UserInfo.Login.ToUpper();
            _balance.Text = $"$ {usdBalance.ToBalanceVaueString()}".ToUpper();
            _tokenBalanceTitle.Text = balance.CurrencyType.ToString().ToUpper();
            _tokenBalance.Text = balance.Value.ToBalanceVaueString();
            _tokenBalance2.Text = balance.EffectiveSp.ToBalanceVaueString();
        }
    }
}