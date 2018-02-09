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

        public bool ContainsKey(string key)
        {
            var contains = _model.Map.ContainsKey(key);

            if (!contains)
            {
                foreach (var item in _model.Map)
                {
                    if (key.StartsWith(item.Key))
                        return true;
                }
            }
            return contains;
        }

        public string GetText(string key, params object[] args)
        {
            var result = string.Empty;

            if (_model.Map.ContainsKey(key))
            {
                if (args != null && args.Length > 0)
                    result = string.Format(_model.Map[key], args);
                else
                    result = _model.Map[key];
            }
            else
            {
                foreach (var item in _model.Map)
                {
                    if (key.StartsWith(item.Key))
                    {
                        result = item.Value;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(result))
                {
                    foreach (var item in _model.Map)
                    {
                        if (key.Contains(item.Key))
                        {
                            result = item.Value;
                            break;
                        }
                    }
                }
            }
#if DEBUG
            return $"_{result}_";
#endif
            return result;
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
        ScanQRCode,
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
        New,
        Top,
        Clear,
        SeeComment,
        Login,
        Photos,
        Following,
        Followers,
        Reply,
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
        SearchHint,
        Tag,
        Users,
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
        EnterPostTitle,
        EnterPostDescription,
        AddHashtag,
        MyProfile,
        AccountBalance,
        ShowNsfw,
        ShowLowRated,
        Guidelines,
        ToS,
    }
}
