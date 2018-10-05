using System.Collections.Generic;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.Holders;

namespace Steepshot.Adapter
{
    public class PromotePagerAdapter : Android.Support.V4.View.PagerAdapter
    {
        private readonly Context _context;
        private readonly CurrencyType _pickedCoin;

        public List<CurrencyType> Coins { get; }
        public PromotePickerHolder PickerHolder { get; private set; }
        public PromoteMainHolder MainHolder { get; private set; }
        public PromoterFoundHolder FoundHolder { get; private set; }
        public PromoteMessageHolder MessageHolder { get; private set; }

        public override int Count => 4;

        public override int GetItemPosition(Java.Lang.Object @object) => PositionNone;

        public PromotePagerAdapter(Context context)
        {
            _context = context;

            Coins = new List<CurrencyType>();
            switch (App.User.Chain)
            {
                case KnownChains.Steem:
                    Coins.AddRange(new[] { CurrencyType.Steem, CurrencyType.Sbd });
                    break;
                case KnownChains.Golos:
                    Coins.AddRange(new[] { CurrencyType.Golos, CurrencyType.Gbg });
                    break;
            }
            _pickedCoin = Coins[0];
        }

        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
        {
            var inflater = LayoutInflater.From(_context);
            View itemView;

            switch (position)
            {
                case 0:
                    itemView = inflater.Inflate(Resource.Layout.lyt_promote_picker, container, false);
                    PickerHolder = new PromotePickerHolder(itemView, Coins, Coins.IndexOf(_pickedCoin));
                    break;
                case 1:
                    itemView = inflater.Inflate(Resource.Layout.lyt_promote_main, container, false);
                    MainHolder = new PromoteMainHolder(itemView);
                    MainHolder.CoinPickClick += () => ((ViewPager)container).SetCurrentItem(0, true);
                    break;
                case 2:
                    itemView = inflater.Inflate(Resource.Layout.lyt_promote_searching, container, false);
                    FoundHolder = new PromoterFoundHolder(itemView);
                    break;
                default:
                    itemView = inflater.Inflate(Resource.Layout.lyt_promote_message, container, false);
                    MessageHolder = new PromoteMessageHolder(itemView);
                    break;
            }

            container.AddView(itemView);
            return itemView;
        }

        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
        {
            container.RemoveView((View)@object);
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object @object)
        {
            return view == @object;
        }
    }
}
