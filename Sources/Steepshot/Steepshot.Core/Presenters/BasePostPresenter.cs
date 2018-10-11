using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Extensions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class BasePostPresenter : ListPresenter<Post>, IDisposable
    {
        public static bool IsEnableVote { get; set; }
        protected readonly BaseDitchClient DitchClient;
        protected readonly SteepshotApiClient SteepshotApiClient;
        protected readonly SteepshotClient SteepshotClient;
        protected readonly User User;


        public BasePostPresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient, SteepshotApiClient steepshotApiClient, User user, SteepshotClient steepshotClient)
            : base(connectionService, logService)
        {
            DitchClient = ditchClient;
            SteepshotApiClient = steepshotApiClient;
            User = user;
            SteepshotClient = steepshotClient;
            IsEnableVote = true;
        }

        public async Task<OperationResult<VoidResponse>> TryDeletePostAsync(Post post)
        {
            var request = new DeleteModel(User.UserInfo, post);

            var result = await TaskHelper
                .TryRunTaskAsync(DeletePostOrCommentAsync, request, OnDisposeCts.Token)
                .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                lock (Items)
                {
                    Items.Remove(post);
                    CashManager.RemoveRef(post);
                }
            }

            NotifySourceChanged(nameof(TryDeletePostAsync), true);
            return result;
        }

        private async Task<OperationResult<VoidResponse>> DeletePostOrCommentAsync(DeleteModel model, CancellationToken ct)
        {
            if (model.IsEnableToDelete)
            {
                var operationResult = await DitchClient
                    .DeleteAsync(model, ct)
                    .ConfigureAwait(false);

                if (operationResult.IsSuccess)
                {
                    //log parent post to perform update
                    if (model.IsPost)
                        await SteepshotApiClient.TraceAsync($"post/@{model.Author}/{model.Permlink}/delete", model.Login, operationResult.Exception, $"@{model.Author}/{model.Permlink}", ct).ConfigureAwait(false);
                    else
                        await SteepshotApiClient.TraceAsync($"post/@{model.ParentAuthor}/{model.ParentPermlink}/comment", model.Login, operationResult.Exception, $"@{model.ParentAuthor}/{model.ParentPermlink}", ct).ConfigureAwait(false);

                    return operationResult;
                }
            }

            var result = await DitchClient.CreateOrEditAsync(model, ct).ConfigureAwait(false);

            //log parent post to perform update
            if (model.IsPost)
                await SteepshotApiClient.TraceAsync($"post/@{model.Author}/{model.Permlink}/edit", model.Login, result.Exception, $"@{model.Author}/{model.Permlink}", ct).ConfigureAwait(false);
            else
                await SteepshotApiClient.TraceAsync($"post/@{model.ParentAuthor}/{model.ParentPermlink}/comment", model.Login, result.Exception, $"@{model.ParentAuthor}/{model.ParentPermlink}", ct).ConfigureAwait(false);

            return result;
        }

        public async Task<OperationResult<VoidResponse>> TryDeleteCommentAsync(Post post, Post parentPost)
        {
            var request = new DeleteModel(User.UserInfo, post, parentPost);
            var response = await TaskHelper
                .TryRunTaskAsync(DeletePostOrCommentAsync, request, OnDisposeCts.Token)
                .ConfigureAwait(false);
            if (response.IsSuccess)
            {
                lock (Items)
                    Items.Remove(post);
            }
            NotifySourceChanged(nameof(TryDeletePostAsync), true);
            return response;
        }

        public void HidePost(Post post)
        {
            if (!User.PostBlackList.Contains(post.Url))
            {
                User.PostBlackList.Add(post.Url);
                User.Save();
            }

            lock (Items)
                Items.Remove(post);

            NotifySourceChanged(nameof(HidePost), true);
        }

        protected bool ResponseProcessing(OperationResult<ListResponse<Post>> response, int itemsLimit, out Exception exception, string sender, bool isNeedClearItems = false, bool enableEmptyMedia = false)
        {
            exception = null;
            if (response == null)
                return false;

            if (response.IsSuccess)
            {
                var results = response.Result.Results;
                if (results.Count > 0)
                {
                    var isAdded = false;
                    lock (Items)
                    {
                        if (isNeedClearItems)
                            Clear(false);

                        foreach (var item in results)
                        {
                            if (User.PostBlackList.Contains(item.Url))
                                continue;

                            if (!Items.Any(itm => itm.Url.Equals(item.Url, StringComparison.OrdinalIgnoreCase))
                                && (enableEmptyMedia || IsValidMedia(item)))
                            {
                                var refItem = CashManager.Add(item);
                                Items.Add(refItem);
                                isAdded = true;
                            }
                        }
                    }

                    if (isAdded)
                    {
                        OffsetUrl = response.Result.Offset;
                    }
                    else if (!string.Equals(OffsetUrl, response.Result.Offset, StringComparison.OrdinalIgnoreCase))
                    {
                        OffsetUrl = response.Result.Offset;
                        return true;
                    }

                    NotifySourceChanged(sender, isAdded);
                }
                if (results.Count < Math.Min(ServerMaxCount, itemsLimit))
                    IsLastReaded = true;
            }
            exception = response.Exception;
            return false;
        }

        protected bool IsValidMedia(Post item)
        {
            //This part of the server logic, but... let`s check that everything is okay
            if (item.Media == null || item.Media.Length == 0)
                return false;

            item.Media = item.Media.Where(i => !string.IsNullOrEmpty(i.Url)).ToArray();

            if (item.Media.Length == 0)
                return false;

            foreach (var itm in item.Media)
            {
                if (itm.Size == null || itm.Size.Height == 0 || itm.Size.Width == 0)
                {
                    itm.Size = new FrameSize(1024, 1024);
                }

                if (itm.Thumbnails == null)
                    itm.Thumbnails = new Thumbnails();

                itm.Thumbnails.DefaultUrl = itm.Url;
            }
            return true;
        }

        public async Task<OperationResult<Post>> TryVoteAsync(Post post)
        {
            if (post == null || post.VoteChanging || post.FlagChanging)
                return new OperationResult<Post>(new OperationCanceledException());

            IsEnableVote = false;
            post.VoteChanging = true;

            var wasFlaged = post.Flag;
            var request = new VoteModel(User.UserInfo, post, post.Vote ? VoteType.Down : VoteType.Up);
            var response = await TaskHelper.TryRunTaskAsync(VoteAsync, request, OnDisposeCts.Token).ConfigureAwait(true);

            if (response.IsSuccess)
            {
                if (post.IsComment)
                    ChangeLike(post, wasFlaged);
                else
                    CashManager.Add(response.Result);
            }

            IsEnableVote = true;
            post.VoteChanging = false;

            return response;
        }

        private async Task<OperationResult<Post>> VoteAsync(VoteModel model, CancellationToken ct)
        {
            var result = await DitchClient.VoteAsync(model, ct).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                if (result.Exception is RequestException requestException && !string.IsNullOrEmpty(requestException.RawResponse)
                    && (requestException.RawResponse.Contains(Constants.VotedInASimilarWaySteem) || requestException.RawResponse.Contains(Constants.VotedInASimilarWayGolos)))
                {
                    //try to update post
                }
                else
                {
                    return new OperationResult<Post>(result.Exception);
                }
            }


            var startDelay = DateTime.Now;

            await SteepshotApiClient.TraceAsync($"post/@{model.Author}/{model.Permlink}/{model.Type.GetDescription()}", model.Login, null, $"@{model.Author}/{model.Permlink}", ct).ConfigureAwait(false);

            OperationResult<Post> postInfo;
            if (model.IsComment) //TODO: << delete when comment update support will added on backend
            {
                postInfo = new OperationResult<Post> { Result = model.Post };
            }
            else
            {
                var infoModel = new NamedInfoModel($"@{model.Author}/{model.Permlink}")
                {
                    Login = model.Login,
                    ShowLowRated = true,
                    ShowNsfw = true
                };
                postInfo = await SteepshotApiClient.GetPostInfoAsync(infoModel, ct).ConfigureAwait(false);
            }

            var delay = (int)(model.VoteDelay - (DateTime.Now - startDelay).TotalMilliseconds);
            if (delay > 100)
                await Task.Delay(delay, ct).ConfigureAwait(false);

            return postInfo;
        }

        private void ChangeLike(Post post, bool wasFlaged)
        {
            post.Vote = !post.Vote;
            post.Flag = false;
            post.NetLikes = post.Vote ? post.NetLikes + 1 : post.NetLikes - 1;
            if (wasFlaged)
                post.NetFlags--;
        }

        public async Task<OperationResult<Post>> TryFlagAsync(Post post)
        {
            if (post == null || post.VoteChanging || post.FlagChanging)
                return new OperationResult<Post>(new OperationCanceledException());

            post.FlagNotificationWasShown = post.Flag;
            IsEnableVote = false;
            post.FlagChanging = true;

            var wasVote = post.Vote;
            var request = new VoteModel(User.UserInfo, post, post.Flag ? VoteType.Down : VoteType.Flag);
            var response = await TaskHelper.TryRunTaskAsync(VoteAsync, request, OnDisposeCts.Token).ConfigureAwait(true);

            if (response.IsSuccess)
            {
                if (post.IsComment)
                {
                    ChangeFlag(post, wasVote);
                }
                else
                {
                    CashManager.Add(response.Result);
                }
            }

            IsEnableVote = true;
            post.FlagChanging = false;

            return response;
        }

        private void ChangeFlag(Post post, bool wasVote)
        {
            post.Flag = !post.Flag;
            post.Vote = false;
            post.NetFlags = post.Flag ? post.NetFlags + 1 : post.NetFlags - 1;
            if (wasVote)
                post.NetLikes--;
        }

        public override void Clear(bool isNotify)
        {
            lock (Items)
                CashManager.RemoveAll(Items);

            base.Clear(isNotify);
        }

        public async Task<OperationResult<string>> CheckServiceStatusAsync()
        {
            return await TaskHelper
                .TryRunTaskAsync(SteepshotClient.CheckRegistrationServiceStatusAsync, OnDisposeCts.Token)
                .ConfigureAwait(false);
        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (Items)
                    {
                        CashManager.RemoveAll(Items);
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BasePostPresenter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
