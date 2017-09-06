namespace Steepshot.Core
{
    public class Localization
    {
        public class Errors
        {
            public const string WrongPrivateKey = "It`s not a valid Private posting key! Check - Private key looks like 5********...";
            public const string EmptyResponseContent = "Empty response content";
            public const string ResponseContentContainsHtml = "Response content contains HTML: ";
            public const string UnexpectedUrlFormat = "Unexpected url format: ";
            public const string EnableConnectToServer = "Can not connect to the server, check for an Internet connection and try again.";
            public const string ServeNotRespond = "The server does not respond to the request. Check your internet connection and try again.";
            public const string ServeUnexpectedError = "An unexpected error occurred. Check the Internet or try restarting the application.";
            public const string MissingSessionId = "SessionId field is missing.";
            public const string EmptyField = "This field may not be blank!";
            public const string Unknownerror = "Unknown error. Try again";
            public const string EmptyDescription = "Description cannot be empty";
            public const string EmptyLogin = "Login cannot be empty";
            public const string PhotoCompressingError = "Photo compressing error";
            public const string PhotoUploadError = "Photo upload error: ";
            public const string ErrorCameraPreview = "Error setting camera preview: ";
            public const string ErrorCameraScale = "ScalemageView does not support FitStart or FitEnd";
            public const string ErrorCameraZoom = "getZoomedRect() not supported with FitXy";
            public const string FollowError = "Follow page follow error: ";
            public const string PostTagsError = "Post tags page get items error: ";
            public const string InternetUnavailable = "Check your internet connection";

            /// <summary>
            ///  $"The server did not accept the request! Reason ({code}) {msg}";
            /// </summary>
            /// <param name="code"></param>
            /// <param name="msg"></param>
            /// <returns></returns>
            public static string ServeRejectRequest(long code, string msg)
            {
                return $"The server did not accept the request! Reason ({code}) {msg}";
            }
        }

        public class Messages
        {
            public const string PostFirstComment = "Post first comment";
            public const string RapidPosting = "You post so fast. Try it later";
            public const string CameraHoldUp = "Hold the camera up to the barcode\nAbout 6 inches away";
            public const string WaitforScan = "Wait for the barcode to automatically scan!";
            public const string Likes = "like's";
            public const string Follow = "Follow";
            public const string Unfollow = "Unfollow";
            public const string Error = "Error";
            public const string Ok = "Ok";
            public const string Voters = "Voters";
            public const string ViewComments = "View {0} comments";
            public const string FlagPhoto = "Flag photo";
            public const string HidePhoto = "Hide photo";
            public const string Cancel = "Cancel";
            public const string Feed = "FEED";
            public const string Trending = "TRENDING";
            public const string Hot = "HOT";
            public const string Login = "Login";
            public const string NewPhotos = "NEW PHOTOS";
            public const string Hello = "Hello, ";
            public const string Profile = "PROFILE";
            public const string AcceptToS = "Make sure you accept the terms of service and privacy policy";
            public const string ChoosePhoto = "CHOOSE PHOTO";
            public const string TypeTag = "Please type a tag";
            public const string TypeUsername = "Please type an username";

            /// <summary>
            /// $"Log in with your {chain} Account";
            /// </summary>
            /// <param name="chain"></param>
            /// <returns></returns>
            public static string LoginMsg(KnownChains chain)
            {
                return $"Log in with your {chain} Account";
            }

            /// <summary>
            ///  $"Haven't {chain} account yet?";
            /// </summary>
            /// <param name="chain"></param>
            /// <returns></returns>
            public static string NoAccountMsg(KnownChains chain)
            {
                return $"Haven't {chain} account yet?";
            }

            public static string AppVersion(KnownChains chain)
            {
                return $"Haven't {chain} account yet?";
            }

            public static string AppVersion(string v, string bn)
            {
                return $"App version: {v} Build number: {bn}";
            }
        }
    }
}
