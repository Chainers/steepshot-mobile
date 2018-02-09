using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

namespace Steepshot.Utils
{
    public static class MessageHelpewr
    {
        public static void ShowAlert(this Context context, string message)
        {
            Show(context, message);
        }

        public static void ShowAlert(this Context context, ErrorBase error)
        {
            if (error == null || error is CanceledError)
                return;
            Show(context, error.Message);
        }

        public static void ShowAlert(this Context context, LocalizationKeys key)
        {
            Show(context, AppSettings.LocalizationManager.GetText(key));
        }

        public static void ShowAlert(this Context context, OperationResult response)
        {
            if (response == null)
                return;
            ShowAlert(context, response.Error);
        }

        public static void ShowAlert(this Context context, LocalizationKeys keys, ToastLength length)
        {
            var message = AppSettings.LocalizationManager.GetText(keys);

            if (string.IsNullOrWhiteSpace(message))
                return;

            Toast.MakeText(context, message, length).Show();
        }

        public static void ShowAlert(this Context context, string message, ToastLength length)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var lm = AppSettings.LocalizationManager;
            if (!lm.ContainsKey(message))
            {
                AppSettings.Reporter.SendMessage($"New message: {message}");
                message = nameof(LocalizationKeys.UnexpectedError);
            }

            Toast.MakeText(context, lm.GetText(message), length).Show();
        }

        public static void ShowAlert(this Context context, ErrorBase error, ToastLength length)
        {
            if (error == null || error is CanceledError)
                return;
            ShowAlert(context, error.Message, length);
        }

        public static void ShowAlert(this Context context, OperationResult response, ToastLength length)
        {
            if (response == null)
                return;
            ShowAlert(context, response.Error, length);
        }

        private static void Show(this Context context, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var alert = new AlertDialog.Builder(context);
            var lm = AppSettings.LocalizationManager;
            if (!lm.ContainsKey(text))
            {
                AppSettings.Reporter.SendMessage($"New message: {text}");
                text = nameof(LocalizationKeys.UnexpectedError);
            }

            alert.SetMessage(lm.GetText(text));
            alert.SetPositiveButton(lm.GetText(LocalizationKeys.Ok), (senderAlert, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public static void ShowInteractiveMessage(this Context context, string text, EventHandler<DialogClickEventArgs> tryAgainAction, EventHandler<DialogClickEventArgs> forgetAction)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var alert = new AlertDialog.Builder(context);
            var lm = AppSettings.LocalizationManager;
            if (!lm.ContainsKey(text))
            {
                AppSettings.Reporter.SendMessage($"New message: {text}");
                text = nameof(LocalizationKeys.UnexpectedError);
            }

            alert.SetMessage(lm.GetText(text));
            alert.SetNegativeButton(lm.GetText(LocalizationKeys.Forget), forgetAction);
            alert.SetPositiveButton(lm.GetText(LocalizationKeys.TryAgain), tryAgainAction);
            Dialog dialog = alert.Create();
            dialog.Show();
        }
    }
}
