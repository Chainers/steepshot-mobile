using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Widget;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Extensions;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Android.Views;
using Android.Text;
using System.Globalization;
using Android.Content;
using Android.Views.InputMethods;
using Steepshot.Base;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Holders
{
    public class PromoteMainHolder : RecyclerView.ViewHolder
    {
        private EditText _amountTextField;
        private TextView _pickerLabel;
        private TextView _balanceLabel;
        private TextView _errorMessage;
        private Button _maxBtn;
        private ProgressBar _balanceLoader;

        public List<BalanceModel> Balances { get; private set; }
        public CurrencyType PickedCoin { get; private set; } = CurrencyType.Steem;
        public event Action CoinPickClick;

        private AccountInfoResponse _accountInfo;
        public AccountInfoResponse AccountInfo
        {
            get => _accountInfo;
            set
            {
                _accountInfo = value;
                SetBalance();
            }
        }

        public string AmountEdit => _amountTextField.Text;


        public PromoteMainHolder(View itemView) : base(itemView)
        {
            InitializeView();
        }

        private void InitializeView()
        {
            var promoteAmount = ItemView.FindViewById<TextView>(Resource.Id.promote_amount);
            promoteAmount.Text = App.Localization.GetText(LocalizationKeys.Amount);
            promoteAmount.Typeface = Style.Semibold;

            _balanceLabel = ItemView.FindViewById<TextView>(Resource.Id.user_balance);
            _balanceLabel.Typeface = Style.Semibold;
            _balanceLoader = ItemView.FindViewById<ProgressBar>(Resource.Id.balance_spinner);

            _amountTextField = ItemView.FindViewById<EditText>(Resource.Id.promote_amount_edit);
            _amountTextField.Typeface = Style.Light;
            _amountTextField.Hint = App.Localization.GetText(LocalizationKeys.TransferAmountHint);
            _amountTextField.TextChanged += IsEnoughBalance;

            _errorMessage = ItemView.FindViewById<TextView>(Resource.Id.promote_errormessage);
            _errorMessage.Typeface = Style.Semibold;

            _pickerLabel = ItemView.FindViewById<TextView>(Resource.Id.promotecoin_name);
            _pickerLabel.Text = PickedCoin.ToString();
            _pickerLabel.Typeface = Style.Semibold;

            var promoteCoin = ItemView.FindViewById<LinearLayout>(Resource.Id.promote_coin);
            promoteCoin.Click += PromoteCoinOnClick;

            _maxBtn = ItemView.FindViewById<Button>(Resource.Id.promote_max);
            _maxBtn.Typeface = Style.Semibold;
            _maxBtn.Text = App.Localization.GetText(LocalizationKeys.Max);
            _maxBtn.Click += MaxBtnOnClick;

            SetBalance();
        }

        private void MaxBtnOnClick(object sender, EventArgs e)
        {
            _amountTextField.Text = Balances.Find(x => x.CurrencyType == PickedCoin).Value.ToBalanceValueString();
            _amountTextField.SetSelection(_amountTextField.Text.Length);
        }

        private void PromoteCoinOnClick(object sender, EventArgs e)
        {
            HideKeyboard();
            CoinPickClick?.Invoke();
        }

        public void UpdateTokenInfo(CurrencyType currencyType)
        {
            PickedCoin = currencyType;
            _pickerLabel.Text = PickedCoin.ToString();

            SetBalance();
        }

        public void ShowError(string mes)
        {
            _errorMessage.Visibility = ViewStates.Visible;
            _errorMessage.Text = mes;
        }

        private void HideKeyboard()
        {
            var imm = (InputMethodManager)ItemView.Context.GetSystemService(Context.InputMethodService);
            imm?.HideSoftInputFromWindow(_amountTextField.WindowToken, 0);
        }

        private void SetBalance()
        {
            if (_accountInfo == null)
            {
                _balanceLabel.Visibility = ViewStates.Gone;
                _balanceLoader.Visibility = ViewStates.Visible;
                _maxBtn.Enabled = false;
            }
            else
            {
                Balances = _accountInfo.Balances;
                var balance = Balances?.Find(x => x.CurrencyType == PickedCoin);
                _balanceLabel.Text = $"{App.Localization.GetText(LocalizationKeys.Balance)}: {balance?.Value}";

                _balanceLabel.Visibility = ViewStates.Visible;
                _balanceLoader.Visibility = ViewStates.Gone;
                _maxBtn.Enabled = true;
            }
            CoinSelected();
        }

        private void CoinSelected()
        {
            switch (PickedCoin)
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
            _amountTextField.SetPadding(_amountTextField.PaddingLeft, _amountTextField.PaddingTop, ((View)_pickerLabel.Parent).Width, _amountTextField.PaddingBottom);
        }

        private void IsEnoughBalance(object sender, TextChangedEventArgs e)
        {
            _errorMessage.Visibility = ViewStates.Gone;

            if (string.IsNullOrEmpty(_amountTextField.Text))
                return;

            if (!double.TryParse(_amountTextField.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amountEdit))
                return;

            if (amountEdit <= 0)
                return;

            if (amountEdit < Core.Constants.MinBid)
            {
                _errorMessage.Visibility = ViewStates.Visible;
                _errorMessage.Text = $"{App.Localization.GetText(LocalizationKeys.MinBid)} {Core.Constants.MinBid}";
            }
            else if (amountEdit > Core.Constants.MaxBid)
            {
                _errorMessage.Visibility = ViewStates.Visible;
                _errorMessage.Text = $"{App.Localization.GetText(LocalizationKeys.MaxBid)} {Core.Constants.MaxBid}";
            }
        }
    }
}
