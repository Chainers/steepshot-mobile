using System;
using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public class WalletExtraDialog : BottomSheetDialog
    {
        public Action<PowerAction> ExtraAction;
        private readonly BalanceModel _balance;

        public WalletExtraDialog(Context context, BalanceModel balance) : base(context)
        {
            _balance = balance;
        }

        public override void Show()
        {
            using (var dialogView = LayoutInflater.From(Context).Inflate(Resource.Layout.lyt_wallet_extra, null))
            {
                dialogView.SetMinimumWidth((int)(Style.ScreenWidth * 0.8));

                var dialogTitle = dialogView.FindViewById<TextView>(Resource.Id.title);
                dialogTitle.Typeface = Style.Semibold;
                dialogTitle.Text = App.Localization.GetText(LocalizationKeys.SelectAction);

                var porwerUp = dialogView.FindViewById<Button>(Resource.Id.power_up);
                porwerUp.Typeface = Style.Semibold;
                porwerUp.Text = App.Localization.GetText(LocalizationKeys.PowerUp);
                porwerUp.Click += PorwerUpOnClick;

                var porwerDown = dialogView.FindViewById<Button>(Resource.Id.power_down);
                porwerDown.Typeface = Style.Semibold;
                porwerDown.Text = App.Localization.GetText(LocalizationKeys.PowerDown);
                porwerDown.Click += PorwerDownOnClick;
                porwerDown.Visibility = _balance.EffectiveSp - _balance.DelegatedToMe - 5 > 0 ? ViewStates.Visible : ViewStates.Gone;

                var cancelPorwerDown = dialogView.FindViewById<Button>(Resource.Id.cancel_power_down);
                cancelPorwerDown.Typeface = Style.Semibold;
                cancelPorwerDown.Text = App.Localization.GetText(LocalizationKeys.CancelPowerDown);
                cancelPorwerDown.Click += CancelPorwerDownOnClick;
                cancelPorwerDown.Visibility = _balance.ToWithdraw > 0 ? ViewStates.Visible : ViewStates.Gone;

                var cancel = dialogView.FindViewById<Button>(Resource.Id.cancel);
                cancel.Typeface = Style.Semibold;
                cancel.Text = App.Localization.GetText(LocalizationKeys.Cancel);
                cancel.Click += CancelOnClick;

                SetContentView(dialogView);
                Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                var dialogPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
                Window.DecorView.SetPadding(dialogPadding, dialogPadding, dialogPadding, dialogPadding);
                base.Show();

                var bottomSheet = FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
                BottomSheetBehavior.From(bottomSheet).State = BottomSheetBehavior.StateExpanded;
            }
        }

        private void PorwerUpOnClick(object sender, EventArgs e)
        {
            Dismiss();
            ExtraAction?.Invoke(PowerAction.PowerUp);
        }

        private void PorwerDownOnClick(object sender, EventArgs e)
        {
            Dismiss();
            ExtraAction?.Invoke(PowerAction.PowerDown);
        }

        private void CancelPorwerDownOnClick(object sender, EventArgs e)
        {
            Dismiss();
            ExtraAction?.Invoke(PowerAction.CancelPowerDown);
        }

        private void CancelOnClick(object sender, EventArgs e)
        {
            Dismiss();
        }
    }
}