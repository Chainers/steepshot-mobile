using System;
using System.Collections.Generic;
using Android.App;
using Android.Support.V7.App;
using Android.Widget;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Fragment;

namespace Steepshot.Base
{
    public abstract class BaseActivity : AppCompatActivity
    {
        protected HostFragment CurrentHostFragment;

        public override void OnBackPressed()
        {
            if (CurrentHostFragment == null || !CurrentHostFragment.HandleBackPressed(SupportFragmentManager))
                base.OnBackPressed();
        }

        public virtual void OpenNewContentFragment(Android.Support.V4.App.Fragment frag)
        {
            CurrentHostFragment?.ReplaceFragment(frag, true);
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
        }

        protected void ShowAlert(OperationResult response)
        {
            if (response == null)
                return;
            ShowAlert(response.Errors);
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
        }

        protected void ShowAlert(OperationResult response, ToastLength length)
        {
            if (response == null)
                return;
            ShowAlert(response.Errors, length);
        }

        private void Show(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            var alert = new Android.Support.V7.App.AlertDialog.Builder(this);
            alert.SetMessage(text);
            alert.SetPositiveButton(Localization.Messages.Ok, (senderAlert, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public override void OnTrimMemory(Android.Content.TrimMemory level)
        {
            GC.Collect();
            base.OnTrimMemory(level);
        }
    }
}
