using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Widget;
using Java.IO;
using Steepshot.Core;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public abstract class PostCreateBaseFragment : PostPrepareBaseFragment
    {
        public PostCreateBaseFragment(GalleryMediaModel media) : base(media)
        {
        }

        public PostCreateBaseFragment(List<GalleryMediaModel> media) : base(media)
        {
        }

        protected async Task CheckOnSpam(bool disableEditing)
        {
            EnablePostAndEdit(false, disableEditing);
            isSpammer = false;

            var spamCheck = await Presenter.TryCheckForSpam(AppSettings.User.Login);

            if (spamCheck.IsSuccess)
            {
                if (!spamCheck.Result.IsSpam)
                {
                    if (spamCheck.Result.WaitingTime > 0)
                    {
                        isSpammer = true;
                        PostingLimit = TimeSpan.FromMinutes(5);
                        StartPostTimer((int)spamCheck.Result.WaitingTime);
                        Activity.ShowAlert(LocalizationKeys.Posts5minLimit, ToastLength.Long);
                    }
                    else
                    {
                        EnabledPost();
                    }
                }
                else
                {
                    // more than 15 posts
                    isSpammer = true;
                    PostingLimit = TimeSpan.FromHours(24);
                    StartPostTimer((int)spamCheck.Result.WaitingTime);
                    Activity.ShowAlert(LocalizationKeys.PostsDayLimit, ToastLength.Long);
                }
            }

            EnablePostAndEdit(true);
        }

        private async void StartPostTimer(int startSeconds)
        {
            var timepassed = PostingLimit - TimeSpan.FromSeconds(startSeconds);

            while (timepassed < PostingLimit)
            {
                if (!IsInitialized)
                    return;
                var delay = PostingLimit - timepassed;
                var timeFormat = delay.TotalHours >= 1 ? "hh\\:mm\\:ss" : "mm\\:ss";
                _postButton.Text = delay.ToString(timeFormat);
                _postButton.Enabled = false;
                await Task.Delay(1000);
                timepassed = timepassed.Add(TimeSpan.FromSeconds(1));
            }

            isSpammer = false;
            EnabledPost();
        }

        protected async Task<OperationResult<MediaModel>> UploadMedia(string path)
        {
            System.IO.Stream stream = null;
            FileInputStream fileInputStream = null;

            try
            {
                var photo = new File(path);
                fileInputStream = new FileInputStream(photo);
                stream = new StreamConverter(fileInputStream, null);

                var request = new UploadMediaModel(AppSettings.User.UserInfo, stream, System.IO.Path.GetExtension(path));
                var serverResult = await Presenter.TryUploadMedia(request);
                return serverResult;
            }
            catch (Exception ex)
            {
                await AppSettings.Logger.Error(ex);
                return new OperationResult<MediaModel>(new InternalException(LocalizationKeys.PhotoUploadError, ex));
            }
            finally
            {
                fileInputStream?.Close(); // ??? change order?
                stream?.Flush();
                fileInputStream?.Dispose();
                stream?.Dispose();
            }
        }

        public override void OnDetach()
        {
            CleanCash();
            base.OnDetach();
        }

        private void CleanCash()
        {
            var files = Context.CacheDir.ListFiles();
            foreach (var file in files)
                if (file.Path.EndsWith(Constants.Steepshot))
                    file.Delete();
        }
    }
}