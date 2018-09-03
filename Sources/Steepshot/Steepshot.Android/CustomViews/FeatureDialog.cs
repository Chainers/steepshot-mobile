using System;
using Android.Content;
using Android.Views;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Graphics;
using Android.Widget;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public class FeatureDialog : BottomSheetDialog
    {
        public FeatureDialog(Context context) : base(context)
        {
        }

        public override void Show()
        {
            using (var dialogView = LayoutInflater.From(Context).Inflate(Resource.Layout.lyt_successfull_transaction, null))
            {
                dialogView.SetMinimumWidth((int)(Style.ScreenWidth * 0.8));

                // TODO: setup content
                // ...

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
