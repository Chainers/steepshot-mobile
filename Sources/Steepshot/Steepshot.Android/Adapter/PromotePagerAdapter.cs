using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Steepshot.Core;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Holders;

namespace Steepshot.Adapter
{
    public class PromotePagerAdapter : Android.Support.V4.View.PagerAdapter
    {
        private readonly Context context;
        private readonly BasePostPresenter presenter;
        private readonly Action<ActionType> promoteAction;
        private readonly Action<CurrencyType> coinAction;
        private CurrencyType _pickedCoin;

        public List<CurrencyType> coins;
        public PromotePickerHolder pickerHolder;
        public PromoteMainHolder mainHolder;
        public PromoterFoundHolder foundHolder;

        public override int Count => 3;

        public override int GetItemPosition(Java.Lang.Object @object) => PositionNone;

        public PromotePagerAdapter(Context context, BasePostPresenter presenter, Action<ActionType> promoteAction)
        {
            this.context = context;
            this.presenter = presenter;
            this.promoteAction = promoteAction;

            coins = new List<CurrencyType>();
            switch (AppSettings.User.Chain)
            {
                case KnownChains.Steem:
                    coins.AddRange(new[] { CurrencyType.Steem, CurrencyType.Sbd });
                    break;
                case KnownChains.Golos:
                    coins.AddRange(new[] { CurrencyType.Golos, CurrencyType.Gbg });
                    break;
            }
            _pickedCoin = coins[0];
        }

        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
        {
            var inflater = (LayoutInflater)container.Context.GetSystemService(Context.LayoutInflaterService);
            var resId = 0;
            View view;
            
            switch (position)
            {
                case 0:
                    resId = Resource.Layout.lyt_promote_picker;
                    view = inflater.Inflate(resId, container, false);
                    pickerHolder = new PromotePickerHolder(view, context, coins, coins.IndexOf(_pickedCoin));
                    container.AddView(pickerHolder.ItemView);
                    return pickerHolder.ItemView;
                case 1:
                    resId = Resource.Layout.lyt_promote_main;
                    view = inflater.Inflate(resId, container, false);
                    mainHolder = new PromoteMainHolder(view, presenter)
                    {
                        PromoteAction = promoteAction
                    };
                    container.AddView(mainHolder.ItemView);
                    return mainHolder.ItemView;
                default:
                    resId = Resource.Layout.lyt_promote_searching;
                    view = inflater.Inflate(resId, container, false);
                    foundHolder = new PromoterFoundHolder(view, context);
                    container.AddView(foundHolder.ItemView);
                    return foundHolder.ItemView;
            }
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
