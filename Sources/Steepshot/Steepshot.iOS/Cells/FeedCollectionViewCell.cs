using System;
using System.Linq;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Extensions;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class FeedCollectionViewCell : BaseProfileCell
    {
        protected FeedCollectionViewCell(IntPtr handle) : base(handle)
        {

        }
        public static readonly NSString Key = new NSString(nameof(FeedCollectionViewCell));
        public static readonly UINib Nib;

        static FeedCollectionViewCell()
        {
            Nib = UINib.FromName(nameof(FeedCollectionViewCell), NSBundle.MainBundle);
        }

        public event Action<ActionType, Post> CellAction;

        private bool _isButtonBinded;
        //public event VoteEventHandler<OperationResult<VoteResponse>> Voted;
        //public event VoteEventHandler<OperationResult<VoteResponse>> Flagged;
        //public event HeaderTappedHandler GoToProfile;
        //public event HeaderTappedHandler GoToComments;
        //public event HeaderTappedHandler GoToVoters;
        //public event ImagePreviewHandler ImagePreview;
        private Post _currentPost;

        public bool IsCellActionSet => CellAction != null;
        ///public bool IsVotedSet => Voted != null;
        //public bool IsFlaggedSet => Flagged != null;
        //public bool IsGoToProfileSet => GoToProfile != null;
        //public bool IsGoToCommentsSet => GoToComments != null;
        //public bool IsGoToVotersSet => GoToVoters != null;
        //public bool IsImagePreviewSet => ImagePreview != null;
        private IScheduledWork _scheduledWorkAvatar;
        private IScheduledWork _scheduledWorkBody;

        /*
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            CellAction.Invoke(ActionType.Tap, null);
        }*/

        public override void UpdateCell(Post post)
        {
            _currentPost = post;
            avatarImage.Image = null;
            _scheduledWorkAvatar?.Cancel();

            bodyImage.Image = null;
            _scheduledWorkBody?.Cancel();

            _scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentPost.Avatar, TimeSpan.FromDays(30))
                                                     .WithCache(FFImageLoading.Cache.CacheType.All)
                                                     .DownSample(200)
                                                     .LoadingPlaceholder("ic_noavatar.png")
                                                     .ErrorPlaceholder("ic_noavatar.png")
                                                     .Into(avatarImage);
            
            var photo = _currentPost.Photos?.FirstOrDefault();
            if (photo != null)
                _scheduledWorkBody = ImageService.Instance.LoadUrl(photo, Steepshot.iOS.Helpers.Constants.ImageCacheDuration)
                                                         .WithCache(FFImageLoading.Cache.CacheType.All)
                                                         .DownSample((int)UIScreen.MainScreen.Bounds.Width)
                                                         .Into(bodyImage);

            topLikers.Hidden = true;
            if (_currentPost.TopLikersAvatars.Count() >= 1 && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[0]))
            {
                topLikers.Hidden = false;
                firstLiker.Hidden = false;
                /*_scheduledWorkAvatar = */ ImageService.Instance.LoadUrl(_currentPost.TopLikersAvatars[0], TimeSpan.FromDays(30))
                                                         .WithCache(FFImageLoading.Cache.CacheType.All)
                                                         .LoadingPlaceholder("ic_noavatar.png")
                                                         .ErrorPlaceholder("ic_noavatar.png")
                                                         .DownSample(width: 100)
                                                         .Into(firstLiker);
            }
            else
                firstLiker.Hidden = true;
                //firstLiker.Image = UIImage.FromBundle("ic_noavatar");

            if (_currentPost.TopLikersAvatars.Count() >= 2 && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[1]))
            {
                secondLiker.Hidden = false;
                /*_scheduledWorkAvatar = */
                ImageService.Instance.LoadUrl(_currentPost.TopLikersAvatars[1], TimeSpan.FromDays(30))
                             .WithCache(FFImageLoading.Cache.CacheType.All)
                            .LoadingPlaceholder("ic_noavatar.png")
                             .ErrorPlaceholder("ic_noavatar.png")
                             .DownSample(width: 100)
                            .Into(secondLiker);
            }
            else
                secondLiker.Hidden = true;
                //secondLiker.Image = UIImage.FromBundle("ic_noavatar");

            if (_currentPost.TopLikersAvatars.Count() >= 3 && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[2]))
            {
                thirdLiker.Hidden = false;
                /*_scheduledWorkAvatar = */
                ImageService.Instance.LoadUrl(_currentPost.TopLikersAvatars[2], TimeSpan.FromDays(30))
                             .WithCache(FFImageLoading.Cache.CacheType.All)
                            .LoadingPlaceholder("ic_noavatar.png")
                            .ErrorPlaceholder("ic_noavatar.png")
                            .DownSample(width: 100)
                            .Into(thirdLiker);
            }
            else
                thirdLiker.Hidden = true;
                //thirdLiker.Image = UIImage.FromBundle("ic_noavatar");

            //topLikers.LayoutIfNeeded();

            //var t = topLikers.Frame.Width;

            cellText.Text = _currentPost.Author;
            rewards.Hidden = !BasePresenter.User.IsNeedRewards;
            rewards.Text = BaseViewController.ToFormatedCurrencyString(_currentPost.TotalPayoutReward);

            netVotes.Text = $"{_currentPost.NetVotes} {Localization.Messages.Likes}";
            likeButton.Selected = _currentPost.Vote;
            flagButton.Selected = _currentPost.Flag;
            commentText.Text = _currentPost.Title;
            var buttonTitle = _currentPost.Children == 0 ? Localization.Messages.PostFirstComment : string.Format(Localization.Messages.ViewComments, _currentPost.Children);
            viewCommentButton.SetTitle(buttonTitle, UIControlState.Normal);
            likeButton.Enabled = true;
            flagButton.Enabled = true;
            postTimeStamp.Text = _currentPost.Created.ToPostTime();

            imageHeight.Constant = PhotoHeight.Get(_currentPost.ImageSize);
            contentViewWidth.Constant = UIScreen.MainScreen.Bounds.Width;
            //ContentView
            //if (_currentPost.ImageSize.Width != 0)
                //bodyImage.ContentMode = UIViewContentMode.ScaleAspectFill;
            //else
                //bodyImage.ContentMode = UIViewContentMode.ScaleAspectFit;

            if (!_isButtonBinded)
            {
                cellText.Font = Helpers.Constants.Semibold14;
                postTimeStamp.Font = Helpers.Constants.Regular12;
                netVotes.Font = Helpers.Constants.Semibold14;
                rewards.Font = Helpers.Constants.Semibold14;
                commentText.Font = Helpers.Constants.Regular14;
                viewCommentButton.Font = Helpers.Constants.Regular14;

                avatarImage.Layer.CornerRadius = avatarImage.Frame.Size.Width / 2;
                firstLiker.Layer.CornerRadius = firstLiker.Frame.Size.Width / 2;
                secondLiker.Layer.CornerRadius = secondLiker.Frame.Size.Width / 2;
                thirdLiker.Layer.CornerRadius = thirdLiker.Frame.Size.Width / 2;

                viewCommentButton.TouchDown += (sender, e) => 
                {
                    CellAction?.Invoke(ActionType.Comments, _currentPost);
                };

                UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
                {
                    var photoUrl = _currentPost.Photos?.FirstOrDefault();
                    if (photoUrl != null)
                        CellAction?.Invoke(ActionType.Preview, _currentPost);
                });
                bodyImage.AddGestureRecognizer(tap);

                UITapGestureRecognizer imageTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Profile, _currentPost);
                    //GoToProfile(_currentPost.Author);
                });
                UITapGestureRecognizer textTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Profile, _currentPost);
                    //GoToProfile(_currentPost.Author);
                });
                UITapGestureRecognizer moneyTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Profile, _currentPost);
                    //GoToProfile(_currentPost.Author);
                });
                avatarImage.AddGestureRecognizer(imageTap);
                cellText.AddGestureRecognizer(textTap);
                rewards.AddGestureRecognizer(moneyTap);

                UITapGestureRecognizer commentTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Comments, _currentPost);
                    //GoToComments(_currentPost.Url);
                });
                commentView.AddGestureRecognizer(commentTap);

                UITapGestureRecognizer netVotesTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Voters, _currentPost);
                    //GoToVoters(_currentPost.Url);
                });
                netVotes.AddGestureRecognizer(netVotesTap);

                flagButton.TouchDown += FlagButton_TouchDown;
                //likeButton.TouchDown += LikeTap;
                likeButton.TouchDown += (object sender, EventArgs e) => 
                {
                    CellAction?.Invoke(ActionType.Like, _currentPost);
                };

                _isButtonBinded = true;
            }
            //LayoutIfNeeded();
        }

        private void LikeTap(object sender, EventArgs e)
        {
            likeButton.Enabled = false;
            //Voted(!likeButton.Selected, _currentPost, VotedAction);
        }

        private void VotedAction(Post post, OperationResult<VoteResponse> operationResult)
        {
            if (string.Equals(post.Url, _currentPost.Url, StringComparison.OrdinalIgnoreCase) && operationResult.Success)
            {
                likeButton.Selected = operationResult.Result.IsSuccess;
                flagButton.Selected = _currentPost.Flag;
                rewards.Text = BaseViewController.ToFormatedCurrencyString(operationResult.Result.NewTotalPayoutReward);
                netVotes.Text = $"{_currentPost.NetVotes.ToString()} {Localization.Messages.Likes}";
            }
            likeButton.Enabled = true;
        }

        private void FlagButton_TouchDown(object sender, EventArgs e)
        {
            flagButton.Enabled = false;
            //Flagged?.Invoke(!flagButton.Selected, _currentPost, FlaggedAction);
        }

        private void FlaggedAction(Post post, OperationResult<VoteResponse> result)
        {
            if (result.Success && string.Equals(post.Url, _currentPost.Url, StringComparison.OrdinalIgnoreCase))
            {
                flagButton.Selected = result.Result.IsSuccess;
                likeButton.Selected = _currentPost.Vote;
                netVotes.Text = $"{_currentPost.NetVotes.ToString()} {Localization.Messages.Likes}";
                rewards.Text = BaseViewController.ToFormatedCurrencyString(result.Result.NewTotalPayoutReward);
            }
            flagButton.Selected = _currentPost.Flag;
            flagButton.Enabled = true;
        }
    }
}
