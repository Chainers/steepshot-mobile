namespace Steepshot.Core.Localization
{
    public class LocalizationManager
    {
        private LocalizationModel _model;

        public LocalizationManager(LocalizationModel model)
        {
            _model = model;
        }


        public void Reset(LocalizationModel model)
        {
            _model = model;
        }

        public string GetText(LocalizationKeys key, params object[] args)
        {
            var ks = key.ToString();
            return GetText(ks, args);
        }

        public string GetText(string key, params object[] args)
        {
            if (_model.Map.ContainsKey(key))
            {
                if (args != null && args.Length > 0)
                    return string.Format(_model.Map[key], args);
#if DEBUG
                return $"_{_model.Map[key]}_";
#endif
                return _model.Map[key];
            }
#if DEBUG
            return "string.Empty";
#endif
            return string.Empty;
        }
    }


    public enum LocalizationKeys
    {
        WrongPrivatePostingKey,
        WrongPrivateActimeKey,
        EmptyResponseContent,
        ResponseContentContainsHtml,
        UnexpectedUrlFormat,
        EnableConnectToServer,
        EnableConnectToBlockchain,
        ServeNotRespond,
        ServeUnexpectedError,
        EmptyCommentField,
        UnknownError,
        UnknownCriticalError,
        EmptyTitleField,
        EmptyBodyField,
        EmptyCategoryField,
        EmptyPhotoField,
        EmptyFileField,
        EmptyContentType,
        EmptyVerifyTransaction,
        EmptyUrlField,
        EmptyUsernameField,
        EmptyLogin,
        EmptyPostingKey,
        EmptyActiveKey,
        EmptyPostPermlink,
        EmptyCategory,
        PhotoProcessingError,
        PhotoPrepareError,
        PhotoUploadError,
        ErrorCameraPreview,
        ErrorCameraScale,
        ErrorCameraZoom,
        FollowError,
        PostTagsError,
        InternetUnavailable,
        IncorrectIdentifier,
        MaxVoteChanges,
        UnexpectedError,
        CameraSettingError,
        VotedInASimilarWay,
        TagLimitError,
        UnsupportedMime,
        UnexpectedProfileData,
        PostFirstComment,
        Comments,
        PostSettings,
        RapidPosting,
        CameraHoldUp,
        WaitforScan,
        Likes,
        Like,
        Flags,
        Flag,
        Follow,
        Unfollow,
        Error,
        Ok,
        TryAgain,
        Forget,
        Voters,
        FlagVoters,
        ViewComments,
        FlagPhoto,
        HidePhoto,
        Feed,
        Trending,
        Hot,
        Login,
        NewPhotos,
        Hello,
        Profile,
        AcceptToS,
        ChoosePhoto,
        TypeTag,
        TypeUsername,
        YourAccountName,
        NextStep,
        Account,
        NsfwShow,
        Nsfw,
        NsfwContent,
        NsfwContentExplanation,
        LowRated,
        LowRatedContent,
        LowRatedContentExplanation,
        FlagMessage,
        FlagSubMessage,
        DeleteAlertTitle,
        DeleteAlertMessage,
        PowerOfLike,
        TitleForAcceptToS,
        PostDelay,
        SignInButtonText,
        CreateButtonText,
        EnterAccountText,
        PasswordViewTitleText,
        PublishButtonText,
        AppSettingsTitle,
        AddAccountText,
        PeopleText,
        TapToSearch,
        YearsAgo,
        MonthAgo,
        DaysAgo,
        DayAgo,
        HrsAgo,
        HrAgo,
        MinAgo,
        SecAgo,
        CreateFirstPostText,
        Copied,
        PostLink,
        ShowMoreString,
        SignIn,
        FlagPost,
        UnFlagPost,
        FlagComment,
        HideComment,
        EditComment,
        DeleteComment,
        UnFlagComment,
        HidePost,
        EditPost,
        DeletePost,
        CopyLink,
        Cancel,
        Delete,
        PutYourComment,
        ServeRejectRequest,
        LoginMsg,
        NoAccountMsg,
        AppVersion,
        AppVersion2,
        BadRequest,
        Forbidden,
        NotFound,
        InternalServerError,
        BadGateway,
        GatewayTimeout,
        HttpErrorCode,
        QueryMinLength,
    }
}
