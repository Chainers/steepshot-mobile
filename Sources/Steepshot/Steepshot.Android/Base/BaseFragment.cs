using System.Collections.Generic;
using Android.App;
using Android.Widget;
using Steepshot.Core;
using Steepshot.Core.Models.Common;

namespace Steepshot.Base
{
    public abstract class BaseFragment : Android.Support.V4.App.Fragment
    {
        protected bool IsInitialized;
        protected Android.Views.View InflatedView;

        public override void OnViewCreated(Android.Views.View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            IsInitialized = true;
        }

        public virtual bool CustomUserVisibleHint
        {
            get;
            set;
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

        protected void ShowAlert(List<string> messages, ToastLength length)
        {
            if (messages == null || messages.Count == 0)
                return;
            Toast.MakeText(Activity, messages[0], length).Show();
        }

        protected void ShowAlert(OperationResult response, ToastLength length)
        {
            if (response == null)
                return;
            ShowAlert(response.Errors, length);
        }

        private void Show(string text)
        {
            var alert = new AlertDialog.Builder(Context);
            alert.SetMessage(text);
            alert.SetPositiveButton(Localization.Messages.Ok, (senderAlert, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }
    }
}
