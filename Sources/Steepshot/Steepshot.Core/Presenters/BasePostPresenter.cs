using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Authorization;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Ditch.Core.JsonRpc;

namespace Steepshot.Core.Presenters
{
    public class BasePostPresenter : ListPresenter<Post>, IDisposable
    {
        public static bool IsEnableVote { get; set; }

        protected BasePostPresenter()
        {
            IsEnableVote = true;
        }

        public async Task<Exception> TryDeletePost(Post post)
        {
            if (post == null)
                return null;

            var exception = await TryRunTask(DeletePost, OnDisposeCts.Token, post);
            NotifySourceChanged(nameof(TryDeletePost), true);
            return exception;
        }

        private async Task<Exception> DeletePost(Post post, CancellationToken ct)
        {
            var request = new DeleteModel(AppSettings.User.UserInfo, post);
            var response = await Api.DeletePostOrComment(request, ct);
            if (response.IsSuccess)
            {
                lock (Items)
                {
                    Items.Remove(post);
                    CashPresenterManager.RemoveRef(post);
                }
            }
            return response.Exception;
        }

        public async Task<Exception> TryDeleteComment(Post post, Post parentPost)
        {
            if (post == null || parentPost == null)
                return null;

            var exception = await TryRunTask(DeleteComment, OnDisposeCts.Token, post, parentPost);
            NotifySourceChanged(nameof(TryDeletePost), true);
            return exception;
        }

        private async Task<Exception> DeleteComment(Post post, Post parentPost, CancellationToken ct)
        {
            var request = new DeleteModel(AppSettings.User.UserInfo, post, parentPost);
            var response = await Api.DeletePostOrComment(request, ct);
            if (response.IsSuccess)
            {
                lock (Items)
                {
                    Items.Remove(post);
                }
            }
            return response.Exception;
        }

        public async Task<Exception> TryEditComment(UserInfo userInfo, Post parentPost, Post post, string body, IAppInfo appInfo)
        {
            if (string.IsNullOrEmpty(body) || parentPost == null || post == null)
                return null;

            var model = new CreateOrEditCommentModel(userInfo, parentPost, post, body, appInfo);
            var exception = await TryRunTask(EditComment, OnDisposeCts.Token, model, post);
            NotifySourceChanged(nameof(TryEditComment), true);
            return exception;
        }

        private async Task<Exception> EditComment(CreateOrEditCommentModel model, Post post, CancellationToken ct)
        {
            var response = await Api.CreateOrEditComment(model, ct);
            if (response.IsSuccess)
                post.Body = model.Body;
            return response.Exception;
        }

        public async Task<OperationResult<PromoteResponse>> FindPromoteBot(PromoteRequest request)
        {
            return await Api.FindPromoteBot(request);
        }

        //copypaste, need to remake architecture
        public async Task<OperationResult<VoidResponse>> TryTransfer(UserInfo userInfo, string recipient, string amount, CurrencyType type, string memo = null)
        {
            var transferModel = new TransferModel(userInfo, recipient, amount, type);

            if (!string.IsNullOrEmpty(memo))
                transferModel.Memo = memo;

            return await TryRunTask<TransferModel, VoidResponse>(Transfer, OnDisposeCts.Token, transferModel);
        }
        //copypaste, need to remake architecture
        private Task<OperationResult<VoidResponse>> Transfer(TransferModel model, CancellationToken ct)
        {
            return Api.Transfer(model, ct);
        }

        //copypaste, need to remake architecture
        public async Task<OperationResult<AccountInfoResponse>> TryGetAccountInfo(string login)
        {
            return await TryRunTask<string, AccountInfoResponse>(GetAccountInfo, OnDisposeCts.Token, login);
        }
        //copypaste, need to remake architecture
        protected Task<OperationResult<AccountInfoResponse>> GetAccountInfo(string login, CancellationToken ct)
        {
            return Api.GetAccountInfo(login, ct);
        }

        public void HidePost(Post post)
        {
            if (!AppSettings.User.PostBlackList.Contains(post.Url))
            {
                AppSettings.User.PostBlackList.Add(post.Url);
                AppSettings.User.Save();
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
                            if (AppSettings.User.PostBlackList.Contains(item.Url))
                                continue;

                            if (Items.Count == 0)
                            {
                                var media = item.Media[0];
                                media.Url = "http://steepshot.org/api/v1/image/a9a8f651-9617-42ca-9d30-22562509e44e.m3u8"; //"https://www.ixbt.com/multimedia/video-methodology/bitrates/avc-1080-25p/1080-25p-10mbps.mp4";
                                media.Size.Height = 1080;
                                media.Size.Width = 1080;
                                media.Thumbnails.Mini = "https://dtxu61vdboi82.cloudfront.net/2018-08-23/a9a8f651-9617-42ca-9d30-22562509e44e_thumbnail_1024p.jpeg";
                                media.Thumbnails.Micro = "https://dtxu61vdboi82.cloudfront.net/2018-08-23/a9a8f651-9617-42ca-9d30-22562509e44e_thumbnail_256p.jpeg";
                                media.ContentType = MimeTypeHelper.GetMimeType(MimeTypeHelper.Mp4);
                            }


                            if (!Items.Any(itm => itm.Url.Equals(item.Url, StringComparison.OrdinalIgnoreCase))
                                && (enableEmptyMedia || IsValidMedia(item)))
                            {
                                var refItem = CashPresenterManager.Add(item);
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

        public async Task<Exception> TryVote(Post post)
        {
            if (post == null || post.VoteChanging || post.FlagChanging)
                return null;

            post.VoteChanging = true;
            IsEnableVote = false;
            NotifySourceChanged(nameof(TryVote), true);

            var exception = await TryRunTask(Vote, OnDisposeCts.Token, post);

            post.VoteChanging = false;
            IsEnableVote = true;
            NotifySourceChanged(nameof(TryVote), true);

            return exception;
        }

        private async Task<Exception> Vote(Post post, CancellationToken ct)
        {
            var wasFlaged = post.Flag;
            var request = new VoteModel(AppSettings.User.UserInfo, post, post.Vote ? VoteType.Down : VoteType.Up);
            var response = await Api.Vote(request, ct);

            if (response.IsSuccess)
            {
                if (post.IsComment)
                {
                    ChangeLike(post, wasFlaged);
                }
                else
                {
                    CashPresenterManager.Add(response.Result);
                }
            }
            else if (response.Exception is RequestException requestException)
            {
                //TODO:KOA: bad solution...
                if (requestException.RawResponse.Contains(Constants.VotedInASimilarWaySteem) ||
                    requestException.RawResponse.Contains(Constants.VotedInASimilarWayGolos))
                {
                    response.Exception = null;
                    ChangeLike(post, wasFlaged);
                }
            }

            return response.Exception;
        }

        private void ChangeLike(Post post, bool wasFlaged)
        {
            post.Vote = !post.Vote;
            post.Flag = false;
            post.NetLikes = post.Vote ? post.NetLikes + 1 : post.NetLikes - 1;
            if (wasFlaged)
                post.NetFlags--;
        }

        public async Task<Exception> TryFlag(Post post)
        {
            if (post == null || post.VoteChanging || post.FlagChanging)
                return null;

            post.FlagNotificationWasShown = post.Flag;
            post.FlagChanging = true;
            IsEnableVote = false;
            NotifySourceChanged(nameof(TryFlag), true);

            var exception = await TryRunTask(Flag, OnDisposeCts.Token, post);

            post.FlagChanging = false;
            IsEnableVote = true;
            NotifySourceChanged(nameof(TryFlag), true);

            return exception;
        }

        private async Task<Exception> Flag(Post post, CancellationToken ct)
        {
            var wasVote = post.Vote;
            var request = new VoteModel(AppSettings.User.UserInfo, post, post.Flag ? VoteType.Down : VoteType.Flag);
            var response = await Api.Vote(request, ct);

            if (response.IsSuccess)
            {
                if (post.IsComment)
                {
                    ChangeFlag(post, wasVote);
                }
                else
                {
                    CashPresenterManager.Add(response.Result);
                }
            }
            else if (response.Exception is RequestException requestException)
            {
                //TODO:KOA: bad solution...
                if (requestException.RawResponse.Contains(Constants.VotedInASimilarWaySteem) ||
                    requestException.RawResponse.Contains(Constants.VotedInASimilarWayGolos))
                {
                    response.Exception = null;
                    ChangeFlag(post, wasVote);
                }
            }
            return response.Exception;
        }

        private void ChangeFlag(Post post, bool wasVote)
        {
            post.Flag = !post.Flag;
            post.Vote = false;
            post.NetFlags = post.Flag ? post.NetFlags + 1 : post.NetFlags - 1;
            if (wasVote)
                post.NetLikes--;
        }

        public override void Clear(bool isNotify = true)
        {
            lock (Items)
            {
                CashPresenterManager.RemoveAll(Items);
            }

            base.Clear(isNotify);
        }

        public async Task<OperationResult<string>> CheckServiceStatus()
        {
            return await Api.CheckRegistrationServiceStatus(CancellationToken.None);
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (Items)
                    {
                        CashPresenterManager.RemoveAll(Items);
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
