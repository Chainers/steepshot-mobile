
namespace Steepshot.Core.Utils
{
    public partial class UrlHelper
    {

        public static string ComposeUrl(string author, string permlink)
        {
            return $"@{author}/{permlink}";
        }

        public static bool TryCastUrlToAuthorPermlinkAndParentPermlink(string url, out string author,
            out string commentPermlink, out string parentAuthor, out string parentPermlink)
        {
            var start = url.LastIndexOf('#');

            author = parentPermlink = parentAuthor = commentPermlink = null;

            if (start == -1)
                return false;

            if (!TryCastUrlToAuthorAndPermlink(url.Remove(0, start + 1), out author, out commentPermlink))
                return false;


            if (!TryCastUrlToAuthorAndPermlink(url.Substring(0, start), out parentAuthor, out parentPermlink))
                return false;

            return true;
        }

        public static bool TryCastUrlToAuthorAndPermlink(string url, out string author, out string permlink)
        {
            var start = url.LastIndexOf('@');
            if (start == -1)
            {
                author = permlink = null;
                return false;
            }
            var authAndPermlink = url.Remove(0, start + 1);
            var authPostArr = authAndPermlink.Split('/');
            if (authPostArr.Length != 2)
            {
                author = permlink = null;
                return false;
            }
            author = authPostArr[0];
            permlink = authPostArr[1];
            return true;
        }
    }
}