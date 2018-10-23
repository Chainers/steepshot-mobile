using System.Collections.Generic;

namespace Steepshot.Core
{
    public class GatewayVersion
    {
        public const string V1 = "v1";
        public const string V1P1 = "v1_1";
    }

    public static class Constants
    {
        public const string IsDevKey = "IsDev";
        public const string Steepshot = "Steepshot";

        public const string SteemUrl = "https://steepshot.org/api";
        public const string SteemUrlQa = "https://qa.steepshot.org/api";
        public const string GolosUrl = "https://golos.steepshot.org/api";
        public const string GolosUrlQa = "https://qa.golos.steepshot.org/api";

        public const string SteemitRegUrl = "https://steemit.com/pick_account";
        public const string GolosRegUrl = "https://golos.io/enter_email";
        public const string SteemCreateRegUrl = "https://www.steemcreate.com/";
        public const string BlocktradesRegUrl = "https://blocktrades.us/create-steem-account";
        public const string SteemPostUrl = "https://alpha.steepshot.io/post{0}";
        public const string GolosPostUrl = "https://alpha.steepshot.io/golos/post{0}";

        public const string Tos = "https://steepshot.org/terms-of-service";
        public const string Guide = "https://alpha.steepshot.io/guide";
        public const string Pp = "https://steepshot.org/privacy-policy";

        public const string VotedInASimilarWaySteem = "Your current vote on this comment is identical to this vote.";
        public const string VotedInASimilarWayGolos = "You have already voted in a similar way";

        public const string OneSignalSteemAppId = "77fa644f-3280-4e87-9f14-1f0c7ddf8ca5";
        public const string OneSignalGolosAppId = "8a045ab9-04e1-4d3e-bb67-ddc1742ae385";

        public const int PhotoMaxSize = 1200;
        public const string DeletedPostText = "*deleted*";
        public const string ProxyForAvatars = "https://steemitimages.com/{0}x{1}/{2}";

        public static readonly HashSet<string> SupportedListBots = new HashSet<string>
        {
            "promobot",
            "upme",
            "therising",
            "upmewhale",
            "rocky1",
            "boomerang",
            "appreciator",
            "postpromoter",
            "smartsteem",
            "spydo",
            "booster",
            "emperorofnaps",
            "jerrybanfield"
        };

        public const double MinBid = 0.5;
        public const double MaxBid = 130;


        public static int ImageMaxUploadSize = 10485760;  // 10 mb
        public static int ImageMinWidth = 420;
        public static int ImageMinHeight = 420;
        public static int VideoMaxUploadSize = 20971520;  // 20 mb
        public static int VideoMinWidth = 360;
        public static int VideoMinHeight = 360;
        public static int VideoMinDuration = 2;  // seconds
        public static int VideoMaxDuration = 40; // seconds
    }
}
