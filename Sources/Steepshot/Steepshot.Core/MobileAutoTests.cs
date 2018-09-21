using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Ditch.Core;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;

namespace Steepshot.Core
{
    public class MobileAutoTests
    {
        private readonly SteepshotApiClient _api;
        private readonly UserInfo _user;
        private readonly IAppInfo _appInfo;
        public event Action<string> StepFinished;
        private readonly StringBuilder _log = new StringBuilder();

        public MobileAutoTests(SteepshotApiClient api, UserInfo user, IAppInfo appInfo)
        {
            _api = api;
            _user = user;
            _appInfo = appInfo;
        }

        public Task RunDitchApiTestsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var num = 1;
                    _log.AppendLine("DitchApi tests started");
                    LoginWithPostingKeyTest(_log, num++);
                    VoteTest(_log, num++);
                    FollowTest(_log, num++);
                    CreateCommentTest(_log, num++);
                    //UploadTest(_log, num++);
                    _log.AppendLine("Tests End;");
                    StepFinished?.Invoke(_log.ToString());
                }
                catch (Exception e)
                {
                    StepFinished?.Invoke($"{e.Message} {e.StackTrace}");
                }
            });
        }
        public Task RunServerTestsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var num = 1;
                    _log.AppendLine("DitchApi tests started");
                    GetUserPostsTest(_log, num++);
                    GetUserRecentPostsTest(_log, num++);
                    GetPostsTest(_log, num++);
                    GetPostsByCategoryTest(_log, num++);
                    GetPostVotersTest(_log, num++);
                    GetCommentsTest(_log, num++);
                    GetUserProfileTest(_log, num++);
                    GetUserFriendsTest(_log, num++);
                    GetPostInfoTest(_log, num++);
                    SearchUserTest(_log, num++);
                    UserExistsCheckTest(_log, num++);
                    GetCategoriesTest(_log, num++);
                    SearchCategoriesTest(_log, num++);
                    _log.AppendLine("Tests End;");
                    StepFinished?.Invoke(_log.ToString());
                }
                catch (Exception e)
                {
                    StepFinished?.Invoke($"{e.Message} {e.StackTrace}");
                }
            });
        }

        #region ApiTests

        //ditchapi

        private void VoteTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) VoteTest : ");
            StepFinished?.Invoke(sb.ToString());
            // Load last created post
            var getPosts = new PostsModel(PostType.New) { Login = _user.Login };
            Post testPost;
            do
            {
                var postsResp = _api.GetPostsAsync(getPosts, CancellationToken.None)
                    .Result;

                if (!postsResp.IsSuccess)
                {
                    sb.AppendLine($"fail. Reason:{Environment.NewLine} {postsResp.Exception.Message}");
                    return;
                }
                if (postsResp.Result.Results.Count == 0)
                {
                    sb.AppendLine($"fail. Reason:{Environment.NewLine} There are no Posts to Upvote!");
                    return;
                }

                testPost = postsResp.Result.Results.FirstOrDefault(i => !i.Vote);
                if (testPost == null)
                    getPosts.Offset = postsResp.Result.Results.Last().Url;

            } while (testPost == null);

            var votereq = new VoteModel(_user, testPost, VoteType.Up);
            var rez = _api.VoteAsync(votereq, CancellationToken.None)
                .Result;

            if (!rez.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {rez.Exception.Message}");
                return;
            }

            Task.Delay(10000);
            getPosts.Offset = testPost.Url;
            getPosts.Limit = 1;

            var verifyPostresp = _api.GetPostsAsync(getPosts, CancellationToken.None)
                .Result;

            if (!verifyPostresp.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {verifyPostresp.Exception.Message}");
                return;
            }
            if (verifyPostresp.Result.Results.Count != 1)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} Upvoted post ({testPost.Url}) not found!");
                return;
            }
            if (!verifyPostresp.Result.Results[0].Vote)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} post ({testPost.Url}) wasn`t Upvoted!");
                return;
            }

            sb.AppendLine("pass.");
        }

        private void FollowTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) FollowTest : ");
            StepFinished?.Invoke(sb.ToString());
            // Load last created post
            var getPosts = new PostsModel(PostType.New) { Login = _user.Login };

            var postsResp = _api.GetPostsAsync(getPosts, CancellationToken.None)
                .Result;

            if (!postsResp.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {postsResp.Exception.Message}");
                return;
            }
            if (postsResp.Result.Results.Count == 0)
            {
                sb.AppendLine("fail. Reason:{Environment.NewLine} There are no Posts to Follow!");
                return;
            }

            var testPost = postsResp.Result.Results.First();

            var votereq = new FollowModel(_user, FollowType.Follow, testPost.Author);
            var rez = _api.FollowAsync(votereq, CancellationToken.None)
                .Result;

            if (!rez.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {rez.Exception.Message}");
                return;
            }

            Task.Delay(10000);

            var userFriendsReq = new UserFriendsModel(_user.Login, FriendsType.Followers) { Login = _user.Login, Offset = testPost.Author, Limit = 1 };
            var verifyResp = _api.GetUserFriendsAsync(userFriendsReq, CancellationToken.None).Result;
            if (!verifyResp.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {verifyResp.Exception.Message}");
                return;
            }
            if (verifyResp.Result.Results.Count != 1)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} user ({testPost.Author}) not found!");
                return;
            }
            sb.AppendLine("pass.");
        }

        private void LoginWithPostingKeyTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) LoginWithPostingKeyTest : ");
            StepFinished?.Invoke(sb.ToString());

            var request = new ValidatePrivateKeyModel(_user.Login, _user.PostingKey, KeyRoleType.Posting);
            var response = _api.ValidatePrivateKeyAsync(request, CancellationToken.None).Result;
            if (!response.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {response.Exception.Message}");
                return;
            }
            sb.AppendLine("pass.");
        }

        private void CreateCommentTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) CreateCommentTest : ");
            StepFinished?.Invoke(sb.ToString());

            // Load last created post
            var getPosts = new PostsModel(PostType.New) { Login = _user.Login };

            var postsResp = _api.GetPostsAsync(getPosts, CancellationToken.None)
                .Result;

            if (!postsResp.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {postsResp.Exception.Message}");
                return;
            }
            if (postsResp.Result.Results.Count == 0)
            {
                sb.AppendLine("fail. Reason:{Environment.NewLine} There are no Posts!");
                return;
            }

            var testPost = postsResp.Result.Results.First();
            var req = new CreateOrEditCommentModel(_user, testPost, "Hi, I am a bot for testing Ditch api, please ignore this comment.", _appInfo);
            var rez = _api.CreateOrEditCommentAsync(req, CancellationToken.None)
                .Result;

            if (!UrlHelper.TryCastUrlToAuthorAndPermlink(testPost.Url, out var parentAuthor, out var parentPermlink))
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} url to permlink cast.");
                return;
            }

            var permlink = OperationHelper.CreateReplyPermlink(_user.Login, parentAuthor, parentPermlink);

            if (!rez.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {rez.Exception.Message}");
                return;
            }

            Task.Delay(10000);

            var getComm = new NamedInfoModel(testPost.Url) { Offset = permlink, Limit = 1 };
            var verifyPostresp = _api.GetCommentsAsync(getComm, CancellationToken.None)
                .Result;

            if (!verifyPostresp.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {verifyPostresp.Exception.Message}");
                return;
            }
            if (verifyPostresp.Result.Results.Count != 1)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} Comment ({permlink}) not found!");
                return;
            }

            sb.AppendLine("pass.");
        }

        //private void UploadTest(StringBuilder sb, int num)
        //{
        //    sb.Append($"{num}) UploadTest : ");
        //    StepFinished?.Invoke(sb.ToString());

        //    var cat = "/9j/4AAQSkZJRgABAQEAwADAAAD/4QBqRXhpZgAATU0AKgAAAAgAAwESAAMAAAABAAEAAAExAAIAAAAQAAAAModpAAQAAAABAAAAQgAAAABTaG90d2VsbCAwLjIyLjAAAAKgAgAJAAAAAQAAAMKgAwAJAAAAAQAAAMIAAAAAAAD/4Qn0aHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/Pg0KPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iWE1QIENvcmUgNC40LjAtRXhpdjIiPg0KCTxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+DQoJCTxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOmV4aWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vZXhpZi8xLjAvIiB4bWxuczp0aWZmPSJodHRwOi8vbnMuYWRvYmUuY29tL3RpZmYvMS4wLyIgZXhpZjpQaXhlbFhEaW1lbnNpb249IjE5NCIgZXhpZjpQaXhlbFlEaW1lbnNpb249IjE5NCIgdGlmZjpJbWFnZVdpZHRoPSIxOTQiIHRpZmY6SW1hZ2VIZWlnaHQ9IjE5NCIvPg0KCTwvcmRmOlJERj4NCjwveDp4bXBtZXRhPg0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDw/eHBhY2tldCBlbmQ9J3cnPz7/2wBDAAIBAQIBAQICAgICAgICAwUD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";

        //    byte[] byteArray = Encoding.ASCII.GetBytes(cat);
        //    MemoryStream stream = new MemoryStream(byteArray);

        //    var request = new UploadMediaModel(_user, stream, ".jpg")
        //    {
        //        Thumbnails = false,
        //    };
        //    var mediaResponse = _api.UploadMediaUuid(request, CancellationToken.None).Result;
        //    if (!mediaResponse.IsSuccess)
        //    {
        //        sb.AppendLine($"fail. Reason:{Environment.NewLine} {mediaResponse.Exception.Message}");
        //        return;
        //    }

        //    var model = new PreparePostModel(_user, AppSettings.AppInfo.GetModel())
        //    {
        //        Tags = new[] { "spam" },
        //        Title = "Upload test",
        //        Media = new[] { mediaResponse.Result },
        //    };

        //    var response = _api.CreateOrEditPost(model, CancellationToken.None).Result;
        //    if (!response.IsSuccess)
        //    {
        //        sb.AppendLine($"fail. Reason:{Environment.NewLine} {response.Exception.Message}");
        //        return;
        //    }
        //    sb.AppendLine("pass.");
        //}

        //base
        private void GetUserPostsTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) GetUserPostsTest : ");
            StepFinished?.Invoke(sb.ToString());

            var limit = 3;
            var request = new UserPostsModel(_user.Login) { ShowNsfw = _user.IsNsfw, ShowLowRated = _user.IsLowRated, Limit = limit };
            var response = _api.GetUserPostsAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Url;
            response = _api.GetUserPostsAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            sb.AppendLine("pass.");
        }

        private void GetUserRecentPostsTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) GetUserRecentPostsTest : ");
            StepFinished?.Invoke(sb.ToString());

            var limit = 3;
            var request = new CensoredNamedRequestWithOffsetLimitModel
            {
                Login = _user.Login,
                Limit = limit,
                ShowNsfw = _user.IsNsfw,
                ShowLowRated = _user.IsLowRated
            };
            var response = _api.GetUserRecentPostsAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Url;
            response = _api.GetUserRecentPostsAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            sb.AppendLine("pass.");
        }

        private void GetPostsTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) GetPostsTest : ");
            StepFinished?.Invoke(sb.ToString());

            var limit = 3;
            var request = new PostsModel(PostType.New) { ShowNsfw = _user.IsNsfw, ShowLowRated = _user.IsLowRated, Limit = limit };
            var response = _api.GetPostsAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Url;
            response = _api.GetPostsAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            sb.AppendLine("pass.");
        }

        private void GetPostsByCategoryTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) GetPostsByCategoryTest : ");
            StepFinished?.Invoke(sb.ToString());

            var limit = 3;
            var request = new PostsByCategoryModel(PostType.New, "steepshot") { ShowNsfw = _user.IsNsfw, ShowLowRated = _user.IsLowRated, Limit = limit };
            var response = _api.GetPostsByCategoryAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Url;
            response = _api.GetPostsByCategoryAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            sb.AppendLine("pass.");
        }

        private void GetPostVotersTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) GetPostVotersTest : ");
            StepFinished?.Invoke(sb.ToString());

            var limit = 3;

            var testPost = GetFirstPostWhere(sb, PostType.Top, i => i.Children >= limit + limit);
            if (testPost == null)
                return;

            var request = new VotersModel(testPost.Url, VotersType.All) { Limit = limit, Login = _user.Login };
            var response = _api.GetPostVotersAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Author;
            response = _api.GetPostVotersAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            sb.AppendLine("pass.");
        }

        private void GetCommentsTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) GetCommentsTest : ");
            StepFinished?.Invoke(sb.ToString());

            var limit = 3;

            var testPost = GetFirstPostWhere(sb, PostType.Top, i => i.Children >= limit + limit);
            if (testPost == null)
                return;

            var request = new NamedInfoModel(testPost.Url) { Limit = limit };
            var response = _api.GetCommentsAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Url;
            response = _api.GetCommentsAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            sb.AppendLine("pass.");
        }

        private void GetUserProfileTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) GetUserProfileTest : ");
            StepFinished?.Invoke(sb.ToString());

            var request = new UserProfileModel(_user.Login);
            var response = _api.GetUserProfileAsync(request, CancellationToken.None).Result;
            if (!response.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {response.Exception.Message}");
                return;
            }

            sb.AppendLine("pass.");
        }

        private void GetUserFriendsTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) GetUserFriendsTest : ");
            StepFinished?.Invoke(sb.ToString());

            var limit = 3;

            var request = new UserFriendsModel(_user.Login, FriendsType.Followers) { Limit = limit };
            var response = _api.GetUserFriendsAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Author;
            response = _api.GetUserFriendsAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            //---

            request = new UserFriendsModel(_user.Login, FriendsType.Following) { Limit = limit };
            response = _api.GetUserFriendsAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Author;
            response = _api.GetUserFriendsAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            sb.AppendLine("pass.");
        }

        private void GetPostInfoTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) GetPostInfoTest : ");
            StepFinished?.Invoke(sb.ToString());

            var getPosts = new PostsModel(PostType.Top);

            var postsResp = _api.GetPostsAsync(getPosts, CancellationToken.None)
                .Result;

            if (!postsResp.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {postsResp.Exception.Message}");
                return;
            }
            if (postsResp.Result.Results.Count == 0)
            {
                sb.AppendLine("fail. Reason:{Environment.NewLine} There are no Posts!");
                return;
            }

            var testPost = postsResp.Result.Results.First();


            var request = new NamedInfoModel(testPost.Url);
            var response = _api.GetPostInfoAsync(request, CancellationToken.None).Result;
            if (!response.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {postsResp.Exception.Message}");
                return;
            }

            sb.AppendLine("pass.");
        }

        private void SearchUserTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) SearchUserTest : ");
            StepFinished?.Invoke(sb.ToString());

            var limit = 3;

            var request = new SearchWithQueryModel(_user.Login) { Limit = limit };
            var response = _api.SearchUserAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Author;
            response = _api.SearchUserAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            sb.AppendLine("pass.");
        }

        private void UserExistsCheckTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) UserExistsCheckTest : ");
            StepFinished?.Invoke(sb.ToString());

            var request = new UserExistsModel(_user.Login);
            var response = _api.UserExistsCheckAsync(request, CancellationToken.None).Result;
            if (!response.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {response.Exception.Message}");
                return;
            }

            if (!response.Result.Exists)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} User {_user.Login} not found! ");
                return;
            }

            sb.AppendLine("pass.");
        }

        private void GetCategoriesTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) GetCategoriesTest : ");
            StepFinished?.Invoke(sb.ToString());

            var limit = 3;

            var request = new OffsetLimitModel { Limit = limit };
            var response = _api.GetCategoriesAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Name;
            response = _api.GetCategoriesAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            sb.AppendLine("pass.");
        }

        private void SearchCategoriesTest(StringBuilder sb, int num)
        {
            sb.Append($"{num}) SearchCategoriesTest : ");
            StepFinished?.Invoke(sb.ToString());

            var limit = 3;

            var request = new SearchWithQueryModel("go") { Limit = limit };
            var response = _api.SearchCategoriesAsync(request, CancellationToken.None).Result;

            if (IsError1(sb, limit, response, response.Result.Results.Count))
                return;

            request.Offset = response.Result.Results.Last().Name;
            response = _api.SearchCategoriesAsync(request, CancellationToken.None).Result;

            if (IsError2(sb, limit, response, request.Offset))
                return;

            sb.AppendLine("pass.");
        }

        private bool IsError1(StringBuilder sb, int limit, OperationResult response, int count)
        {
            if (!response.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {response.Exception.Message}");
                return true;
            }
            if (count != limit)
            {
                sb.AppendLine($"warn. Reason:{Environment.NewLine} Expected {limit} items but was {count}");
                return true;
            }
            return false;
        }

        private bool IsError2<T>(StringBuilder sb, int limit, OperationResult<T> response, string offset)
        {
            if (!response.IsSuccess)
            {
                sb.AppendLine($"fail. Reason:{Environment.NewLine} {response.Exception.Message}");
                return true;
            }
            var rez = response.Result;

            var collection = rez as IList;
            if (collection != null)
            {
                var count = collection.Count;

                if (count != limit)
                {
                    sb.AppendLine($"warn. Reason:{Environment.NewLine} Expected {limit} items but was {count}");
                    return true;
                }

                if (!string.IsNullOrEmpty(offset))
                {
                    if (count == 0)
                        sb.AppendLine($"warn. Reason:{Environment.NewLine} Empty result");

                    var itm = collection[0];

                    var post = itm as Post;
                    if (post != null && !post.Url.Equals(offset))
                    {
                        sb.AppendLine($"warn. Reason:{Environment.NewLine} First url mast be {offset}");
                        return true;
                    }

                    var friend = itm as UserFriend;
                    if (friend != null && !friend.Author.Equals(offset))
                    {
                        sb.AppendLine($"warn. Reason:{Environment.NewLine} First Author mast be {offset}");
                        return true;
                    }

                    var searchResult = itm as SearchResult;
                    if (searchResult != null && !searchResult.Name.Equals(offset))
                    {
                        sb.AppendLine($"warn. Reason:{Environment.NewLine} First Name mast be {offset}");
                        return true;
                    }
                }
            }

            return false;
        }

        private Post GetFirstPostWhere(StringBuilder sb, PostType postType, Func<Post, bool> func)
        {
            var getPosts = new PostsModel(postType);
            Post testPost;
            do
            {
                var postsResp = _api.GetPostsAsync(getPosts, CancellationToken.None)
                    .Result;

                if (!postsResp.IsSuccess)
                {
                    sb.AppendLine($"fail. Reason:{Environment.NewLine} {postsResp.Exception.Message}");
                    return null;
                }
                if (postsResp.Result.Results.Count == 0)
                {
                    sb.AppendLine("fail. Reason:{Environment.NewLine} There are no Posts!");
                    return null;
                }

                testPost = postsResp.Result.Results.FirstOrDefault(func);
                if (testPost == null)
                    getPosts.Offset = postsResp.Result.Results.Last().Url;

            } while (testPost == null);
            return testPost;
        }

        #endregion
    }
}
