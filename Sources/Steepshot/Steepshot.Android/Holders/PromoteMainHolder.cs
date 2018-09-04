using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Widget;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Extensions;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Android.Views;
using Android.Text;
using System.Globalization;
using Steepshot.Core.Models.Enums;
using Android.Content;

namespace Steepshot.Holders
{
    public class PromoteMainHolder : RecyclerView.ViewHolder
    {
        private readonly BasePostPresenter presenter;

        private EditText _amountTextField;
        private TextView _pickerLabel;
        private TextView _balanceLabel;
        private TextView _errorMessage;
        private Button _maxBtn;
        private ProgressBar _balanceLoader;

        public List<BalanceModel> balances;
        public Action<ActionType> PromoteAction;
        public CurrencyType pickedCoin = CurrencyType.Steem;

        public string AmountEdit => _amountTextField.Text;


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

            _amountTextField = ItemView.FindViewById<EditText>(Resource.Id.promote_amount_edit);
            _amountTextField.Typeface = Style.Light;
            _amountTextField.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferAmountHint);
            _amountTextField.TextChanged += IsEnoughBalance;

            _errorMessage = ItemView.FindViewById<TextView>(Resource.Id.promote_errormessage);
            _errorMessage.Typeface = Style.Semibold;

            _pickerLabel = ItemView.FindViewById<TextView>(Resource.Id.promotecoin_name);
            _pickerLabel.Text = "Steem";
            _pickerLabel.Typeface = Style.Semibold;

            var promoteCoin = ItemView.FindViewById<LinearLayout>(Resource.Id.promote_coin);
            promoteCoin.Click += (sender, e) =>
            {
                PromoteAction?.Invoke(ActionType.PickCoin);
            };

            _maxBtn = ItemView.FindViewById<Button>(Resource.Id.promote_max);
            _maxBtn.Typeface = Style.Semibold;
            _maxBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Max);
            _maxBtn.Click += (sender, e) =>
            {
                _amountTextField.Text = balances.Find(x => x.CurrencyType == pickedCoin).Value.ToBalanceValueString();
                _amountTextField.SetSelection(_amountTextField.Text.Length);
            };

            GetBalance();
        }

        public void UpdateTokenInfo(CurrencyType currencyType)
        {
            pickedCoin = currencyType;
            _pickerLabel.Text = pickedCoin.ToString();

            GetBalance();
        }

        public void ShowError(string mes)
        {
            _errorMessage.Visibility = ViewStates.Visible;
            _errorMessage.Text = mes;
        }

        private async void GetBalance()
        {
            _balanceLabel.Visibility = ViewStates.Gone;
            _balanceLoader.Visibility = ViewStates.Visible;
            _maxBtn.Enabled = false;

            var response = await presenter.TryGetAccountInfo(AppSettings.User.Login);

            if (response.IsSuccess)
            {
                balances = response.Result?.Balances;
                var balance = balances?.Find(x => x.CurrencyType == pickedCoin);
                _balanceLabel.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.Balance)}: {balance.Value}";
            }

            _balanceLabel.Visibility = ViewStates.Visible;
            _balanceLoader.Visibility = ViewStates.Gone;
            _maxBtn.Enabled = true;

            CoinSelected();
        }
        
        private void CoinSelected()
        { 
            switch (pickedCoin)
            {
                case CurrencyType.Steem:
                case CurrencyType.Golos:
                    _amountTextField.SetFilters(new IInputFilter[] { new TransferAmountFilter(20, 3) });
                    break;
                case CurrencyType.Sbd:
                case CurrencyType.Gbg:
                    _amountTextField.SetFilters(new IInputFilter[] { new TransferAmountFilter(20, 6) });
                    break;
            }
            _amountTextField.SetPadding(_amountTextField.PaddingLeft, _amountTextField.PaddingTop, ((View) _pickerLabel.Parent).Width, _amountTextField.PaddingBottom);
        }

        private void IsEnoughBalance(object sender, TextChangedEventArgs e)
        {
            _errorMessage.Visibility = ViewStates.Gone;

            if (string.IsNullOrEmpty(_amountTextField.Text))
                return;

            if (!double.TryParse(_amountTextField.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amountEdit))
                return;

            if (amountEdit == 0)
                return;

            if (amountEdit < Core.Constants.MinBid)
            {
                _errorMessage.Visibility = ViewStates.Visible;
                _errorMessage.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.MinBid)} {Core.Constants.MinBid}";
            }
            else if (amountEdit > Core.Constants.MaxBid)
            {
                _errorMessage.Visibility = ViewStates.Visible;
                _errorMessage.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.MaxBid)} {Core.Constants.MaxBid}";
            }
        }

        private bool IsValidAmount()
        {
            if (!double.TryParse(_amountTextField.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amountEdit))
                return false;

            return amountEdit >= Core.Constants.MinBid && amountEdit <= Core.Constants.MaxBid;
        }
    }
}
