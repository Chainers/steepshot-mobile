using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Object = Java.Lang.Object;

namespace Steepshot.Adapter
{
    public class WalletPagerAdapter : Android.Support.V4.View.PagerAdapter, ViewPager.IPageTransformer
    {
        private readonly ViewPager _pager;
        private readonly List<View> _tokenCards;

        public WalletPagerAdapter(ViewPager pager)
        {
            _pager = pager;
            _tokenCards = new List<View>();
            _tokenCards.AddRange(Enumerable.Repeat<View>(null, Count));
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            if (_tokenCards[position] == null)
            {
                var itemView = LayoutInflater.From(_pager.Context).Inflate(Resource.Layout.lyt_wallet_card, container, false);
                var tokenLogo = itemView.FindViewById<ImageView>(Resource.Id.token_logo);
                var dr = RoundedBitmapDrawableFactory.Create(_pager.Resources, BitmapFactory.DecodeResource(_pager.Resources, Resource.Drawable.ic_eos_logo));
                dr.CornerRadius = TypedValue.ApplyDimension(ComplexUnitType.Dip, 16, _pager.Resources.DisplayMetrics);
                tokenLogo.SetImageDrawable(dr);
                _tokenCards[position] = itemView;
            }
            container.AddView(_tokenCards[position]);

            return _tokenCards[position];
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            container.RemoveView(_tokenCards[position]);
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return @object == view;
        }

        public override int GetItemPosition(Object @object)
        {
            return PositionUnchanged;
        }

        public override int Count => 6;

        public void TransformPage(View page, float position)
        {

        }
    }
}