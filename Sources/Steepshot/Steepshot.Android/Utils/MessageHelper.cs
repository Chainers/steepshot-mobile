using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Widget;
using Steepshot.Core;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils
{
    public static class MessageHelpewr
    {
        public static void ShowAlert(this Context context, string message)
        {
            Show(context, message);
        }

        public static void ShowAlert(this Context context, List<string> messages)
        {
            if (messages == null || messages.Count == 0)
                return;
            Show(context, messages[0]);
        }

        public static void ShowAlert(this Context context, OperationResult response)
        {
            if (response == null)
                return;
            ShowAlert(context, response.Errors);
        }

        public static void ShowAlert(this Context context, string message, ToastLength length)
        {
            Toast.MakeText(context, message, length).Show();
        }

        public static void ShowAlert(this Context context, List<string> messages, ToastLength length)
        {
            if (messages == null || messages.Count == 0)
                return;
            Toast.MakeText(context, messages[0], length).Show();
        }

        public static void ShowAlert(this Context context, OperationResult response, ToastLength length)
        {
            if (response == null)
                return;
            ShowAlert(context, response.Errors, length);
        }

        private static void Show(this Context context, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var alert = new AlertDialog.Builder(context);
            alert.SetMessage(text);
            alert.SetPositiveButton(Localization.Messages.Ok, (senderAlert, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public static void ShowInteractiveMessage(this Context context, string text, EventHandler<DialogClickEventArgs> tryAgainAction, EventHandler<DialogClickEventArgs> forgetAction)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var alert = new AlertDialog.Builder(context);
            alert.SetMessage(text);
            alert.SetNegativeButton(Localization.Messages.Forget, forgetAction);
            alert.SetPositiveButton(Localization.Messages.TryAgain, tryAgainAction);
            Dialog dialog = alert.Create();
            dialog.Show();
        }
    }
}
