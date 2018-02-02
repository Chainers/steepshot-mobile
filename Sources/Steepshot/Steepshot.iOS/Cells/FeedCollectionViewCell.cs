using System;
using System.Linq;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;
using Xamarin.TTTAttributedLabel;
using PureLayout.Net;
using System.Diagnostics;
using CoreGraphics;

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
        public event Action<string> TagAction;

        private bool _isButtonBinded;
        private Post _currentPost;

        public bool IsCellActionSet => CellAction != null;
        private IScheduledWork _scheduledWorkAvatar;
        private IScheduledWork _scheduledWorkBody;

        private IScheduledWork _scheduledWorkfirst;
        private IScheduledWork _scheduledWorksecond;
        private IScheduledWork _scheduledWorkthird;

        private TTTAttributedLabel attributedLabel;

        public override void UpdateCell(Post post)
        {
            _currentPost = post;
            avatarImage.Image = null;
            _scheduledWorkAvatar?.Cancel();

            bodyImage.Image = null;
            _scheduledWorkBody?.Cancel();

            var media = _currentPost.Media[0];
            _scheduledWorkBody = ImageService.Instance.LoadUrl(media.Url, Helpers.Constants.ImageCacheDuration)
                                                     //.Retry(5)
                                                     .FadeAnimation(false)
                                                     .WithCache(FFImageLoading.Cache.CacheType.All)
                                                     .DownSample((int)UIScreen.MainScreen.Bounds.Width)
                                                     .WithPriority(LoadingPriority.Highest)
                                                     .Into(bodyImage);

            if (!string.IsNullOrEmpty(_currentPost.Avatar))
                _scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentPost.Avatar, TimeSpan.FromDays(30))
                                                         .WithCache(FFImageLoading.Cache.CacheType.All)
                                                         .FadeAnimation(false)
                                                         .DownSample(200)
                                                         .LoadingPlaceholder("ic_noavatar.png")
                                                         .ErrorPlaceholder("ic_noavatar.png")
                                                         .WithPriority(LoadingPriority.Normal)
                                                         .Into(avatarImage);
            else
                avatarImage.Image = UIImage.FromBundle("ic_noavatar");

            topLikers.Hidden = true;
            if (_currentPost.TopLikersAvatars.Count() >= 1 && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[0]))
            {
                _scheduledWorkfirst?.Cancel();
                firstLiker.Image = null;
                topLikers.Hidden = false;
                firstLiker.Hidden = false;
                _scheduledWorkfirst = ImageService.Instance.LoadUrl(_currentPost.TopLikersAvatars[0], TimeSpan.FromDays(30))
                                                         .WithCache(FFImageLoading.Cache.CacheType.All)
                                                         .LoadingPlaceholder("ic_noavatar.png")
                                                         .ErrorPlaceholder("ic_noavatar.png")
                                                         .DownSample(width: 100)
                                                         .WithPriority(LoadingPriority.Lowest)
                                                         .Into(firstLiker);
            }
            else
                firstLiker.Hidden = true;

            if (_currentPost.TopLikersAvatars.Count() >= 2 && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[1]))
            {
                _scheduledWorksecond?.Cancel();
                secondLiker.Image = null;
                secondLiker.Hidden = false;
                _scheduledWorksecond = ImageService.Instance.LoadUrl(_currentPost.TopLikersAvatars[1], TimeSpan.FromDays(30))
                             .WithCache(FFImageLoading.Cache.CacheType.All)
                             .LoadingPlaceholder("ic_noavatar.png")
                             .ErrorPlaceholder("ic_noavatar.png")
                             .WithPriority(LoadingPriority.Lowest)
                             .DownSample(width: 100)
                             .Into(secondLiker);
            }
            else
                secondLiker.Hidden = true;

            if (_currentPost.TopLikersAvatars.Count() >= 3 && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[2]))
            {
                _scheduledWorkthird?.Cancel();
                thirdLiker.Image = null;
                thirdLiker.Hidden = false;
                _scheduledWorkthird = ImageService.Instance.LoadUrl(_currentPost.TopLikersAvatars[2], TimeSpan.FromDays(30))
                            .WithCache(FFImageLoading.Cache.CacheType.All)
                            .LoadingPlaceholder("ic_noavatar.png")
                            .ErrorPlaceholder("ic_noavatar.png")
                            .WithPriority(LoadingPriority.Lowest)
                            .DownSample(width: 100)
                            .Into(thirdLiker);
            }
            else
                thirdLiker.Hidden = true;

            cellText.Text = _currentPost.Author;
            rewards.Hidden = !BasePresenter.User.IsNeedRewards;
            //rewards.Text = BaseViewController.ToFormatedCurrencyString(_currentPost.TotalPayoutReward);

            netVotes.Text = $"{_currentPost.NetVotes} {Localization.Messages.Likes}";

            if (_currentPost.VoteChanging)
                Animate();
            else
            {
                likeButton.Transform = CGAffineTransform.MakeScale(1f, 1f);
                likeButton.Selected = _currentPost.Vote;
            }

            flagButton.Selected = _currentPost.Flag;
            viewCommentText.Text = _currentPost.Children == 0 ? Localization.Messages.PostFirstComment : string.Format(Localization.Messages.ViewComments, _currentPost.Children);
            likeButton.Enabled = true;
            flagButton.Enabled = true;
            postTimeStamp.Text = _currentPost.Created.ToPostTime();

            imageHeight.Constant = PhotoHeight.Get(media.Size);
            contentViewWidth.Constant = UIScreen.MainScreen.Bounds.Width;

            if (!_isButtonBinded)
            {
                cellText.Font = Helpers.Constants.Semibold14;
                postTimeStamp.Font = Helpers.Constants.Regular12;
                netVotes.Font = Helpers.Constants.Semibold14;
                rewards.Font = Helpers.Constants.Semibold14;
                viewCommentText.Font = Helpers.Constants.Regular14;

                avatarImage.Layer.CornerRadius = avatarImage.Frame.Size.Width / 2;
                firstLiker.Layer.CornerRadius = firstLiker.Frame.Size.Width / 2;
                secondLiker.Layer.CornerRadius = secondLiker.Frame.Size.Width / 2;
                thirdLiker.Layer.CornerRadius = thirdLiker.Frame.Size.Width / 2;

                attributedLabel = new TTTAttributedLabel();
                attributedLabel.EnabledTextCheckingTypes = NSTextCheckingType.Link;
                var prop = new NSDictionary();
                attributedLabel.LinkAttributes = prop;
                attributedLabel.ActiveLinkAttributes = prop;

                commentView.AddSubview(attributedLabel);
                attributedLabel.Font = Helpers.Constants.Regular14;
                attributedLabel.Lines = 0;
                attributedLabel.UserInteractionEnabled = true;
                attributedLabel.Enabled = true;
                attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15f);
                attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15f);
                attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 15f);
                viewCommentText.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, attributedLabel, 5f);
                attributedLabel.Delegate = new TTTAttributedLabelFeedDelegate(TagAction);

                UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Preview, _currentPost);
                });
                bodyImage.AddGestureRecognizer(tap);

                UITapGestureRecognizer imageTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Profile, _currentPost);
                });
                UITapGestureRecognizer textTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Profile, _currentPost);
                });
                UITapGestureRecognizer moneyTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Profile, _currentPost);
                });
                avatarImage.AddGestureRecognizer(imageTap);
                cellText.AddGestureRecognizer(textTap);
                rewards.AddGestureRecognizer(moneyTap);

                UITapGestureRecognizer commentTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Comments, _currentPost);
                });
                viewCommentText.AddGestureRecognizer(commentTap);

                UITapGestureRecognizer netVotesTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Voters, _currentPost);
                });
                netVotes.AddGestureRecognizer(netVotesTap);

                flagButton.TouchDown += FlagButton_TouchDown;
                likeButton.TouchDown += LikeTap;

                _isButtonBinded = true;

                Debug.WriteLine("Cell created");
            }

            var noLinkAttribute = new UIStringAttributes
            {
                Font = Helpers.Constants.Regular14,
                ForegroundColor = Helpers.Constants.R15G24B30,
            };

            var at = new NSMutableAttributedString();

            at.Append(new NSAttributedString(_currentPost.Title, noLinkAttribute));
            if (!string.IsNullOrEmpty(_currentPost.Description))
            {
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(_currentPost.Description, noLinkAttribute));
            }

            foreach (var tag in _currentPost.Tags)
            {
                if (tag == "steepshot")
                    continue;
                var linkAttribute = new UIStringAttributes
                {
                    Link = new NSUrl(tag),
                    Font = Helpers.Constants.Regular14,
                    ForegroundColor = Helpers.Constants.R231G72B0,
                };
                at.Append(new NSAttributedString($" #{tag}", linkAttribute));
            }
            attributedLabel.SetText(at);
        }

        private void LikeTap(object sender, EventArgs e)
        {
            CellAction?.Invoke(ActionType.Like, _currentPost);
            Animate();
        }

        private void Animate()
        {
            likeButton.Selected = true;
            UIView.Animate(0.4, 0, UIViewAnimationOptions.Autoreverse | UIViewAnimationOptions.Repeat | UIViewAnimationOptions.CurveEaseIn, () =>
            {
                likeButton.Transform = CGAffineTransform.MakeScale(0.5f, 0.5f);
            }, null);
        }

        private void VotedAction(Post post, OperationResult<VoteResponse> operationResult)
        {
            if (string.Equals(post.Url, _currentPost.Url, StringComparison.OrdinalIgnoreCase) && operationResult.IsSuccess)
            {
                likeButton.Selected = operationResult.Result.IsSuccess;
                flagButton.Selected = _currentPost.Flag;
                //rewards.Text = BaseViewController.ToFormatedCurrencyString(operationResult.Result.NewTotalPayoutReward);
                netVotes.Text = $"{_currentPost.NetVotes.ToString()} {Localization.Messages.Likes}";
            }
            likeButton.Enabled = true;
        }

        private void FlagButton_TouchDown(object sender, EventArgs e)
        {
            CellAction?.Invoke(ActionType.More, _currentPost);
        }

        private void FlaggedAction(Post post, OperationResult<VoteResponse> result)
        {
            if (result.IsSuccess && string.Equals(post.Url, _currentPost.Url, StringComparison.OrdinalIgnoreCase))
            {
                flagButton.Selected = result.Result.IsSuccess;
                likeButton.Selected = _currentPost.Vote;
                netVotes.Text = $"{_currentPost.NetVotes.ToString()} {Localization.Messages.Likes}";
                //rewards.Text = BaseViewController.ToFormatedCurrencyString(result.Result.NewTotalPayoutReward);
            }
            flagButton.Selected = _currentPost.Flag;
            flagButton.Enabled = true;
        }
    }

    public class TTTAttributedLabelFeedDelegate : TTTAttributedLabelDelegate
    {
        private Action<string> _tagAction;

        public TTTAttributedLabelFeedDelegate(Action<string> tagAction)
        {
            _tagAction = tagAction;
        }

        public override void DidSelectLinkWithURL(TTTAttributedLabel label, NSUrl url)
        {
            _tagAction?.Invoke(url.Description);
        }
    }
}
