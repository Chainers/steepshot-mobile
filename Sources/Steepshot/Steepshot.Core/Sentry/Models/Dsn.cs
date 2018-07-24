using System;

namespace Steepshot.Core.Sentry.Models
{
    public class Dsn
    {
        public string Path { get; }

        public int Port { get; }

        public string PrivateKey { get; }

        public string ProjectID { get; }

        public string PublicKey { get; }

        public Uri SentryUri { get; }

        public Uri Uri { get; }


        public Dsn(string dsn)
        {
            if (string.IsNullOrEmpty(dsn) || string.IsNullOrEmpty(dsn.Trim()))
                throw new ArgumentNullException("dsn");

            try
            {
                Uri = new Uri(dsn);
                PrivateKey = GetPrivateKey(Uri);
                PublicKey = GetPublicKey(Uri);
                Port = Uri.Port;
                ProjectID = GetProjectID(Uri);
                Path = GetPath(Uri);

                var sentryUriString = $"{ Uri.Scheme}://{Uri.DnsSafeHost}:{Port}{Path}/api/{ProjectID}/store/";
                SentryUri = new Uri(sentryUriString);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("Invalid DSN", "dsn", exception);
            }
        }

        public override string ToString()
        {
            return Uri.ToString();
        }

        private static string GetPath(Uri uri)
        {
            int lastSlash = uri.AbsolutePath.LastIndexOf("/", StringComparison.Ordinal);
            return uri.AbsolutePath.Substring(0, lastSlash);
        }

        private static string GetPrivateKey(Uri uri)
        {
            return uri.UserInfo.Split(':')[1];
        }

        private static string GetProjectID(Uri uri)
        {
            int lastSlash = uri.AbsoluteUri.LastIndexOf("/", StringComparison.Ordinal);
            return uri.AbsoluteUri.Substring(lastSlash + 1);
        }

        private static string GetPublicKey(Uri uri)
        {
            return uri.UserInfo.Split(':')[0];
        }
    }
}