using System;
using System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Extensions;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Android.Support.Design.Widget;

namespace Steepshot.CustomViews
{
    public sealed class PromoteAlertDialog : BottomSheetDialog
    {
        private readonly Context context;

        private TextView _errorMessage;
        private TextView _balanceLabel;
        private ProgressBar _balanceLoader;
        private EditText _amountTextField;
        private Button _findBtn;
        private Button _maxBtn;
        private LinearLayout _promoteCoin;

        private CurrencyType _pickedCoin = CurrencyType.Steem;
        private BasePostPresenter _presenter;
        private List<BalanceModel> _balances;
        private ViewGroup _activityRoot;

        private bool EditEnabled
        {
            set
            {
                _amountTextField.Enabled = value;
                _findBtn.Enabled = value;
                _maxBtn.Enabled = value;
                _promoteCoin.Enabled = value;
            }
        }

        private PromoteAlertDialog(Context context) : base(context) { }

        public PromoteAlertDialog(Context context, BasePostPresenter presenter) : this(context)
        {
            this.context = context;
            _presenter = presenter;
        }

        public override void Show()
        {
            using (var dialogView = LayoutInflater.From(context).Inflate(Resource.Layout.lyt_promote_popup, null))
            {
                dialogView.SetMinimumWidth((int)(context.Resources.DisplayMetrics.WidthPixels * 0.8));

                var promoteTitle = dialogView.FindViewById<TextView>(Resource.Id.promote_title);
                promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromotePost);
                promoteTitle.Typeface = Style.Semibold;

                var promoteAmount = dialogView.FindViewById<TextView>(Resource.Id.promote_amount);
                promoteAmount.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Amount);
                promoteAmount.Typeface = Style.Semibold;

                _balanceLabel = dialogView.FindViewById<TextView>(Resource.Id.user_balance);
                _balanceLabel.Typeface = Style.Semibold;
                _balanceLoader = dialogView.FindViewById<ProgressBar>(Resource.Id.balance_spinner);
                GetBalance();

                _amountTextField = dialogView.FindViewById<EditText>(Resource.Id.promote_amount_edit);
                _amountTextField.Typeface = Style.Light;
                _amountTextField.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferAmountHint);
                _amountTextField.TextChanged += IsEnoughBalance;

                _errorMessage = dialogView.FindViewById<TextView>(Resource.Id.promote_errormessage);
                _errorMessage.Typeface = Style.Semibold;

                var pickerLabel = dialogView.FindViewById<TextView>(Resource.Id.promotecoin_name);
                pickerLabel.Text = "Steem";
                pickerLabel.Typeface = Style.Semibold;

                _promoteCoin = dialogView.FindViewById<LinearLayout>(Resource.Id.promote_coin);
                _promoteCoin.Click += (sender, e) => { };

                _maxBtn = dialogView.FindViewById<Button>(Resource.Id.promote_max);
                _maxBtn.Typeface = Style.Semibold;
                _maxBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Max);
                _maxBtn.Click += (sender, e) =>
                {
                    _amountTextField.Text = _balances.Find(x => x.CurrencyType == _pickedCoin).Value.ToBalanceValueString();
                    _amountTextField.SetSelection(_amountTextField.Text.Length);
                };

                _findBtn = dialogView.FindViewById<Button>(Resource.Id.findpromote_btn);
                _findBtn.Typeface = Style.Semibold;
                _findBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.FindPromoter);
                _findBtn.Typeface = Style.Semibold;

                var close = dialogView.FindViewById<Button>(Resource.Id.close);
                close.Typeface = Style.Semibold;
                close.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Close);
                close.Click += (sender, e) => { Cancel(); };

                SetContentView(dialogView);
                Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                var dialogPadding = (int)Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
                Window.DecorView.SetPadding(dialogPadding, dialogPadding, dialogPadding, dialogPadding);
                base.Show();

                var bottomSheet = FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
                BottomSheetBehavior.From(bottomSheet).State = BottomSheetBehavior.StateExpanded;
            }
        }

        private async void GetBalance()
        {
            _balanceLabel.Visibility = ViewStates.Gone;
            _balanceLoader.Visibility = ViewStates.Visible;

            var response = await _presenter.TryGetAccountInfo(AppSettings.User.Login);

            if (response.IsSuccess)
            {
                _balances = response.Result?.Balances;
                var balance = _balances?.Find(x => x.CurrencyType == _pickedCoin);
                _balanceLabel.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.Balance)}: {balance.Value}";
            }

            _balanceLabel.Visibility = ViewStates.Visible;
            _balanceLoader.Visibility = ViewStates.Gone;
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
    }
}
