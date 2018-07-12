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
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
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
        private List<BalanceModel> _balances;

        public WalletPagerAdapter(ViewPager pager)
        {
            _pager = pager;
            _pager.PageScrolled += PageScrolled;
            _tokenCards = new List<TokenCardHolder>();
            _tokenCards.AddRange(Enumerable.Repeat<TokenCardHolder>(null, CachedPagesCount));
        }

        public void UpdateData(List<BalanceModel> balances)
        {
            _balances = balances;
            NotifyDataSetChanged();
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            position %= CachedPagesCount;
            if (_tokenCards[position] == null)
            {
                var itemView = LayoutInflater.From(_pager.Context).Inflate(Resource.Layout.lyt_wallet_card, container, false);
                _tokenCards[position] = new TokenCardHolder(itemView);
                container.AddView(itemView);
            }
            _tokenCards[position].UpdateData(_balances[position]);

            return _tokenCards[position].ItemView;
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object) { }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return @object == view;
        }

        public override int GetItemPosition(Object @object)
        {
            return PositionUnchanged;
        }

        public override int Count => _balances?.Count ?? 0;

        private void PageScrolled(object sender, ViewPager.PageScrolledEventArgs e)
        {
            var scrollingCards = _tokenCards.FindAll(x => x != null);
            var leftOffset = _pager.PaddingLeft + _pager.PageMargin;
            scrollingCards.Sort((x, y) => ((Math.Abs(x.ItemView.Left - _pager.ScrollX - leftOffset) / (double)_pager.Width).CompareTo(Math.Abs(y.ItemView.Left - _pager.ScrollX - leftOffset) / (double)_pager.Width)));
            var realWidth = _pager.Width - _pager.PaddingLeft - _pager.PageMargin * 2;
            var visibilityPercentage = Math.Abs(scrollingCards[0].ItemView.Right - scrollingCards[0].ItemView.Left - _pager.ScrollX + _pager.PageMargin) / (float)scrollingCards[0].ItemView.Width;
            if (visibilityPercentage >= 0 && visibilityPercentage <= 1)
                OnPageTransforming?.Invoke(scrollingCards[0], scrollingCards[1], visibilityPercentage);
        }
    }

    public class TokenCardHolder : RecyclerView.ViewHolder
    {
        private readonly ImageView _holderImage;
        private readonly TextView _balanceTitle;
        private readonly TextView _balance;
        private readonly TextView _tokenBalanceTitle;
        private readonly TextView _tokenBalance;
        private readonly TextView _tokenBalanceTitle2;
        private readonly TextView _tokenBalance2;

        public TokenCardHolder(View itemView) : base(itemView)
        {
            _holderImage = itemView.FindViewById<ImageView>(Resource.Id.token_logo);
            _balanceTitle = itemView.FindViewById<TextView>(Resource.Id.balance_title);
            _balance = itemView.FindViewById<TextView>(Resource.Id.balance);
            _tokenBalanceTitle = itemView.FindViewById<TextView>(Resource.Id.token_balance_title);
            _tokenBalance = itemView.FindViewById<TextView>(Resource.Id.token_balance);
            _tokenBalanceTitle2 = itemView.FindViewById<TextView>(Resource.Id.token_balance_title2);
            _tokenBalance2 = itemView.FindViewById<TextView>(Resource.Id.token_balance2);

            _balanceTitle.Typeface = Style.Semibold;
            _balance.Typeface = Style.Semibold;
            _tokenBalanceTitle.Typeface = Style.Semibold;
            _tokenBalance.Typeface = Style.Semibold;
            _tokenBalanceTitle2.Typeface = Style.Semibold;
            _tokenBalance2.Typeface = Style.Semibold;

        }

        public void UpdateData(BalanceModel balance)
        {
            var dr = RoundedBitmapDrawableFactory.Create(ItemView.Resources, BitmapFactory.DecodeResource(ItemView.Resources, Resource.Drawable.wallet_card_bg1));
            dr.CornerRadius = TypedValue.ApplyDimension(ComplexUnitType.Dip, 16, ItemView.Resources.DisplayMetrics);
            _holderImage.SetImageDrawable(dr);
            switch (balance.CurrencyType)
            {
                case CurrencyType.Steem:
                    _balanceTitle.Text = "Accaount balance";
                    _balance.Text = "$ 99999.999";
                    _tokenBalanceTitle.Text = balance.CurrencyType.ToString();
                    _tokenBalance.Text = "999";
                    _tokenBalanceTitle2.Text = balance.CurrencyType.ToString();
                    _tokenBalance2.Text = "999";
                    break;
                case CurrencyType.Sbd:
                    break;
                case CurrencyType.Golos:
                    break;
                case CurrencyType.Gbg:
                    break;
                case CurrencyType.Eos:
                    break;
                case CurrencyType.Vim:
                    break;
            }
        }
    }
}