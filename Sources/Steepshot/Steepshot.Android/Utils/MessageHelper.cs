using System;
using Android.App;
using Android.Content;
using Android.Text.Method;
using Android.Text.Util;
using Android.Views;
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

            var message = GetMsg(error);
            if (string.IsNullOrWhiteSpace(message))
                return;

            var btnOk = AppSettings.LocalizationManager.GetText(LocalizationKeys.Ok);
            CreateAndShowDialog(context, message, btnOk);
        }

        public static void ShowAlert(this Context context, LocalizationKeys key)
        {
            var lm = AppSettings.LocalizationManager;
            var btnOk = lm.GetText(LocalizationKeys.Ok);
            var msg = lm.GetText(key);

            CreateAndShowDialog(context, msg, btnOk);
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

            var message = GetMsg(error);

            if (string.IsNullOrWhiteSpace(message))
                return;

            Toast.MakeText(context, message, length).Show();
        }

        public static void ShowInteractiveMessage(this Context context, ErrorBase error, EventHandler<DialogClickEventArgs> tryAgainAction, EventHandler<DialogClickEventArgs> forgetAction)
        {
            if (error == null || error is CanceledError)
                return;

            var message = GetMsg(error);
            if (string.IsNullOrWhiteSpace(message))
                return;

            var lm = AppSettings.LocalizationManager;
            var pBtn = lm.GetText(LocalizationKeys.TryAgain);
            var nBtn = lm.GetText(LocalizationKeys.Forget);
            CreateAndShowDialog(context, message, pBtn, tryAgainAction, nBtn, forgetAction);
        }

        private static void CreateAndShowDialog(Context context, string msg,
            string positiveButtonText, EventHandler<DialogClickEventArgs> positiveButtonAction = null,
            string negativeButtonText = null, EventHandler<DialogClickEventArgs> negativeButtonAction = null)
        {
            var alert = new AlertDialog.Builder(context);
            if (msg.Contains("https:"))
            {
                var inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                var alertView = inflater.Inflate(Resource.Layout.lyt_alert, null);
                var textView = (TextView)alertView.FindViewById(Resource.Id.alert);
                textView.SetText(msg, TextView.BufferType.Normal);
                alert.SetView(alertView);
            }
            else
            {
                alert.SetMessage(msg);
            }

            if (positiveButtonAction == null)
                positiveButtonAction = (senderAlert, args) => { };

            alert.SetPositiveButton(positiveButtonText, positiveButtonAction);

            if (!string.IsNullOrEmpty(negativeButtonText))
                alert.SetNegativeButton(negativeButtonText, negativeButtonAction);

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        private static string GetMsg(ErrorBase error)
        {
            var message = error.Message;
            if (string.IsNullOrWhiteSpace(message))
                return message;

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

            var txt = lm.GetText(message);
            if (error.Parameters != null)
                txt = string.Format(txt, error.Parameters);

            return txt;
        }
    }
}
