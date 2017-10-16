using System.Runtime.InteropServices;

namespace Steepshot.Core
{
    public enum GatewayVersion
    {
        V1,
        V1P1
    }

    public class Constants
    {
        public const string ReportLogin = "crash.steepshot.org@gmail.com";
        public const string ReportPassword = "steep7788";
        public const string UserContextKey = "UserCredentials";
        public const string IsDevKey = "IsDev";
        public const string Steepshot = "Steepshot";

        public const string SteemUrl = "https://steepshot.org/api";
        public const string SteemUrlQa = "https://qa.steepshot.org/api";
        public const string GolosUrl = "https://golos.steepshot.org/api";
        public const string GolosUrlQa = "https://qa.golos.steepshot.org/api";

        public const string SteemitRegUrl = "https://steemit.com/pick_account";
        public const string GolosRegUrl = "https://golos.io/enter_email";
    }
}
