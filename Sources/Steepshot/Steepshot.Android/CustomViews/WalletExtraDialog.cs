using System;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public class WalletExtraDialog : BottomSheetDialog
    {
        public Action PowerUp;
        public Action PowerDown;
        private readonly BaseActivity _baseActivityContext;

        public WalletExtraDialog(BaseActivity context) : base(context)
        {
            _baseActivityContext = context;
        }

        public override void Show()
        {
            using (var dialogView = LayoutInflater.From(Context).Inflate(Resource.Layout.lyt_wallet_extra, null))
            {
                dialogView.SetMinimumWidth((int)(Context.Resources.DisplayMetrics.WidthPixels * 0.8));

                var dialogTitle = dialogView.FindViewById<TextView>(Resource.Id.title);
                dialogTitle.Typeface = Style.Semibold;
                dialogTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SelectAction);

                var porwerUp = dialogView.FindViewById<Button>(Resource.Id.power_up);
                porwerUp.Typeface = Style.Semibold;
                porwerUp.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PowerUp);
                porwerUp.Click += PorwerUpOnClick;

                var porwerDown = dialogView.FindViewById<Button>(Resource.Id.power_down);
                porwerDown.Typeface = Style.Semibold;
                porwerDown.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PowerDown);
                porwerDown.Click += PorwerDownOnClick;

                var cancel = dialogView.FindViewById<Button>(Resource.Id.cancel);
                cancel.Typeface = Style.Semibold;
                cancel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel);
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
            PowerUp?.Invoke();
        }

        private void PorwerDownOnClick(object sender, EventArgs e)
        {
            Dismiss();
            PowerDown?.Invoke();
        }

        private void CancelOnClick(object sender, EventArgs e)
        {
            Dismiss();
        }
    }
}