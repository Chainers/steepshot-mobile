using System;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Steepshot.Core;

namespace Steepshot
{
    public class Reporter
    {
        public async static Task SendCrash(Exception ex)
        {
            var mimeMessage = NewMimeMessage();
            await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(ex.GetType().ToString());
                sb.AppendLine(ex.StackTrace);
                sb.Append("Exception message: ");
                sb.AppendLine(ex.Message);
                sb.Append("Inner exception message: ");
                sb.AppendLine(ex.InnerException?.ToString());
                sb.Append("UTC time: ");
                sb.AppendLine(DateTime.UtcNow.ToString());
                sb.Append("App version: ");
                sb.AppendLine(BasePresenter.AppVersion);

                mimeMessage.Body = new TextPart("plain")
                {
                    Text = sb.ToString()
                };
            });
            await SendReport(mimeMessage);
        }

        public async static Task SendCrash(string message)
        {
            var mimeMessage = NewMimeMessage();
            await Task.Run(() =>
                        {
                            message += $" UTC time: {DateTime.UtcNow.ToString()}";
                            mimeMessage.Body = new TextPart("plain")
                            {
                                Text = message
                            };
                        });
            await SendReport(mimeMessage);
        }

        private static MimeMessage NewMimeMessage()
        {
            var customMessage = new MimeMessage();
            customMessage.From.Add(new MailboxAddress(Constants.ReportLogin));
            customMessage.To.Add(new MailboxAddress(Constants.ReportLogin));
            customMessage.Subject = string.IsNullOrEmpty(BasePresenter.User.Login) ? string.Empty : BasePresenter.User.Login;
            customMessage.Subject += " [Android]";
            return customMessage;
        }


        private static async Task SendReport(MimeMessage message)
        {
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect("smtp.gmail.com", 587, false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(Constants.ReportLogin, Constants.ReportPassword);
                await client.SendAsync(message);
                client.Disconnect(true);
            }
        }
    }
}
