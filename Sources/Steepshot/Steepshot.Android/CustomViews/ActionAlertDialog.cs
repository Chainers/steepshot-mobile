using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public sealed class ActionAlertDialog : BottomSheetDialog
    {
        public Action AlertAction;
        private readonly string _header;
        private readonly string _message;
        private readonly string _alertAct;
        private readonly string _cancel;

        public ActionAlertDialog(Context context, string header, string message, string alertAct, string cancel) : base(context)
        {
            _header = header;
            _message = message;
            _alertAct = alertAct;
            _cancel = cancel;
        }

        public override void Show()
        {
            var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            using (var dialogView = inflater.Inflate(Resource.Layout.lyt_custom_alert, null))
            {
                dialogView.SetMinimumWidth((int)(Style.ScreenWidth * 0.8));

                var alertTitle = dialogView.FindViewById<TextView>(Resource.Id.alert_title);
                alertTitle.Text = _header;
                alertTitle.Typeface = Style.Semibold;

                var alertMessage = dialogView.FindViewById<TextView>(Resource.Id.alert_message);
                alertMessage.Text = _message;
                alertMessage.Typeface = Style.Light;
                alertMessage.Visibility = string.IsNullOrEmpty(_message) ? ViewStates.Gone : ViewStates.Visible;

                var alertCancel = dialogView.FindViewById<Button>(Resource.Id.cancel);
                alertCancel.Text = _cancel;
                alertCancel.Click += (sender, e) => { Cancel(); };

                var actionBtn = dialogView.FindViewById<Button>(Resource.Id.alert_action_btn);
                actionBtn.Text = _alertAct;
                actionBtn.Click += OnButtonAction;

                SetContentView(dialogView);
                Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                var dialogPadding = (int)Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
                Window.DecorView.SetPadding(dialogPadding, dialogPadding, dialogPadding, dialogPadding);
                base.Show();

                var bottomSheet = FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
                BottomSheetBehavior.From(bottomSheet).State = BottomSheetBehavior.StateExpanded;
            }
        }

        private void OnButtonAction(object sender, EventArgs e)
        {
            Cancel();
            AlertAction?.Invoke();
        }
    }
}