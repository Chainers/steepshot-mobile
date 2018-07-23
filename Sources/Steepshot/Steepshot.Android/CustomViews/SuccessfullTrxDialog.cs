using System;
using System.Globalization;
using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public class SuccessfullTrxDialog : BottomSheetDialog
    {
        private readonly string _recipient;
        private readonly string _amount;


        public SuccessfullTrxDialog(Context context, string recipient, string amount) : this(context)
        {
            _recipient = recipient;
            _amount = amount;
        }


        private SuccessfullTrxDialog(Context context) : base(context)
        {
        }

        public override void Show()
        {
            using (var dialogView = LayoutInflater.From(Context).Inflate(Resource.Layout.lyt_successfull_transaction, null))
            {
                dialogView.SetMinimumWidth((int)(Context.Resources.DisplayMetrics.WidthPixels * 0.8));

                var title = dialogView.FindViewById<TextView>(Resource.Id.title);
                title.Typeface = Style.Light;
                title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferSuccess);

                var recipient = dialogView.FindViewById<TextView>(Resource.Id.recipient);
                recipient.Typeface = Style.Semibold;
                recipient.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Recipient);

                var recipientName = dialogView.FindViewById<TextView>(Resource.Id.recipient_value);
                recipientName.Typeface = Style.Semibold;
                recipientName.Text = _recipient;

                var amount = dialogView.FindViewById<TextView>(Resource.Id.amount);
                amount.Typeface = Style.Semibold;
                amount.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Amount);

                var amountValue = dialogView.FindViewById<TextView>(Resource.Id.amount_value);
                amountValue.Typeface = Style.Semibold;
                amountValue.Text = _amount;

                var closeBtn = dialogView.FindViewById<Button>(Resource.Id.close);
                closeBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Close);
                closeBtn.Click += CloseBtnOnClick;

                SetContentView(dialogView);
                Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                var dialogPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
                Window.DecorView.SetPadding(dialogPadding, dialogPadding, dialogPadding, dialogPadding);
                base.Show();

                var bottomSheet = FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
                BottomSheetBehavior.From(bottomSheet).State = BottomSheetBehavior.StateExpanded;
            }
        }

        private void CloseBtnOnClick(object sender, EventArgs e)
        {
            Dismiss();
        }
    }
}