using System;
using System.Text;
using MailKit.Net.Smtp;
using MimeKit;

namespace Steepshot.iOS
{
	public class Reporter
	{
		public static void SendCrash(Exception ex)
		{
			var mimeMessage = NewMimeMessage();

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(ex.GetType().ToString());
			sb.AppendLine(ex.StackTrace);
			sb.Append("Exception message: ");
			sb.AppendLine(ex.Message);
			sb.Append("Inner exception message: ");
			sb.AppendLine(ex.InnerException?.ToString());
			sb.Append("UTC time: ");
			sb.AppendLine(DateTime.UtcNow.ToString());

			mimeMessage.Body = new TextPart("plain")
			{
				Text = sb.ToString()
			};

			SendReport(mimeMessage);
		}

		public static void SendCrash(string message)
		{
			var mimeMessage = NewMimeMessage();
			mimeMessage.Body = new TextPart("plain")
			{
				Text = message
			};
			SendReport(mimeMessage);
		}

		private static MimeMessage NewMimeMessage()
		{
			var customMessage = new MimeMessage();
			customMessage.From.Add(new MailboxAddress(Constants.ReportLogin));
			customMessage.To.Add(new MailboxAddress(Constants.ReportLogin));
			customMessage.Subject = UserContext.Instanse.Username ?? string.Empty;
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
	}
}
