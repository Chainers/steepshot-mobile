using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

namespace Steepshot.Utils
{
    public static class MessageHelper
    {
        public static void ShowAlert(this Context context, ErrorBase error)
        {
            if (error == null || error is CanceledError)
                return;

            var message = error.Message;
            if (string.IsNullOrWhiteSpace(message))
                return;

            var alert = new AlertDialog.Builder(context);
            var lm = AppSettings.LocalizationManager;
            if (!lm.ContainsKey(message))
            {
                if (error is BlockchainError blError)
                {
                    AppSettings.Reporter.SendMessage($"New message: {LocalizationManager.NormalizeKey(blError.Message)}{Environment.NewLine}Full Message:{blError.FullMessage}");
                }
                else
                {
                    AppSettings.Reporter.SendMessage($"New message: {LocalizationManager.NormalizeKey(message)}");
                }
                message = nameof(LocalizationKeys.UnexpectedError);
            }

            alert.SetMessage(lm.GetText(message));
            alert.SetPositiveButton(lm.GetText(LocalizationKeys.Ok), (senderAlert, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public static void ShowAlert(this Context context, LocalizationKeys key)
        {
            var lm = AppSettings.LocalizationManager;
            var alert = new AlertDialog.Builder(context);
            alert.SetMessage(lm.GetText(key));
            alert.SetPositiveButton(lm.GetText(LocalizationKeys.Ok), (senderAlert, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public static void ShowAlert(this Context context, LocalizationKeys keys, ToastLength length)
        {
            var message = AppSettings.LocalizationManager.GetText(keys);
            Toast.MakeText(context, message, length).Show();
        }

        public static void ShowAlert(this Context context, ErrorBase error, ToastLength length)
        {
            if (error == null || error is CanceledError)
                return;

            var message = error.Message;
            if (string.IsNullOrWhiteSpace(message))
                return;

            var lm = AppSettings.LocalizationManager;
            if (!lm.ContainsKey(message))
            {
                if (error is BlockchainError blError)
                {
                    AppSettings.Reporter.SendMessage($"New message: {LocalizationManager.NormalizeKey(blError.Message)}{Environment.NewLine}Full Message:{blError.FullMessage}");
                }
                else
                {
                    AppSettings.Reporter.SendMessage($"New message: {LocalizationManager.NormalizeKey(message)}");
                }
                message = nameof(LocalizationKeys.UnexpectedError);
            }

            Toast.MakeText(context, lm.GetText(message), length).Show();
        }

        public static void ShowInteractiveMessage(this Context context, ErrorBase error, EventHandler<DialogClickEventArgs> tryAgainAction, EventHandler<DialogClickEventArgs> forgetAction)
        {
            if (error == null || error is CanceledError)
                return;

            var message = error.Message;
            if (string.IsNullOrWhiteSpace(message))
                return;

            var lm = AppSettings.LocalizationManager;
            if (!lm.ContainsKey(message))
            {
                if (error is BlockchainError blError)
                {
                    AppSettings.Reporter.SendMessage($"New message: {LocalizationManager.NormalizeKey(blError.Message)}{Environment.NewLine}Full Message:{blError.FullMessage}");
                }
                else
                {
                    AppSettings.Reporter.SendMessage($"New message: {LocalizationManager.NormalizeKey(message)}");
                }
                message = nameof(LocalizationKeys.UnexpectedError);
            }

            var alert = new AlertDialog.Builder(context);
            alert.SetMessage(lm.GetText(message));
            alert.SetNegativeButton(lm.GetText(LocalizationKeys.Forget), forgetAction);
            alert.SetPositiveButton(lm.GetText(LocalizationKeys.TryAgain), tryAgainAction);
            Dialog dialog = alert.Create();
            dialog.Show();
        }
    }
}
