using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Support.V7.App;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Fragment;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Steepshot.Base
{
    public abstract class BaseActivity : AppCompatActivity, IBaseView
    {
        public static LruCache Cache { get; set; }
        protected HostFragment CurrentHostFragment;

        public Context GetContext()
        {
            return this;
        }

        public override void OnBackPressed()
        {
            if (CurrentHostFragment == null || !CurrentHostFragment.HandleBackPressed(SupportFragmentManager))
                base.OnBackPressed();
        }

        public virtual void OpenNewContentFragment(Android.Support.V4.App.Fragment frag)
        {
            CurrentHostFragment.ReplaceFragment(frag, true);
        }


        protected void ShowAlert(int messageid)
        {
            Show(GetString(messageid));
        }

        protected void ShowAlert(string message)
        {
            Show(message);
        }

        protected void ShowAlert(List<string> messages)
        {
            if (messages == null || messages.Count == 0)
                return;

            Show(messages[0]);
            //Show(string.Join(System.Environment.NewLine, messages));
        }

        protected void ShowAlert(OperationResult response)
        {
            if (response == null)
                return;
            ShowAlert(response.Errors);
        }


        protected void ShowAlert(int messageid, ToastLength length)
        {
            Toast.MakeText(this, GetString(messageid), length)
                .Show();
        }

        protected void ShowAlert(string message, ToastLength length)
        {
            Toast.MakeText(this, message, length)
                .Show();
        }

        protected void ShowAlert(List<string> messages, ToastLength length)
        {
            if (messages == null || messages.Count == 0)
                return;

            Toast.MakeText(this, messages[0], length).Show();
            //Toast.MakeText(this, string.Join(System.Environment.NewLine, messages), length).Show();
        }

        protected void ShowAlert(OperationResult response, ToastLength length)
        {
            if (response == null)
                return;
            ShowAlert(response.Errors, length);
        }


        private void Show(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            var alert = new AlertDialog.Builder(this);
            alert.SetMessage(text);
            alert.SetPositiveButton(Localization.Messages.Ok, (senderAlert, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }
    }
}
