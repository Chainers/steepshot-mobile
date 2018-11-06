using System;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.iOS.Models;
using UIKit;
using Xamarin.TTTAttributedLabel;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.Helpers;
using AVFoundation;

namespace Steepshot.iOS.Cells
{
    public class NewFeedCollectionViewCell : UICollectionViewCell
    {
        public FeedCellBuilder Cell;

        protected NewFeedCollectionViewCell(IntPtr handle) : base(handle)
        {
            Cell = new FeedCellBuilder(ContentView);
        }
    }

    public class FeedCellBuilder : UIView
    {
        private Post _currentPost;
        private UIView _contentView;

        private UIImageView _avatarImage;
        private UIButton _volumeButton;
        private UILabel _author;
        private UILabel _timestamp;
        private UIButton _moreButton;
        private UIImageView[] _bodyImage;
        private UIView _profileTapView;
        private UIImageView _firstLikerImage;
        private UIImageView _secondLikerImage;
        private UIImageView _thirdLikerImage;
        private UILabel _likes;
        private UIView _likersTapView;
        private UILabel _flags;
        private UILabel _rewards;
        private UIView _verticalSeparator;
        private UIImageView _like;
        private SliderView _sliderView;

        public UIImage PostImage => _bodyImage[0].Image;

        private UIScrollView _photoScroll;
        private UIPageControl _pageControl;

        private UIView _topSeparator;
        private TTTAttributedLabel _attributedLabel;
        private UILabel _comments;
        private UIView _bottomSeparator;

        private readonly VideoView _videoView;

        private IScheduledWork _scheduledWorkAvatar;
        private IScheduledWork[] _scheduledWorkBody = new IScheduledWork[0];
        private IScheduledWork _scheduledWorkfirst;
        private IScheduledWork _scheduledWorksecond;
        private IScheduledWork _scheduledWorkthird;

        private readonly nfloat likersY;
        private nfloat likesMargin;
        private readonly nfloat leftMargin = 15;
        private readonly nfloat likesMarginConst = 10;
        private readonly nfloat flagsMarginConst = 10;
        private readonly nfloat likeButtonWidthConst = 70;
        private readonly nfloat likersImageSide = 24;
        private readonly nfloat likersMargin = 6;
        private readonly nfloat underPhotoPanelHeight = 60;
        private readonly nfloat verticalSeparatorHeight = 30;
        private readonly nfloat moreButtonWidth = 50;
        private readonly nfloat volumeAreaSide = 70;
        private readonly nfloat likersCornerRadius;

        private readonly UITapGestureRecognizer _liketap;
        private readonly UILongPressGestureRecognizer _likelongtap;
        private readonly UITapGestureRecognizer _tap;
        private readonly UITapGestureRecognizer _profileTap;
        private readonly UITapGestureRecognizer _headerTap;
        private readonly UITapGestureRecognizer _commentTap;
        private readonly UITapGestureRecognizer _netVotesTap;
        private readonly UITapGestureRecognizer _flagersTap;

        public bool IsCellActionSet => CellAction != null;
        public Action<ActionType, Post> CellAction;
        public Action MuteAction;

        public event Action<string> TagAction
        {
            add
            {
                _attributedLabel.Delegate = new TTTAttributedLabelFeedDelegate(value);
            }
            remove
            {
                _attributedLabel.Delegate = null;
            }
        }

        public FeedCellBuilder(UIView contentView)
        {
            _contentView = contentView;

            _moreButton = new UIButton();
            _moreButton.Frame = new CGRect(_contentView.Frame.Width - moreButtonWidth, 0, moreButtonWidth, 60);
            _moreButton.SetImage(UIImage.FromBundle("ic_more"), UIControlState.Normal);
            _contentView.AddSubview(_moreButton);

            _avatarImage = new UIImageView(new CGRect(leftMargin, 15, 30, 30));
            _contentView.AddSubview(_avatarImage);

            var authorX = _avatarImage.Frame.Right + 10;

            _author = new UILabel(new CGRect(authorX, _avatarImage.Frame.Top - 2, _moreButton.Frame.Left - authorX, 18));
            _author.Font = Constants.Semibold14;
            _author.LineBreakMode = UILineBreakMode.TailTruncation;
            _author.TextColor = Constants.R15G24B30;
            _contentView.AddSubview(_author);

            _timestamp = new UILabel(new CGRect(authorX, _author.Frame.Bottom, _moreButton.Frame.Left - authorX, 16));
            _timestamp.Font = Constants.Regular12;
            _timestamp.LineBreakMode = UILineBreakMode.TailTruncation;
            _timestamp.TextColor = Constants.R151G155B158;
            _contentView.AddSubview(_timestamp);

            _photoScroll = new UIScrollView();
            _photoScroll.BackgroundColor = Constants.R244G244B246;
            _photoScroll.ShowsHorizontalScrollIndicator = false;
            _photoScroll.Bounces = false;
            _photoScroll.PagingEnabled = true;
            _photoScroll.Scrolled += PhotoScroll_Scrolled;
            contentView.AddSubview(_photoScroll);

            _pageControl = new UIPageControl();
            _pageControl.Hidden = true;
            _pageControl.UserInteractionEnabled = false;
            contentView.AddSubview(_pageControl);

            _likes = new UILabel();
            _likes.Font = Constants.Semibold14;
            _likes.LineBreakMode = UILineBreakMode.TailTruncation;
            _likes.TextColor = Constants.R15G24B30;
            _likes.UserInteractionEnabled = true;
            _contentView.AddSubview(_likes);

            _flags = new UILabel();
            _flags.Font = Constants.Semibold14;
            _flags.LineBreakMode = UILineBreakMode.TailTruncation;
            _flags.TextColor = Constants.R15G24B30;
            _flags.UserInteractionEnabled = true;
            _contentView.AddSubview(_flags);

            _rewards = new UILabel();
            _rewards.Font = Constants.Semibold14;
            _rewards.LineBreakMode = UILineBreakMode.TailTruncation;
            _rewards.TextColor = Constants.R15G24B30;
            _rewards.UserInteractionEnabled = true;
            _contentView.AddSubview(_rewards);

            _like = new UIImageView();
            _like.ContentMode = UIViewContentMode.Center;
            _contentView.AddSubview(_like);

            _verticalSeparator = new UIView();
            _verticalSeparator.BackgroundColor = Constants.R244G244B246;
            _contentView.AddSubview(_verticalSeparator);

            _topSeparator = new UIView();
            _topSeparator.BackgroundColor = Constants.R244G244B246;
            _contentView.AddSubview(_topSeparator);

            var _noLinkAttribute = new UIStringAttributes
            {
                Font = Constants.Regular14,
                ForegroundColor = Constants.R151G155B158,
            };

            var at = new NSMutableAttributedString();
            at.Append(new NSAttributedString("...", _noLinkAttribute));

            _attributedLabel = new TTTAttributedLabel();
            _attributedLabel.EnabledTextCheckingTypes = NSTextCheckingType.Link;
            var prop = new NSDictionary();
            _attributedLabel.LinkAttributes = prop;
            _attributedLabel.ActiveLinkAttributes = prop;
            _attributedLabel.Font = Constants.Regular14;
            _attributedLabel.Lines = 3;
            _attributedLabel.UserInteractionEnabled = true;
            _attributedLabel.Enabled = true;
            _attributedLabel.AttributedTruncationToken = at;
            _contentView.AddSubview(_attributedLabel);

            _comments = new UILabel();
            _comments.Font = Constants.Regular14;
            _comments.LineBreakMode = UILineBreakMode.TailTruncation;
            _comments.TextColor = Constants.R151G155B158;
            _comments.UserInteractionEnabled = true;
            _comments.TextAlignment = UITextAlignment.Center;
            _contentView.AddSubview(_comments);

            _bottomSeparator = new UIView();
            _bottomSeparator.BackgroundColor = Constants.R244G244B246;
            _contentView.AddSubview(_bottomSeparator);

            _profileTapView = new UIView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width / 2, likeButtonWidthConst));
            _profileTapView.UserInteractionEnabled = true;
            _contentView.AddSubview(_profileTapView);

            _likersTapView = new UIView();
            _likersTapView.UserInteractionEnabled = true;
            _contentView.AddSubview(_likersTapView);

            likersY = underPhotoPanelHeight / 2 - likersImageSide / 2;
            likersCornerRadius = likersImageSide / 2;

            _liketap = new UITapGestureRecognizer(LikeTap);
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
                        if (!_currentPost.IsEnableVote || BaseViewController.IsSliderOpen)
                            return;
                        BaseViewController.IsSliderOpen = true;
                        _sliderView.Show(_contentView);
                    }
                }
            });
            _like.AddGestureRecognizer(_likelongtap);

            _tap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Preview, _currentPost);
            });
            _photoScroll.AddGestureRecognizer(_tap);

            _profileTap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Profile, _currentPost);
            });
            _headerTap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Profile, _currentPost);
            });
            _profileTapView.AddGestureRecognizer(_headerTap);
            _rewards.AddGestureRecognizer(_profileTap);

            _commentTap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Comments, _currentPost);
            });
            _comments.AddGestureRecognizer(_commentTap);

            _netVotesTap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Voters, _currentPost);
            });
            _likersTapView.AddGestureRecognizer(_netVotesTap);

            _flagersTap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Flagers, _currentPost);
            });
            _flags.AddGestureRecognizer(_flagersTap);

            _volumeButton = new UIButton();
            _volumeButton.Hidden = true;
            _volumeButton.TouchDown += SwitchVolume;

            _moreButton.TouchDown += FlagButton_TouchDown;

            _videoView = new VideoView(true, true);
        }

        public nfloat UpdateCell(Post post, CellSizeHelper variables)
        {
            if (_currentPost != null)
                _currentPost.PropertyChanged -= PostOnPropertyChanged;
            _currentPost = post;
            _currentPost.PropertyChanged += PostOnPropertyChanged;

            likesMargin = leftMargin;

            _avatarImage?.RemoveFromSuperview();
            _avatarImage = new UIImageView(new CGRect(leftMargin, 15, 30, 30));
            _avatarImage.Layer.CornerRadius = _avatarImage.Frame.Size.Width / 2;
            _avatarImage.ClipsToBounds = true;
            _avatarImage.ContentMode = UIViewContentMode.ScaleAspectFill;
            _contentView.AddSubview(_avatarImage);
            _scheduledWorkAvatar?.Cancel();
            if (!string.IsNullOrEmpty(_currentPost.Avatar))
                _scheduledWorkAvatar = ImageLoader.Load(_currentPost.Avatar,
                                                        _avatarImage,
                                                        placeHolder: "ic_noavatar.png");
            else
                _avatarImage.Image = UIImage.FromBundle("ic_noavatar");

            _author.Text = _currentPost.Author;
            _timestamp.Text = _currentPost.Created.ToPostTime(AppDelegate.Localization);

            _photoScroll.Frame = new CGRect(0, _avatarImage.Frame.Bottom + 15, UIScreen.MainScreen.Bounds.Width, variables.PhotoHeight);

            if (_currentPost.Media.Length > 1)
            {
                _pageControl.Hidden = false;
                _pageControl.Pages = _currentPost.Media.Length;
                _pageControl.SizeToFit();
                _pageControl.Frame = new CGRect(new CGPoint(0, _photoScroll.Frame.Bottom - 30), _pageControl.Frame.Size);
            }
            else
                _pageControl.Hidden = true;

            for (int i = 0; i < _scheduledWorkBody.Length; i++)
            {
                _scheduledWorkBody[i]?.Cancel();
            }

            foreach (var subview in _photoScroll.Subviews)
                subview.RemoveFromSuperview();

            if (MimeTypeHelper.IsVideo(_currentPost.Media[0].ContentType))
            {
                _photoScroll.ContentSize = new CGSize(UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Width);
                _photoScroll.AddSubview(_videoView);
                _videoView.PlayerLayer.Frame = new CGRect(new CGPoint(0, 0), _photoScroll.Frame.Size);
                _videoView.Frame = new CGRect(new CGPoint(0, 0), _photoScroll.Frame.Size);
                _videoView.ChangeItem(_currentPost.Media[0].Url);
                _videoView.Player.Muted = !AppDelegate.VolumeEnabled;

                _photoScroll.AddSubview(_volumeButton);
                _volumeButton.Frame = new CGRect(new CGPoint(_videoView.Frame.Width - volumeAreaSide, _videoView.Frame.Height - volumeAreaSide), new CGSize(volumeAreaSide, volumeAreaSide));
                _volumeButton.ContentEdgeInsets = new UIEdgeInsets(23, 23, 18, 18);
                _volumeButton.SetImage(UIImage.FromBundle(AppDelegate.VolumeEnabled ? "ic_volume" : "ic_mute"), UIControlState.Normal);
                _volumeButton.Hidden = false;
                _volumeButton.UserInteractionEnabled = true;
            }
            else
            {
                _photoScroll.ContentSize = new CGSize(UIScreen.MainScreen.Bounds.Width * _currentPost.Media.Length, variables.PhotoHeight);
                _videoView.ChangeItem(string.Empty);

                _scheduledWorkBody = new IScheduledWork[_currentPost.Media.Length];

                _bodyImage = new UIImageView[_currentPost.Media.Length];
                for (int i = 0; i < _currentPost.Media.Length; i++)
                {
                    _bodyImage[i] = new UIImageView();
                    _bodyImage[i].ClipsToBounds = true;
                    _bodyImage[i].UserInteractionEnabled = true;
                    _bodyImage[i].ContentMode = UIViewContentMode.ScaleAspectFill;
                    _bodyImage[i].Frame = new CGRect(UIScreen.MainScreen.Bounds.Width * i, 0, UIScreen.MainScreen.Bounds.Width, variables.PhotoHeight);
                    _photoScroll.AddSubview(_bodyImage[i]);

                    _scheduledWorkBody[i] = ImageLoader.Load(_currentPost.Media[i].Url,
                                                             _bodyImage[i],
                                                             2, LoadingPriority.Highest);
                }
            }

            UpdateTopLikersAvatars(_currentPost);
            UpdateLikeCount(_currentPost);
            UpdateFlagCount(_currentPost);

            _sliderView.Frame = new CGRect(2, _photoScroll.Frame.Bottom - 5, UIScreen.MainScreen.Bounds.Width - 4, 70);

            UpdateLike(_currentPost);

            _verticalSeparator.Frame = new CGRect(_contentView.Frame.Width - likeButtonWidthConst - 1, _photoScroll.Frame.Bottom + underPhotoPanelHeight / 2 - verticalSeparatorHeight / 2, 1, verticalSeparatorHeight);
            UpdateTotalPayoutReward(_currentPost);
            _topSeparator.Frame = new CGRect(0, _photoScroll.Frame.Bottom + underPhotoPanelHeight, UIScreen.MainScreen.Bounds.Width, 1);

            _attributedLabel.SetText(variables.Text);
            _attributedLabel.Frame = new CGRect(new CGPoint(leftMargin, _topSeparator.Frame.Bottom + 15),
                                                new CGSize(UIScreen.MainScreen.Bounds.Width - leftMargin * 2, variables.TextHeight));

            UpdateChildren(_currentPost);

            _comments.Frame = new CGRect(leftMargin - 5, _attributedLabel.Frame.Bottom + 5, _comments.SizeThatFits(new CGSize(10, 20)).Width + 10, 20 + 10);

            _bottomSeparator.Frame = new CGRect(0, _comments.Frame.Bottom + 10, UIScreen.MainScreen.Bounds.Width, 1);
            return _bottomSeparator.Frame.Bottom;
            //for constant size checking
            //var constantsSize = _bottomSeparator.Frame.Bottom - _attributedLabel.Frame.Height - _bodyImage.Frame.Height;
        }


        private void PostOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var post = (Post)sender;
            if (_currentPost != post)
                return;

            switch (e.PropertyName)
            {
                case nameof(Post.IsEnableVote) when !post.FlagChanging && !post.VoteChanging:
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
                case nameof(Post.TopLikersAvatars):
                    {
                        UpdateTopLikersAvatars(post);
                        break;
                    }
                case nameof(Post.Children):
                    {
                        UpdateChildren(post);
                        break;
                    }
            }
        }

        private void UpdateChildren(Post post)
        {
            switch (post.Children)
            {
                case 0:
                    _comments.Text = AppDelegate.Localization.GetText(LocalizationKeys.PostFirstComment);
                    break;
                case 1:
                    _comments.Text = AppDelegate.Localization.GetText(LocalizationKeys.SeeComment);
                    break;
                default:
                    _comments.Text = AppDelegate.Localization.GetText(LocalizationKeys.ViewComments, post.Children);
                    break;

            }
        }

        private void UpdateTotalPayoutReward(Post post)
        {
#if DEBUG
            _rewards.Text = StringHelper.ToFormatedCurrencyString(post.TotalPayoutReward, AppDelegate.MainChain);
            var rewardWidth = _rewards.SizeThatFits(new CGSize(0, underPhotoPanelHeight));
            _rewards.Frame = new CGRect(_verticalSeparator.Frame.Left - rewardWidth.Width, _photoScroll.Frame.Bottom, rewardWidth.Width, underPhotoPanelHeight);
#endif
        }

        private void UpdateFlagCount(Post post)
        {
            nfloat flagMargin = 0;
            if (post.NetLikes != 0)
                flagMargin = flagsMarginConst;

            if (_currentPost.NetFlags != 0)
            {
                _flags.Text = AppDelegate.Localization.GetText(LocalizationKeys.Flags, _currentPost.NetFlags);
                var flagsWidth = _flags.SizeThatFits(new CGSize(0, underPhotoPanelHeight));
                _flags.Frame = new CGRect(likesMargin + _likes.Frame.Width + flagMargin, _photoScroll.Frame.Bottom, flagsWidth.Width, underPhotoPanelHeight);
            }
            else
                _flags.Frame = new CGRect(likesMargin, _photoScroll.Frame.Bottom, 0, 0);

            _like.Frame = new CGRect(_contentView.Frame.Width - likeButtonWidthConst, _photoScroll.Frame.Bottom, likeButtonWidthConst, underPhotoPanelHeight);
        }

        private void UpdateLikeCount(Post post)
        {
            if (post.NetLikes != 0)
            {
                _likes.Text = AppDelegate.Localization.GetText(LocalizationKeys.Likes, post.NetLikes);
                var likesWidth = _likes.SizeThatFits(new CGSize(0, underPhotoPanelHeight));
                _likes.Frame = new CGRect(likesMargin, _photoScroll.Frame.Bottom, likesWidth.Width, underPhotoPanelHeight);
            }
            else
                _likes.Frame = new CGRect(likesMargin, _photoScroll.Frame.Bottom, 0, 0);

            _likersTapView.Frame = new CGRect(leftMargin, _photoScroll.Frame.Bottom, _likes.Frame.Right - leftMargin, _likes.Frame.Height);
        }

        private void UpdateLike(Post post)
        {
            _like.Transform = CGAffineTransform.MakeScale(1f, 1f);
            if (post.VoteChanging)
            {
                _like.Image = UIImage.FromBundle("ic_like_active");
                Animate(0.4, 0, UIViewAnimationOptions.Autoreverse | UIViewAnimationOptions.Repeat | UIViewAnimationOptions.CurveEaseIn, () =>
                {
                    _like.Transform = CGAffineTransform.MakeScale(0.5f, 0.5f);
                }, null);
            }
            else
            {
                _like.Layer.RemoveAllAnimations();
                _like.LayoutIfNeeded();
                if (post.IsEnableVote)
                    _like.Image = post.Vote ? UIImage.FromBundle("ic_like_active") : UIImage.FromBundle("ic_like");
                else
                    _like.Image = post.Vote ? UIImage.FromBundle("ic_like_active_disabled") : UIImage.FromBundle("ic_like_disabled");
                _like.UserInteractionEnabled = true;
            }
        }

        public void OnVolumeChanged()
        {
            _videoView.Player.Muted = !AppDelegate.VolumeEnabled;
            _volumeButton.SetImage(UIImage.FromBundle(AppDelegate.VolumeEnabled ? "ic_volume" : "ic_mute"), UIControlState.Normal);
        }

        private void SwitchVolume(object sender, EventArgs e)
        {
            AppDelegate.VolumeEnabled = _videoView.Player.Muted;
            MuteAction?.Invoke();
        }

        private void UpdateTopLikersAvatars(Post post)
        {
            if (post.TopLikersAvatars.Any() && !string.IsNullOrEmpty(post.TopLikersAvatars[0]))
            {
                _firstLikerImage?.RemoveFromSuperview();
                _firstLikerImage = new UIImageView();
                _contentView.AddSubview(_firstLikerImage);
                _firstLikerImage.Layer.CornerRadius = likersCornerRadius;
                _firstLikerImage.ClipsToBounds = true;
                _firstLikerImage.ContentMode = UIViewContentMode.ScaleAspectFill;
                _firstLikerImage.Frame = new CGRect(leftMargin, _photoScroll.Frame.Bottom + likersY, likersImageSide, likersImageSide);
                _scheduledWorkfirst?.Cancel();

                _scheduledWorkfirst = ImageLoader.Load(post.TopLikersAvatars[0],
                    _firstLikerImage,
                    placeHolder: "ic_noavatar.png",
                    priority: LoadingPriority.Low);
                likesMargin = _firstLikerImage.Frame.Right + likesMarginConst;
            }
            else if (_firstLikerImage != null)
                _firstLikerImage.Hidden = true;

            if (post.TopLikersAvatars.Count() >= 2 && !string.IsNullOrEmpty(post.TopLikersAvatars[1]))
            {
                _secondLikerImage?.RemoveFromSuperview();
                _secondLikerImage = new UIImageView();
                _contentView.AddSubview(_secondLikerImage);
                _secondLikerImage.Layer.CornerRadius = likersCornerRadius;
                _secondLikerImage.ClipsToBounds = true;
                _secondLikerImage.ContentMode = UIViewContentMode.ScaleAspectFill;
                _secondLikerImage.Frame = new CGRect(_firstLikerImage.Frame.Right - likersMargin, _photoScroll.Frame.Bottom + likersY, likersImageSide, likersImageSide);
                _scheduledWorksecond?.Cancel();

                _scheduledWorksecond = ImageLoader.Load(post.TopLikersAvatars[1],
                    _secondLikerImage,
                    placeHolder: "ic_noavatar.png",
                    priority: LoadingPriority.Low);

                likesMargin = _secondLikerImage.Frame.Right + likesMarginConst;
            }
            else if (_secondLikerImage != null)
                _secondLikerImage.Hidden = true;


            if (post.TopLikersAvatars.Count() >= 3 && !string.IsNullOrEmpty(post.TopLikersAvatars[2]))
            {
                _thirdLikerImage?.RemoveFromSuperview();
                _thirdLikerImage = new UIImageView();
                _contentView.AddSubview(_thirdLikerImage);
                _thirdLikerImage.Layer.CornerRadius = likersCornerRadius;
                _thirdLikerImage.ClipsToBounds = true;
                _thirdLikerImage.ContentMode = UIViewContentMode.ScaleAspectFill;
                _thirdLikerImage.Frame = new CGRect(_secondLikerImage.Frame.Right - likersMargin, _photoScroll.Frame.Bottom + likersY, likersImageSide, likersImageSide);
                _scheduledWorkthird?.Cancel();

                _scheduledWorkthird = ImageLoader.Load(post.TopLikersAvatars[2],
                    _thirdLikerImage,
                    placeHolder: "ic_noavatar.png",
                    priority: LoadingPriority.Low);
                likesMargin = _thirdLikerImage.Frame.Right + likesMarginConst;
            }
            else if (_thirdLikerImage != null)
                _thirdLikerImage.Hidden = true;
        }

        private void BaseViewController_SliderAction(bool isSliderOpening)
        {
            if (_sliderView.Superview != null && !isSliderOpening)
                _sliderView.Close();
        }

        private void PhotoScroll_Scrolled(object sender, EventArgs e)
        {
            var pageWidth = _photoScroll.Frame.Size.Width;
            _pageControl.CurrentPage = (int)Math.Floor((_photoScroll.ContentOffset.X - pageWidth / 2) / pageWidth) + 1;
        }

        public void Playback(bool shouldPlay)
        {
            if (_videoView.Player.Status == AVPlayerStatus.ReadyToPlay)
            {
                if (shouldPlay)
                    _videoView.Play();
                else
                    _videoView.Stop();
            }
        }

        private void LikeTap()
        {
            if (!_currentPost.IsEnableVote)
                return;

            CellAction?.Invoke(ActionType.Like, _currentPost);
        }

        private void FlagButton_TouchDown(object sender, EventArgs e)
        {
            CellAction?.Invoke(ActionType.More, _currentPost);
        }

        public void ReleaseCell()
        {
            CellAction = null;

            _currentPost.PropertyChanged -= PostOnPropertyChanged;
            _photoScroll.Scrolled -= PhotoScroll_Scrolled;
            _like.RemoveGestureRecognizer(_liketap);
            _sliderView.LikeTap -= LikeTap;
            _like.RemoveGestureRecognizer(_likelongtap);
            _photoScroll.RemoveGestureRecognizer(_tap);
            _profileTapView.RemoveGestureRecognizer(_headerTap);
            _rewards.RemoveGestureRecognizer(_profileTap);
            _moreButton.TouchDown -= FlagButton_TouchDown;
            _comments.RemoveGestureRecognizer(_commentTap);
            _likersTapView.RemoveGestureRecognizer(_netVotesTap);
            _flags.RemoveGestureRecognizer(_flagersTap);
            BaseViewController.SliderAction -= BaseViewController_SliderAction;
        }
    }
}
