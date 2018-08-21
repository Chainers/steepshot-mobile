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
    public sealed class PromoteAlertDialog : AlertDialog.Builder
    {
        private AlertDialog _alert;

        public PromoteAlertDialog(Context context) : base(context)
        {
            Create();
        }

        public override AlertDialog Create()
        {
            _alert = base.Create();
            var alertView = LayoutInflater.From(Context).Inflate(Resource.Layout.lyt_promote_popup, null);

            var promoteTitle = alertView.FindViewById<TextView>(Resource.Id.promote_title);
            promoteTitle.Text = "Promote post";
            promoteTitle.Typeface = Style.Semibold;

            var promoteAmount = alertView.FindViewById<TextView>(Resource.Id.promote_amount);
            promoteAmount.Text = "Amount";
            promoteAmount.Typeface = Style.Semibold;

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
    }
}
