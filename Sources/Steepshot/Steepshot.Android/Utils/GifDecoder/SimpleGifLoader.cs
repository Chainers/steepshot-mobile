using System.Net;
using System.Threading.Tasks;

namespace Steepshot.Utils.GifDecoder
{
    public class SimpleGifLoader
    {
        public static Task<byte[]> LoadAsync(string url)
        {
            return Task.Run(() =>
            {
                byte[] result;
                using (var client = new WebClient())
                {
                    result = client.DownloadData(new System.Uri(url));
                }
                return result;
            });
        }
    }
}