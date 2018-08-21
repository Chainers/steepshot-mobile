using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public sealed class ActionAlertDialog : AlertDialog.Builder
    {
        public Action AlertAction;
        private AlertDialog _alert;
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
            Create();
        }

        public override AlertDialog Create()
        {
            _alert = base.Create();
            var alertView = LayoutInflater.From(Context).Inflate(Resource.Layout.lyt_deletion_alert, null);

            var alertTitle = alertView.FindViewById<TextView>(Resource.Id.deletion_title);
            alertTitle.Text = _header;
            alertTitle.Typeface = Style.Semibold;

            var alertMessage = alertView.FindViewById<TextView>(Resource.Id.deletion_message);
            alertMessage.Text = _message;
            alertMessage.Typeface = Style.Light;
            alertMessage.Visibility = string.IsNullOrEmpty(_message) ? ViewStates.Gone : ViewStates.Visible;

            var alertCancel = alertView.FindViewById<Button>(Resource.Id.cancel);
            alertCancel.Text = _cancel;
            alertCancel.Click += AlertCancelOnClick;

            var alertDelete = alertView.FindViewById<Button>(Resource.Id.delete);
            alertDelete.Text = _alertAct;
            alertDelete.Click += AlertDeleteOnClick;

            _alert.SetCancelable(true);
            _alert.SetView(alertView);
            _alert.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            return _alert;
        }

        public override AlertDialog Show()
        {
            _alert.Show();
            return _alert;
        }

        private void AlertDeleteOnClick(object sender, EventArgs e)
        {
            _alert.Cancel();
            AlertAction?.Invoke();
        }

        private void AlertCancelOnClick(object sender, EventArgs e)
        {
            _alert.Cancel();
        }
    }
}