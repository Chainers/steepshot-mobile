using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Utils;

namespace Steepshot.Holders
{
    public class PromoteMessageHolder : RecyclerView.ViewHolder
    {
        private TextView title;
        private TextView message;

        public PromoteMessageHolder(View itemView) : base(itemView)
        {
            InitializeView();
        }

        private void InitializeView()
        {
            title = ItemView.FindViewById<TextView>(Resource.Id.message_title);
            title.Typeface = Style.Regular;

            message = ItemView.FindViewById<TextView>(Resource.Id.message_body);
            message.Typeface = Style.Regular;
        }

        public void SetupMessage(string titleText, string messageText)
        {
            title.Text = titleText;

            if (!string.IsNullOrEmpty(messageText))
                message.Text = messageText;
            else
                message.Visibility = ViewStates.Gone;
        }
    }
}
