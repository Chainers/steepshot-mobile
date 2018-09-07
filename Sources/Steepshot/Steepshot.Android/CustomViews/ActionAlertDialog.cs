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
        private readonly string _headerText;
        private readonly string _messageText;
        private readonly string _alertActText;
        private readonly string _cancelText;
        private Orientation _orientation;

        public ActionAlertDialog(Context context, string headerText, string messageText, string alertActText, string cancelText, Orientation orientation = Orientation.Horizontal) : base(context)
        {
            _headerText = headerText;
            _messageText = messageText;
            _alertActText = alertActText;
            _cancelText = cancelText;
            _orientation = orientation;
        }

        public override void Show()
        {
            var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            using (var dialogView = inflater.Inflate(Resource.Layout.lyt_custom_alert, null))
            {
                dialogView.SetMinimumWidth((int)(Style.ScreenWidth * 0.8));

                var alertTitle = dialogView.FindViewById<TextView>(Resource.Id.alert_title);
                alertTitle.Text = _headerText;
                alertTitle.Typeface = string.IsNullOrEmpty(_messageText) ? Style.Regular : Style.Semibold;
                alertTitle.Visibility = string.IsNullOrEmpty(_headerText) ? ViewStates.Gone : ViewStates.Visible;

                var alertMessage = dialogView.FindViewById<TextView>(Resource.Id.alert_message);
                alertMessage.Text = _messageText;
                alertMessage.Typeface = Style.Light;
                alertMessage.Visibility = string.IsNullOrEmpty(_messageText) ? ViewStates.Gone : ViewStates.Visible;

                var isHorizontal = _orientation.Equals(Orientation.Horizontal);

                var layout = isHorizontal ? dialogView.FindViewById<LinearLayout>(Resource.Id.horizontal_lyt)
                                          : dialogView.FindViewById<LinearLayout>(Resource.Id.vertical_lyt);
                layout.Visibility = ViewStates.Visible;

                var alertCancel = isHorizontal ? dialogView.FindViewById<Button>(Resource.Id.cancel_h) 
                                               : dialogView.FindViewById<Button>(Resource.Id.cancel_v);
                alertCancel.Text = _cancelText;
                alertCancel.Click += (sender, e) => { Cancel(); };

                var actionBtn = isHorizontal ? dialogView.FindViewById<Button>(Resource.Id.alert_action_btn_h)
                                             : dialogView.FindViewById<Button>(Resource.Id.alert_action_btn_v);
                actionBtn.Text = _alertActText;
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