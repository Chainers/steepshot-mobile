using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

namespace Steepshot.Utils
{
    public static class MessageHelper
    {
        public static void ShowAlert(this Context context, Exception error)
        {
            if (IsSkeepError(error))
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

        public static void ShowAlert(this Context context, Exception error, ToastLength length)
        {
            if (IsSkeepError(error))
                return;

            var message = GetMsg(error);

            if (string.IsNullOrWhiteSpace(message))
                return;

            Toast.MakeText(context, message, length).Show();
        }

        public static void ShowInteractiveMessage(this Context context, Exception error, EventHandler<DialogClickEventArgs> tryAgainAction, EventHandler<DialogClickEventArgs> forgetAction)
        {
            if (IsSkeepError(error))
                return;

            var message = GetMsg(error);
            if (string.IsNullOrWhiteSpace(message))
                return;

            var lm = AppSettings.LocalizationManager;
            var pBtn = lm.GetText(LocalizationKeys.TryAgain);
            var nBtn = lm.GetText(LocalizationKeys.Forget);
            CreateAndShowDialog(context, message, pBtn, tryAgainAction, nBtn, forgetAction);
        }

        private static void CreateAndShowDialog(Context context, string msg, string positiveButtonText, EventHandler<DialogClickEventArgs> positiveButtonAction = null, string negativeButtonText = null, EventHandler<DialogClickEventArgs> negativeButtonAction = null)
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

        private static string GetMsg(Exception error)
        {
            var lm = AppSettings.LocalizationManager;

            if (error is ValidationError validationError)
                return lm.GetText(validationError);

            AppSettings.Reporter.Error(error);
            var msg = string.Empty;

            if (error is InternalError internalError)
            {
                msg = lm.GetText(internalError.Key);
            }
            else if (error is RequestError requestError)
            {
                if (!string.IsNullOrEmpty(requestError.RawResponse))
                    msg = lm.GetText(requestError.RawResponse);
            }
            else
            {
                msg = lm.GetText(error.Message);
            }

            return string.IsNullOrEmpty(msg) ? lm.GetText(LocalizationKeys.UnexpectedError) : msg;
        }

        private static bool IsSkeepError(Exception error)
        {
            if (error == null || error is TaskCanceledException || error is OperationCanceledException)
                return true;

            if (error is RequestError requestError)
            {
                if (requestError.Exception is TaskCanceledException || requestError.Exception is OperationCanceledException)
                    return true;
            }

            return false;
        }
    }
}
