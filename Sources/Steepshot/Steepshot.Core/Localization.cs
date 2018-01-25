using System;
using System.Net;

namespace Steepshot.Core
{
    public class Localization
    {
        public class Errors
        {
            public const string WrongPrivatePostingKey = "It`s not a valid Private Posting key! Check - Private key looks like 5********...";
            public const string WrongPrivateActimeKey = "It`s not a valid Private Active key! Check - Private key looks like 5********...";
            public const string EmptyResponseContent = "Empty response content";
            public const string ResponseContentContainsHtml = "Response content contains HTML: ";
            public const string UnexpectedUrlFormat = "Unexpected url format: ";
            public const string EnableConnectToServer = "Can not connect to the server, check for an Internet connection and try again.";
            public const string EnableConnectToBlockchain = "Failed to connect to blockchain!";
            public const string ServeNotRespond = "The server does not respond to the request. Check your internet connection and try again.";
            public const string ServeUnexpectedError = "An unexpected error occurred. Check the Internet or try restarting the application.";
            public const string EmptyCommentField = "Comment may not be blank!";
            public const string UnknownError = "Unknown error. Try again";
            public const string UnknownCriticalError = "An unexpected critical error occurred. Unfortunately the next step can not be performed.";
            public const string EmptyTitleField = "Title required";
            public const string EmptyFileField = "File cannot be empty";
            public const string EmptyVerifyTransaction = "Empty authentification";
            public const string EmptyUrlField = "Url cannot be empty";
            public const string EmptyUsernameField = "Username cannot be empty";
            public const string EmptyLogin = "Login cannot be empty";
            public const string EmptyPostingKey = "Posting key cannot be empty";
            public const string EmptyActiveKey = "Active key cannot be empty";
            public const string PhotoProcessingError = "An error occurred while processing the photo. Unfortunately the next step can not be performed.";
            public const string PhotoPrepareError = "Failure to process the photos. Try to re-select the photo.";
            public const string PhotoUploadError = "Photo upload error: ";
            public const string ErrorCameraPreview = "Error setting camera preview: ";
            public const string ErrorCameraScale = "ScalemageView does not support FitStart or FitEnd";
            public const string ErrorCameraZoom = "getZoomedRect() not supported with FitXy";
            public const string FollowError = "Follow page follow error: ";
            public const string PostTagsError = "Post tags page get items error: ";
            public const string InternetUnavailable = "Check your internet connection";
            public const string IncorrectIdentifier = "Incorrect identifier";
            public const string MaxVoteChanges = "You`ve already liked this post a few times. Please try another one.";
            public const string UnexpectedError = "An unexpected error occurred.";
            public const string CameraSettingError = "Camera Setting error.";
            public const string VotedInASimilarWay = "You have already voted in a similar way.";
            public const string TagLimitError = "The number of tags should not be more than 20. Please remove a couple of tags and try again.";
            public const string UnsupportedMime = "Unsupported file type";
            public const string UnexpectedProfileData = "Unexpected profile data";


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

            internal static string HttpErrorCodeToMessage(HttpStatusCode statusCode, string content)
            {
                switch (statusCode)
                {
                    case HttpStatusCode.BadRequest:
                        return $"{statusCode} BadRequest. (The server cannot or will not process the request due to an apparent client error.)";
                    case HttpStatusCode.Forbidden:
                        return $"{statusCode} Forbidden. (The request was valid, but the server is refusing action. Try to log in.) ";
                    case HttpStatusCode.NotFound:
                        return $"{statusCode} NotFound. (The requested resource could not be found but may be available in the future.) ";
                    case HttpStatusCode.InternalServerError:
                        return $"{statusCode} InternalServerError. (A generic error message, given when an unexpected condition was encountered and no more specific message is suitable.) ";
                    case HttpStatusCode.BadGateway:
                        return $"{statusCode} BadGateway. (The server was acting as a gateway or proxy and received an invalid response from the upstream server) ";
                    case HttpStatusCode.GatewayTimeout:
                        return $"{statusCode} GatewayTimeout. (The server was acting as a gateway or proxy and did not receive a timely response from the upstream server. Try repeat request a little later.) ";
                    default:
                        return $"({statusCode}) Some error on server side.";
                }
            }
        }

        public class Messages
        {
            public const string PostFirstComment = "Post your first comment";
            public const string Comments = "Comments";
            public const string RapidPosting = "You post so fast. Try it later";
            public const string CameraHoldUp = "Hold the camera up to the barcode\nAbout 6 inches away";
            public const string WaitforScan = "Wait for the barcode to automatically scan!";
            public const string Likes = "likes";
            public const string Like = "like";
            public const string Flags = "flags";
            public const string Flag = "flag";
            public const string Follow = "Follow";
            public const string Unfollow = "Unfollow";
            public const string Error = "Error";
            public const string Ok = "Ok";
            public const string TryAgain = "Try again";
            public const string Forget = "Forget";
            public const string Voters = "Likes";
            public const string FlagVoters = "Flags";
            public const string ViewComments = "See all {0} comments";
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
            public const string AcceptToS = "Please make sure you accept the terms of service and privacy policy";
            public const string ChoosePhoto = "CHOOSE PHOTO";
            public const string TypeTag = "Please type a tag";
            public const string TypeUsername = "Please type an username";
            public const string YourAccountName = "Your account name";
            public const string NextStep = "Next step";
            public const string Account = "account";
            public const string NSFWShow = "Show";
            public const string NSFW = "NSFW";
            public const string NSFWContent = "NSFW content";
            public const string NSFWContentExplanation = "This content is for adults only. Not recommended for children or sensitive individuals.";
            public const string LowRated = "Low rated";
            public const string LowRatedContent = "Low rated content";
            public const string LowRatedContentExplanation = "This content is hidden due to low ratings";
            public const string FlagMessage = "You sent a complaint to this post.";
            public const string FlagSubMessage = "";
            public const string DeleteAlertTitle = "Are you sure you want to delete this post?";
            public const string DeleteAlertMessage = "This action can not be undone.";

            public static readonly string TitleForAcceptToS = $"By pressing any of the buttons you agree with our <a href=\"{Constants.Tos}\">Terms of Service</a> & <a href=\"{Constants.Pp}\">Privacy policy</a>";
            public const string PostDelay = "If you don't see the post in your profile, please give it a few minutes to sync from the blockchain";

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

        public class Texts
        {
            public const string SignInButtonText = "Sign in with {0}";
            public const string CreateButtonText = "Create new account";
            public const string EnterAccountText = "Enter to your account";
            public const string PasswordViewTitleText = "Account posting key";
            public const string PublishButtonText = "Publish Photo";
            public const string AppSettingsTitle = "App settings";
            public const string AddAccountText = "Add account";
            public const string PeopleText = "people";
            public const string TapToSearch = "Tap to search";
            public const string YearsAgo = "years ago";
            public const string MonthAgo = "month ago";
            public const string DaysAgo = "days ago";
            public const string DayAgo = "day ago";
            public const string HrsAgo = "hrs ago";
            public const string HrAgo = "hr ago";
            public const string MinAgo = "min ago";
            public const string SecAgo = "sec ago";
            public const string CreateFirstPostText = "Create first photo";
            public const string EmptyQuery = "It's very strange, but we do not have anything yet for this query. Try to look for something else ...";
            public const string Copied = "Copied to clipboard";
            public const string PostLink = "https://alpha.steepshot.io/post{0}";
            public const string ShowMoreString = " Show more...";
            public const string SignIn = "Sign in";
            public const string FlagPost = "Flag post";
            public const string UnFlagPost = "Remove flag";
            public const string FlagComment = "Flag comment";
            public const string HideComment = "Hide comment";
            public const string DeleteComment = "Delete comment";
            public const string UnFlagComment = "Unflag comment";
            public const string HidePost = "Hide post";
            public const string DeletePost = "Delete post";
            public const string CopyLink = "Copy link";
            public const string Cancel = "Cancel";
            public const string Delete = "Delete";
            public const string PutYourComment = "Put your comment here...";
        }
    }
}
