using System;
using FFImageLoading.Work;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Extensions;
using UIKit;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using CoreGraphics;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using MGSwipeTableCellXamarin;
using System.Collections.Generic;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.CustomViews;
using PureLayout.Net;
using Steepshot.iOS.Helpers;
using System.ComponentModel;

namespace Steepshot.iOS.Cells
{
    public class CommentTableViewCell : MGSwipeTableCell
    {
        private Post _currentPost;
        private IScheduledWork _scheduledWorkAvatar;
        private UIView[] rigthButtons;

        public bool IsCellActionSet => CellAction != null;
        public Action<ActionType, Post> CellAction;

        private MGSwipeButton deleteButton;
        private MGSwipeButton editButton;
        private MGSwipeButton flagButton;
        private SliderView _sliderView;
        private UIImageView _avatar;
        private UILabel _loginLabel;
        private UIView _profileTapView;
        private UILabel _timestamp;
        private UITextView _commentText;
        private UIImageView _like;
        private UILabel _replyLabel;
        private UILabel _likesLabel;
        private UILabel _flagsLabel;
        private UILabel _costLabel;
        private UIStackView _bottomView;

        private readonly UILongPressGestureRecognizer _likelongtap;
        private readonly UITapGestureRecognizer _tap;
        private readonly UITapGestureRecognizer _costTap;
        private readonly UITapGestureRecognizer _replyTap;
        private readonly UITapGestureRecognizer _likersTap;
        private readonly UITapGestureRecognizer _flagersTap;
        private readonly UITapGestureRecognizer _liketap;

        protected CommentTableViewCell(IntPtr handle) : base(handle)
        {
            _avatar = new UIImageView();
            _avatar.ContentMode = UIViewContentMode.ScaleAspectFill;
            _avatar.Layer.CornerRadius = 15;
            _avatar.ClipsToBounds = true;
            ContentView.AddSubview(_avatar);
            _avatar.AutoSetDimensionsToSize(new CGSize(30, 30));
            _avatar.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _avatar.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 10);

            _loginLabel = new UILabel();
            _loginLabel.Font = Constants.Semibold14;
            ContentView.AddSubview(_loginLabel);
            _loginLabel.AutoPinEdge(ALEdge.Left, ALEdge.Right, _avatar, 10);
            _loginLabel.AutoAlignAxis(ALAxis.Horizontal, _avatar);

            _timestamp = new UILabel();
            _timestamp.Font = Constants.Regular12;
            _timestamp.TextColor = Constants.R151G155B158;
            ContentView.AddSubview(_timestamp);
            _timestamp.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 14);
            _timestamp.AutoPinEdge(ALEdge.Left, ALEdge.Right, _loginLabel);
            _timestamp.SetContentHuggingPriority(251, UILayoutConstraintAxis.Horizontal);
            _timestamp.AutoAlignAxis(ALAxis.Horizontal, _avatar);

            _commentText = new UITextView();
            _commentText.Editable = false;
            _commentText.ScrollEnabled = false;
            _commentText.Font = Constants.Regular14;
            _commentText.BackgroundColor = UIColor.Clear;
            ContentView.AddSubview(_commentText);
            _commentText.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _avatar);
            _commentText.AutoPinEdge(ALEdge.Left, ALEdge.Left, _avatar, -4);

            _like = new UIImageView();
            _like.ContentMode = UIViewContentMode.Center;
            _like.UserInteractionEnabled = true;
            ContentView.AddSubview(_like);
            _like.AutoSetDimensionsToSize(new CGSize(40, 50));
            _like.AutoAlignAxis(ALAxis.Horizontal, _commentText);
            _like.AutoPinEdge(ALEdge.Left, ALEdge.Right, _commentText);
            _like.AutoPinEdge(ALEdge.Right, ALEdge.Right, _timestamp, 11);

            _profileTapView = new UIView();
            _profileTapView.UserInteractionEnabled = true;
            ContentView.AddSubview(_profileTapView);
            _profileTapView.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _profileTapView.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _profileTapView.AutoPinEdge(ALEdge.Right, ALEdge.Right, _loginLabel);
            _profileTapView.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, _commentText);

            _bottomView = new UIStackView();
            _bottomView.Axis = UILayoutConstraintAxis.Horizontal;
            _bottomView.Distribution = UIStackViewDistribution.Fill;
            _bottomView.Alignment = UIStackViewAlignment.Leading;
            _bottomView.Spacing = 19;
            ContentView.AddSubview(_bottomView);
            _bottomView.AutoSetDimension(ALDimension.Height, 50);
            _bottomView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _bottomView.AutoPinEdge(ALEdge.Left, ALEdge.Left, _avatar);
            _bottomView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _commentText);
            _bottomView.AutoPinEdge(ALEdge.Right, ALEdge.Right, _timestamp);

            _replyLabel = new UILabel();
            _replyLabel.Text = "Reply";
            _replyLabel.Font = Constants.Regular12;
            _replyLabel.TextColor = Constants.R151G155B158;
            _replyLabel.UserInteractionEnabled = true;
            _bottomView.AddArrangedSubview(_replyLabel);
            _replyLabel.SetContentHuggingPriority(251, UILayoutConstraintAxis.Horizontal);
            _replyLabel.SetContentCompressionResistancePriority(750, UILayoutConstraintAxis.Horizontal);

            _likesLabel = new UILabel();
            _likesLabel.Font = Constants.Regular12;
            _likesLabel.TextColor = Constants.R151G155B158;
            _likesLabel.UserInteractionEnabled = true;
            _bottomView.AddArrangedSubview(_likesLabel);
            _likesLabel.SetContentHuggingPriority(251, UILayoutConstraintAxis.Horizontal);

            _flagsLabel = new UILabel();
            _flagsLabel.Font = Constants.Regular12;
            _flagsLabel.TextColor = Constants.R151G155B158;
            _flagsLabel.UserInteractionEnabled = true;
            _bottomView.AddArrangedSubview(_flagsLabel);
            _flagsLabel.SetContentHuggingPriority(251, UILayoutConstraintAxis.Horizontal);

            _costLabel = new UILabel();
            _costLabel.Font = Constants.Regular12;
            _costLabel.Hidden = true;
#if DEBUG
            _costLabel.Hidden = false;
#endif
            _costLabel.TextColor = Constants.R151G155B158;
            _bottomView.AddArrangedSubview(_costLabel);
            _costLabel.SetContentHuggingPriority(251, UILayoutConstraintAxis.Horizontal);

            var hugView = new UIView();
            _bottomView.AddArrangedSubview(hugView);
            hugView.SetContentHuggingPriority(250, UILayoutConstraintAxis.Horizontal);

            _tap = new UITapGestureRecognizer(() =>
            {
                if (SwipeState == MGSwipeState.None)
                    CellAction?.Invoke(ActionType.Profile, _currentPost);
            });
            _costTap = new UITapGestureRecognizer(() =>
            {
                if (SwipeState == MGSwipeState.None)
                    CellAction?.Invoke(ActionType.Profile, _currentPost);
            });
            _replyTap = new UITapGestureRecognizer(() =>
            {
                if (SwipeState == MGSwipeState.None)
                    CellAction?.Invoke(ActionType.Reply, _currentPost);
            });
            _likersTap = new UITapGestureRecognizer(() =>
            {
                if (SwipeState == MGSwipeState.None)
                    CellAction?.Invoke(ActionType.Voters, _currentPost);
            });
            _flagersTap = new UITapGestureRecognizer(() =>
            {
                if (SwipeState == MGSwipeState.None)
                    CellAction?.Invoke(ActionType.Flagers, _currentPost);
            });
            _liketap = new UITapGestureRecognizer(LikeTap);
            _replyLabel.AddGestureRecognizer(_replyTap);
            _profileTapView.AddGestureRecognizer(_tap);
            _costLabel.AddGestureRecognizer(_costTap);
            _likesLabel.AddGestureRecognizer(_likersTap);
            _flagsLabel.AddGestureRecognizer(_flagersTap);
            _like.AddGestureRecognizer(_liketap);

            _sliderView = new SliderView(UIScreen.MainScreen.Bounds.Width);
            _sliderView.LikeTap += LikeTap;

            BaseViewController.SliderAction += BaseViewController_SliderAction;

            _likelongtap = new UILongPressGestureRecognizer((UILongPressGestureRecognizer obj) =>
            {
                if (AppDelegate.User.HasPostingPermission && !_currentPost.Vote)
                {
                    if (obj.State == UIGestureRecognizerState.Began)
                    {
                        if (!BasePostPresenter.IsEnableVote || BaseViewController.IsSliderOpen)
                            return;
                        rigthButtons = RightButtons;
                        RightButtons = new UIView[0];

                        _sliderView.Frame = new CGRect(4, ContentView.Frame.Height / 2 - 35, UIScreen.MainScreen.Bounds.Width - 8, 70);

                        BaseViewController.IsSliderOpen = true;
                        _sliderView.Show(this);
                    }
                }
            });
            _like.AddGestureRecognizer(_likelongtap);

            RightSwipeSettings.Transition = MGSwipeTransition.Border;

            deleteButton = MGSwipeButton.ButtonWithTitle("", UIImage.FromBundle("ic_delete"), UIColor.FromRGB(250, 250, 250), 26, (tableCell) =>
            {
                CellAction?.Invoke(ActionType.Delete, _currentPost);
                return true;
            });

            editButton = MGSwipeButton.ButtonWithTitle("", UIImage.FromBundle("ic_edit"), UIColor.FromRGB(250, 250, 250), 26, (arg0) =>
            {
                CellAction?.Invoke(ActionType.Edit, _currentPost);
                _currentPost.Editing = true;
                ContentView.BackgroundColor = UIColor.FromRGB(255, 235, 143).ColorWithAlpha(0.5f);
                return true;
            });

            flagButton = MGSwipeButton.ButtonWithTitle("", UIImage.FromBundle("ic_flag"), UIColor.FromRGB(250, 250, 250), 26, (arg0) =>
            {
                CellAction?.Invoke(ActionType.Flag, _currentPost);
                return true;
            });
        }

        public void UpdateCell(Post post)
        {
            if (_currentPost != null)
                _currentPost.PropertyChanged -= PostOnPropertyChanged;
            _currentPost = post;
            _currentPost.PropertyChanged += PostOnPropertyChanged;

            _scheduledWorkAvatar?.Cancel();
            if (!string.IsNullOrEmpty(_currentPost.Avatar))
                _scheduledWorkAvatar = ImageLoader.Load(_currentPost.Avatar, _avatar, placeHolder: "ic_noavatar.png");
            else
                _avatar.Image = UIImage.FromBundle("ic_noavatar");

            _loginLabel.Text = _currentPost.Author;
            _timestamp.Text = _currentPost.Created.ToPostTime(AppDelegate.Localization);
            _commentText.Text = post.Body;

            _like.Transform = CGAffineTransform.MakeScale(1f, 1f);


            if (_currentPost.Editing)
                ContentView.BackgroundColor = UIColor.FromRGB(255, 235, 143).ColorWithAlpha(0.5f);
            else
                ContentView.BackgroundColor = UIColor.White;

            _replyLabel.Hidden = _currentPost.Author == AppDelegate.User.Login || !AppDelegate.User.HasPostingPermission;

            if (_currentPost.Body != Core.Constants.DeletedPostText)
            {
                var rightButtons = new List<MGSwipeButton>();

                if (_currentPost.Author != AppDelegate.User.Login)
                    rightButtons.Add(flagButton);
                else if (_currentPost.CashoutTime > DateTime.Now)
                {
                    rightButtons.Insert(0, deleteButton);
                    rightButtons.Insert(1, editButton);
                }
                RightButtons = rightButtons.ToArray();
            }
            else
                RightButtons = new UIView[0];

            UpdateLike(_currentPost);
            UpdateLikeCount(_currentPost);
            UpdateFlagCount(_currentPost);
            UpdateTotalPayoutReward(_currentPost);
        }

        private void PostOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var post = sender as Post;
            if (post == null || _currentPost != post)
                return;

            switch (e.PropertyName)
            {
                case nameof(Post.Flag):
                case nameof(Post.Vote):
                case nameof(Post.FlagChanging):
                case nameof(Post.VoteChanging):
                    {
                        UpdateLike(post);
                        break;
                    }
                case nameof(Post.NetLikes):
                    {
                        UpdateLikeCount(post);
                        break;
                    }
                case nameof(Post.NetFlags):
                    {
                        UpdateFlagCount(post);
                        break;
                    }
                case nameof(Post.TotalPayoutReward):
                    {
                        UpdateTotalPayoutReward(post);
                        break;
                    }
            }
        }
        
        private void UpdateTotalPayoutReward(Post post)
        {
            _costLabel.Text = StringHelper.ToFormatedCurrencyString(post.TotalPayoutReward, AppDelegate.MainChain);
        }

        private void UpdateFlagCount(Post post)
        {
            _flagsLabel.Text = AppDelegate.Localization.GetText(post.NetFlags == 1 ? LocalizationKeys.Flag : LocalizationKeys.Flags, post.NetFlags);
            _flagsLabel.Hidden = post.NetFlags == 0;
        }

        private void UpdateLikeCount(Post post)
        {
            _likesLabel.Text = AppDelegate.Localization.GetText(post.NetLikes == 1 ? LocalizationKeys.Like : LocalizationKeys.Likes, post.NetLikes);
            _likesLabel.Hidden = post.NetLikes == 0;
        }

        private void UpdateLike(Post post)
        {
            if (post.VoteChanging)
            {
                _like.Image = UIImage.FromBundle("ic_comment_like_active");
                Animate(0.4, 0, UIViewAnimationOptions.Autoreverse | UIViewAnimationOptions.Repeat | UIViewAnimationOptions.CurveEaseIn, () =>
                {
                    _like.Transform = CGAffineTransform.MakeScale(0.5f, 0.5f);
                }, null);
            }
            else
            {
                _like.Layer.RemoveAllAnimations();
                _like.LayoutIfNeeded();
                if (BasePostPresenter.IsEnableVote)
                    _like.Image = post.Vote ? UIImage.FromBundle("ic_comment_like_active") : UIImage.FromBundle("ic_comment_like_inactive");
                else
                    _like.Image = post.Vote ? UIImage.FromBundle("ic_comment_like_active_disabled") : UIImage.FromBundle("ic_comment_like_inactive_disabled");
                _like.UserInteractionEnabled = true;
            }
        }

        private void BaseViewController_SliderAction(bool isSliderOpening)
        {
            if (_sliderView.Superview != null && !isSliderOpening)
            {
                RightButtons = rigthButtons;
                _sliderView.Close();
            }
        }

        private void LikeTap()
        {
            if (!BasePostPresenter.IsEnableVote)
                return;
            CellAction?.Invoke(ActionType.Like, _currentPost);
        }

        public void ReleaseCell()
        {

            CellAction = null;
            
            _currentPost.PropertyChanged -= PostOnPropertyChanged;
            _replyLabel.RemoveGestureRecognizer(_replyTap);
            _profileTapView.RemoveGestureRecognizer(_tap);
            _costLabel.RemoveGestureRecognizer(_costTap);
            _likesLabel.RemoveGestureRecognizer(_likersTap);
            _flagsLabel.RemoveGestureRecognizer(_flagersTap);
            _like.RemoveGestureRecognizer(_liketap);
            _like.RemoveGestureRecognizer(_likelongtap);
            _sliderView.LikeTap -= LikeTap;
            BaseViewController.SliderAction -= BaseViewController_SliderAction;

            deleteButton.Callback = null;
            flagButton.Callback = null;
            editButton.Callback = null;
        }
    }
}
