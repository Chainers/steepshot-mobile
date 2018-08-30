using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Extensions;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class PromotePagerAdapter : Android.Support.V4.View.PagerAdapter
    {
        private readonly Context context;
        private readonly BasePostPresenter presenter;

        public override int Count => 3;

        public override int GetItemPosition(Java.Lang.Object @object) => PositionNone;

        public PromotePagerAdapter(Context context, BasePostPresenter presenter)
        {
            this.context = context;
            this.presenter = presenter;
        }

        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
        {
            var inflater = (LayoutInflater)container.Context.GetSystemService(Context.LayoutInflaterService);
            var resId = 0;

            PromoteMainHolder viewHolder = null;

            switch (position)
            {
                case 0:
                    resId = Resource.Layout.lyt_promote_picker;
                    break;
                case 1:
                    resId = Resource.Layout.lyt_promote_main;
                    var view = inflater.Inflate(resId, container, false);
                    viewHolder = new PromoteMainHolder(view, presenter);
                    container.AddView(viewHolder.ItemView);
                    break;
                default:
                    resId = Resource.Layout.lyt_promote_searching;
                    break;
            }

            return viewHolder?.ItemView;
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

    public class PromoteMainHolder : RecyclerView.ViewHolder
    {
        private readonly BasePostPresenter presenter;

        private List<BalanceModel> _balances;
        private CurrencyType _pickedCoin = CurrencyType.Steem;
        private TextView _balanceLabel;
        private ProgressBar _balanceLoader;

        public PromoteMainHolder(View itemView, BasePostPresenter presenter) : base(itemView)
        {
            this.presenter = presenter;
            InitializeView();
        }

        private void InitializeView()
        {
            var promoteAmount = ItemView.FindViewById<TextView>(Resource.Id.promote_amount);
            promoteAmount.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Amount);
            promoteAmount.Typeface = Style.Semibold;

            _balanceLabel = ItemView.FindViewById<TextView>(Resource.Id.user_balance);
            _balanceLabel.Typeface = Style.Semibold;
            _balanceLoader = ItemView.FindViewById<ProgressBar>(Resource.Id.balance_spinner);

            GetBalance();

            var amountTextField = ItemView.FindViewById<EditText>(Resource.Id.promote_amount_edit);
            amountTextField.Typeface = Style.Light;
            amountTextField.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferAmountHint);
            //amountTextField.TextChanged += IsEnoughBalance;

            var errorMessage = ItemView.FindViewById<TextView>(Resource.Id.promote_errormessage);
            errorMessage.Typeface = Style.Semibold;

            var pickerLabel = ItemView.FindViewById<TextView>(Resource.Id.promotecoin_name);
            pickerLabel.Text = "Steem";
            pickerLabel.Typeface = Style.Semibold;

            var promoteCoin = ItemView.FindViewById<LinearLayout>(Resource.Id.promote_coin);
            promoteCoin.Click += (sender, e) =>
            {

            };

            var maxBtn = ItemView.FindViewById<Button>(Resource.Id.promote_max);
            maxBtn.Typeface = Style.Semibold;
            maxBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Max);
            maxBtn.Click += (sender, e) =>
            {
                amountTextField.Text = _balances.Find(x => x.CurrencyType == _pickedCoin).Value.ToBalanceValueString();
                amountTextField.SetSelection(amountTextField.Text.Length);
            };
        }

        private async void GetBalance()
        {
            _balanceLabel.Visibility = ViewStates.Gone;
            _balanceLoader.Visibility = ViewStates.Visible;

            var response = await presenter.TryGetAccountInfo(AppSettings.User.Login);

            if (response.IsSuccess)
            {
                _balances = response.Result?.Balances;
                var balance = _balances?.Find(x => x.CurrencyType == _pickedCoin);
                _balanceLabel.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.Balance)}: {balance.Value}";
            }

            _balanceLabel.Visibility = ViewStates.Visible;
            _balanceLoader.Visibility = ViewStates.Gone;
        }
    }
}
