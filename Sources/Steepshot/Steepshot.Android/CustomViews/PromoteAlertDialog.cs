using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public sealed class PromoteAlertDialog : AlertDialog.Builder
    {
        private TextView _balanceLabel;
        private CurrencyType _pickedCoin = CurrencyType.Steem;
        private AlertDialog _alert;
        private BasePostPresenter _presenter;
        private List<BalanceModel> _balances;

        public PromoteAlertDialog(Context context, BasePostPresenter presenter) : base(context)
        {
            _presenter = presenter;
            Create();
        }

        public override AlertDialog Create()
        {
            _alert = base.Create();
            var alertView = LayoutInflater.From(Context).Inflate(Resource.Layout.lyt_promote_popup, null);

            var promoteTitle = alertView.FindViewById<TextView>(Resource.Id.promote_title);
            promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromotePost);
            promoteTitle.Typeface = Style.Semibold;

            var promoteAmount = alertView.FindViewById<TextView>(Resource.Id.promote_amount);
            promoteAmount.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Amount);
            promoteAmount.Typeface = Style.Semibold;

            _balanceLabel = alertView.FindViewById<TextView>(Resource.Id.user_balance);
            _balanceLabel.Typeface = Style.Semibold;

            _alert.SetCancelable(true);
            _alert.SetView(alertView);
            _alert.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

            GetBalance();

            return _alert;
        }

        private async void GetBalance()
        {
            var response = await _presenter.TryGetAccountInfo(AppSettings.User.Login);

            if (response.IsSuccess)
            {
                _balances = response.Result?.Balances;
                var balance = _balances?.Find(x => x.CurrencyType == _pickedCoin);
                _balanceLabel.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.Balance)}: {balance.Value}";
            }
        }

        public override AlertDialog Show()
        {
            _alert.Show();
            return _alert;
        }
    }
}
