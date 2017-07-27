using System;
using System.Globalization;
using System.Text;
using MailKit.Net.Smtp;
using MimeKit;

namespace Steepshot.Core.Utils
{
    //TODO:KOA: необходимо запихнуть отправку сообщения в бэкграунд процесс
    public class Reporter
    {
        public static void SendCrash(Exception ex, string user, string appVersion)
        {
            var mimeMessage = NewMimeMessage(user);

            var sb = new StringBuilder();
            AppendException(sb, ex);
            AppendDateTime(sb);
            AppendVersion(sb, appVersion);

            mimeMessage.Body = new TextPart("plain")
            {
                Text = sb.ToString()
            };
            SendReport(mimeMessage);
        }

        public static void SendCrash(string message, string user, string appVersion)
        {
            var mimeMessage = NewMimeMessage(user);

            var sb = new StringBuilder(message);
            AppendDateTime(sb);
            AppendVersion(sb, appVersion);

            mimeMessage.Body = new TextPart("plain")
            {
                Text = sb.ToString()
            };
            SendReport(mimeMessage);
        }

        private static MimeMessage NewMimeMessage(string user)
        {
            var customMessage = new MimeMessage();
            customMessage.From.Add(new MailboxAddress(Constants.ReportLogin));
            customMessage.To.Add(new MailboxAddress(Constants.ReportLogin));
            customMessage.Subject = user;
            customMessage.Subject += "[Android]";
            return customMessage;
        }

        private static void SendReport(MimeMessage message)
        {
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect("smtp.gmail.com", 587, false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(Constants.ReportLogin, Constants.ReportPassword);
                client.Send(message);
                client.Disconnect(true);
            }
        }

        private static void AppendException(StringBuilder sb, Exception ex)
        {
            sb.AppendLine(ex.GetType().ToString());
            sb.AppendLine(ex.StackTrace);
            sb.Append("Exception message: ");
            sb.AppendLine(ex.Message);
            sb.Append("Inner exception message: ");
            sb.AppendLine(ex.InnerException?.ToString());
        }

        private static void AppendDateTime(StringBuilder sb)
        {
            sb.Append("UTC time: ");
            sb.AppendLine(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
        }

        private static void AppendVersion(StringBuilder sb, string appVersion)
        {
            sb.Append("App version: ");
            sb.AppendLine(appVersion);
        }
    }
}
