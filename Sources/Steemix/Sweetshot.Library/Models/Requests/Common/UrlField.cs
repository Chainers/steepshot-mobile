using System;

namespace Sweetshot.Library.Models.Requests.Common
{
    public class UrlField
    {
        public UrlField(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            Url = url;
        }

        public string Url { get; private set; }
    }
}