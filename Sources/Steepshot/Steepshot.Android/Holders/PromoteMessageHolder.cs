using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Utils;

namespace Steepshot.Holders
{
    public class PromoteMessageHolder : RecyclerView.ViewHolder
    {
        private TextView _title;
        private TextView _message;

        public PromoteMessageHolder(View itemView) : base(itemView)
        {
            InitializeView();
        }

        private void InitializeView()
        {
            _title = ItemView.FindViewById<TextView>(Resource.Id.message_title);
            _title.Typeface = Style.Regular;

            _message = ItemView.FindViewById<TextView>(Resource.Id.message_body);
            _message.Typeface = Style.Regular;
        }

        public void SetupMessage(string titleText, string messageText)
        {
            _title.Text = titleText;

            if (!string.IsNullOrEmpty(messageText))
                _message.Text = messageText;
            else
                _message.Visibility = ViewStates.Gone;
        }
    }
}
