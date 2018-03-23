namespace Steepshot.Core
{
    public enum GatewayVersion
    {
        V1,
        V1P1
    }

    public static class Constants
    {
        public const string Localization = "Localization";
        public const string UserContextKey = "UserCredentials";
        public const string IsDevKey = "IsDev";
        public const string Steepshot = "Steepshot";

        public const string SteemUrl = "https://steepshot.org/api";
        public const string SteemUrlQa = "https://qa.steepshot.org/api";
        public const string GolosUrl = "https://golos.steepshot.org/api";
        public const string GolosUrlQa = "https://qa.golos.steepshot.org/api";

        public const string SteemitRegUrl = "https://steemit.com/pick_account";
        public const string GolosRegUrl = "https://golos.io/enter_email";

        public const string Tos = "https://steepshot.org/terms-of-service";
        public const string Guide = "https://alpha.steepshot.io/guide";
        public const string Pp = "https://steepshot.org/privacy-policy";

        public const string VotedInASimilarWaySteem = "You have already voted in a similar way";
        public const string VotedInASimilarWayGolos = "You have already voted in a similar way";
    }
}
