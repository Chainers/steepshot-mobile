using System;
using System.Linq;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.iOS.Models;
using UIKit;
using Xamarin.TTTAttributedLabel;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

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

        public UIImage PostImage => _bodyImage[0].Image;

        private UIScrollView _photoScroll;
        private UIPageControl _pageControl;

        private UIView _topSeparator;
        private TTTAttributedLabel _attributedLabel;
        private UILabel _comments;
        private UIView _bottomSeparator;

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
        private readonly nfloat likersCornerRadius;

        public bool IsCellActionSet => CellAction != null;
        public event Action<ActionType, Post> CellAction;
        public event Action<string> TagAction
        {
            add
            {
                _attributedLabel.Delegate = new TTTAttributedLabelFeedDelegate(value);
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        public FeedCellBuilder(UIView contentView)
        {
            _contentView = contentView;

            _moreButton = new UIButton();
            _moreButton.Frame = new CGRect(_contentView.Frame.Width - moreButtonWidth, 0, moreButtonWidth, likeButtonWidthConst);
            _moreButton.SetImage(UIImage.FromBundle("ic_more"), UIControlState.Normal);
            //_moreButton.BackgroundColor = UIColor.Black;
            _contentView.AddSubview(_moreButton);

            _avatarImage = new UIImageView(new CGRect(leftMargin, 20, 30, 30));
            _contentView.AddSubview(_avatarImage);

            var authorX = _avatarImage.Frame.Right + 10;

            _author = new UILabel(new CGRect(authorX, _avatarImage.Frame.Top - 2, _moreButton.Frame.Left - authorX, 18));
            _author.Font = Constants.Semibold14;
            //_author.BackgroundColor = UIColor.Yellow;
            _author.LineBreakMode = UILineBreakMode.TailTruncation;
            _author.TextColor = Constants.R15G24B30;
            _contentView.AddSubview(_author);

            _timestamp = new UILabel(new CGRect(authorX, _author.Frame.Bottom, _moreButton.Frame.Left - authorX, 16));
            _timestamp.Font = Constants.Regular12;
            //_timestamp.BackgroundColor = UIColor.Green;
            _timestamp.LineBreakMode = UILineBreakMode.TailTruncation;
            _timestamp.TextColor = Constants.R151G155B158;
            _contentView.AddSubview(_timestamp);

            _photoScroll = new UIScrollView();
            _photoScroll.ShowsHorizontalScrollIndicator = false;
            _photoScroll.Bounces = false;
            _photoScroll.PagingEnabled = true;
            _photoScroll.Scrolled+= (sender, e) => {
                var pageWidth = _photoScroll.Frame.Size.Width;
                _pageControl.CurrentPage = (int)Math.Floor((_photoScroll.ContentOffset.X - pageWidth / 2) / pageWidth) + 1;
            };
            contentView.AddSubview(_photoScroll);

            _pageControl = new UIPageControl();
            _pageControl.Hidden = true;
            contentView.AddSubview(_pageControl);

            _likes = new UILabel();
            _likes.Font = Constants.Semibold14;
            _likes.LineBreakMode = UILineBreakMode.TailTruncation;
            _likes.TextColor = Constants.R15G24B30;
            _likes.UserInteractionEnabled = true;
            _contentView.AddSubview(_likes);

            _flags = new UILabel();
            _flags.Font = Constants.Semibold14;
            //_flags.BackgroundColor = UIColor.Orange;
            _flags.LineBreakMode = UILineBreakMode.TailTruncation;
            _flags.TextColor = Constants.R15G24B30;
            _flags.UserInteractionEnabled = true;
            _contentView.AddSubview(_flags);

            _rewards = new UILabel();
            _rewards.Font = Constants.Semibold14;
            //_rewards.BackgroundColor = UIColor.Orange;
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
            //_attributedLabel.BackgroundColor = UIColor.Blue;
            _contentView.AddSubview(_attributedLabel);

            _comments = new UILabel();
            _comments.Font = Constants.Regular14;
            //_comments.BackgroundColor = UIColor.DarkGray;
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

            var liketap = new UITapGestureRecognizer(LikeTap);
            _like.AddGestureRecognizer(liketap);

            UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Preview, _currentPost);
            });
            _photoScroll.AddGestureRecognizer(tap);

            var profileTap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Profile, _currentPost);
            });
            var headerTap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Profile, _currentPost);
            });
            _profileTapView.AddGestureRecognizer(headerTap);
            _rewards.AddGestureRecognizer(profileTap);

            var commentTap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Comments, _currentPost);
            });
            _comments.AddGestureRecognizer(commentTap);

            var netVotesTap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Voters, _currentPost);
            });
            _likersTapView.AddGestureRecognizer(netVotesTap);

            var flagersTap = new UITapGestureRecognizer(() =>
            {
                CellAction?.Invoke(ActionType.Flagers, _currentPost);
            });
            _flags.AddGestureRecognizer(flagersTap);

            _moreButton.TouchDown += FlagButton_TouchDown;
        }

        public nfloat UpdateCell(Post post, CellSizeHelper variables)
        {
            _currentPost = post;
            likesMargin = leftMargin;

            _avatarImage?.RemoveFromSuperview();
            _avatarImage = new UIImageView(new CGRect(leftMargin, 20, 30, 30));
            _avatarImage.Layer.CornerRadius = _avatarImage.Frame.Size.Width / 2;
            _avatarImage.ClipsToBounds = true;
            _avatarImage.ContentMode = UIViewContentMode.ScaleAspectFill;
            _contentView.AddSubview(_avatarImage);
            _scheduledWorkAvatar?.Cancel();
            if (!string.IsNullOrEmpty(_currentPost.Avatar))
                _scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentPost.Avatar, TimeSpan.FromDays(30))
                                                   .WithCache(FFImageLoading.Cache.CacheType.All)
                                                   .FadeAnimation(false)
                                                   .DownSample(200)
                                                   .LoadingPlaceholder("ic_noavatar.png")
                                                   .ErrorPlaceholder("ic_noavatar.png")
                                                   .WithPriority(LoadingPriority.Normal)
                                                   .Into(_avatarImage);
            else
                _avatarImage.Image = UIImage.FromBundle("ic_noavatar");

            _author.Text = _currentPost.Author;
            _timestamp.Text = _currentPost.Created.ToPostTime();

            _photoScroll.Frame = new CGRect(0, _avatarImage.Frame.Bottom + 20, UIScreen.MainScreen.Bounds.Width, variables.PhotoHeight);
            _photoScroll.ContentSize = new CGSize(UIScreen.MainScreen.Bounds.Width * _currentPost.Media.Length, variables.PhotoHeight);

            foreach (var subview in _photoScroll.Subviews)
                subview.RemoveFromSuperview();

            for (int i = 0; i < _scheduledWorkBody.Length; i++)
            {
                _scheduledWorkBody[i]?.Cancel();
            }
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

                _scheduledWorkBody[i] = ImageService.Instance.LoadUrl(_currentPost.Media[i].Url)
                                             .Retry(2)
                                             .FadeAnimation(false)
                                             .WithCache(FFImageLoading.Cache.CacheType.All)
                                             .WithPriority(LoadingPriority.Highest)
                                             /* .DownloadProgress((f)=>
                                            {
                                            })*/
                                              .Into(_bodyImage[i]);
            }
            if (_currentPost.Media.Length > 1)
            {
                _pageControl.Hidden = false;
                _pageControl.Pages = _currentPost.Media.Length;
                _pageControl.SizeToFit();
                _pageControl.Frame = new CGRect(new CGPoint(0, _photoScroll.Frame.Bottom - 30), _pageControl.Frame.Size);
            }
            else
                _pageControl.Hidden = true;

            if (_currentPost.TopLikersAvatars.Any() && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[0]))
            {
                _firstLikerImage?.RemoveFromSuperview();
                _firstLikerImage = new UIImageView();
                _contentView.AddSubview(_firstLikerImage);
                _firstLikerImage.Layer.CornerRadius = likersCornerRadius;
                _firstLikerImage.ClipsToBounds = true;
                _firstLikerImage.ContentMode = UIViewContentMode.ScaleAspectFill;
                _firstLikerImage.Frame = new CGRect(leftMargin, _photoScroll.Frame.Bottom + likersY, likersImageSide, likersImageSide);
                _scheduledWorkfirst?.Cancel();

                _scheduledWorkfirst = ImageService.Instance.LoadUrl(_currentPost.TopLikersAvatars[0], TimeSpan.FromDays(30))
                                                  .WithCache(FFImageLoading.Cache.CacheType.All)
                                                  .LoadingPlaceholder("ic_noavatar.png")
                                                  .ErrorPlaceholder("ic_noavatar.png")
                                                  .DownSample(width: 100)
                                                  .FadeAnimation(false)
                                                  .WithPriority(LoadingPriority.Lowest)
                                                  .Into(_firstLikerImage);
                likesMargin = _firstLikerImage.Frame.Right + likesMarginConst;
            }
            else if(_firstLikerImage != null)
                _firstLikerImage.Hidden = true;
            
            if (_currentPost.TopLikersAvatars.Count() >= 2 && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[1]))
            {
                _secondLikerImage?.RemoveFromSuperview();
                _secondLikerImage = new UIImageView();
                _contentView.AddSubview(_secondLikerImage);
                _secondLikerImage.Layer.CornerRadius = likersCornerRadius;
                _secondLikerImage.ClipsToBounds = true;
                _secondLikerImage.ContentMode = UIViewContentMode.ScaleAspectFill;
                _secondLikerImage.Frame = new CGRect(_firstLikerImage.Frame.Right - likersMargin, _photoScroll.Frame.Bottom + likersY, likersImageSide, likersImageSide);
                _scheduledWorksecond?.Cancel();

                _scheduledWorksecond = ImageService.Instance.LoadUrl(_currentPost.TopLikersAvatars[1], TimeSpan.FromDays(30))
                                                    .WithCache(FFImageLoading.Cache.CacheType.All)
                                                    .LoadingPlaceholder("ic_noavatar.png")
                                                    .ErrorPlaceholder("ic_noavatar.png")
                                                    .WithPriority(LoadingPriority.Lowest)
                                                    .DownSample(width: 100)
                                                    .FadeAnimation(false)
                                                    .Into(_secondLikerImage);
                likesMargin = _secondLikerImage.Frame.Right + likesMarginConst;
            }
            else if(_secondLikerImage != null)
                _secondLikerImage.Hidden = true;

            if (_currentPost.TopLikersAvatars.Count() >= 3 && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[2]))
            {
                _thirdLikerImage?.RemoveFromSuperview();
                _thirdLikerImage = new UIImageView();
                _contentView.AddSubview(_thirdLikerImage);
                _thirdLikerImage.Layer.CornerRadius = likersCornerRadius;
                _thirdLikerImage.ClipsToBounds = true;
                _thirdLikerImage.ContentMode = UIViewContentMode.ScaleAspectFill;
                _thirdLikerImage.Frame = new CGRect(_secondLikerImage.Frame.Right - likersMargin, _photoScroll.Frame.Bottom + likersY, likersImageSide, likersImageSide);
                _scheduledWorkthird?.Cancel();

                _scheduledWorkthird = ImageService.Instance.LoadUrl(_currentPost.TopLikersAvatars[2], TimeSpan.FromDays(30))
                                                   .WithCache(FFImageLoading.Cache.CacheType.All)
                                                   .LoadingPlaceholder("ic_noavatar.png")
                                                   .ErrorPlaceholder("ic_noavatar.png")
                                                   .WithPriority(LoadingPriority.Lowest)
                                                   .DownSample(width: 100)
                                                   .FadeAnimation(false)
                                                   .Into(_thirdLikerImage);
                likesMargin = _thirdLikerImage.Frame.Right + likesMarginConst;
            }
            else if (_thirdLikerImage != null)
                _thirdLikerImage.Hidden = true;

            nfloat flagMargin = 0;

            if (_currentPost.NetLikes != 0)
            {
                _likes.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Likes, _currentPost.NetLikes);
                var likesWidth = _likes.SizeThatFits(new CGSize(0, underPhotoPanelHeight));
                _likes.Frame = new CGRect(likesMargin, _photoScroll.Frame.Bottom, likesWidth.Width, underPhotoPanelHeight);
                flagMargin = flagsMarginConst;
            }
            else
                _likes.Frame = new CGRect(likesMargin, _photoScroll.Frame.Bottom, 0, 0);

            _likersTapView.Frame = new CGRect(leftMargin, _photoScroll.Frame.Bottom, _likes.Frame.Right - leftMargin, _likes.Frame.Height);

            if (_currentPost.NetFlags != 0)
            {
                _flags.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Flags, _currentPost.NetFlags);
                var flagsWidth = _flags.SizeThatFits(new CGSize(0, underPhotoPanelHeight));
                _flags.Frame = new CGRect(likesMargin + _likes.Frame.Width + flagMargin, _photoScroll.Frame.Bottom, flagsWidth.Width, underPhotoPanelHeight);
            }
            else
                _flags.Frame = new CGRect(likesMargin, _photoScroll.Frame.Bottom, 0, 0);

            _like.Frame = new CGRect(_contentView.Frame.Width - likeButtonWidthConst, _photoScroll.Frame.Bottom, likeButtonWidthConst, underPhotoPanelHeight);

            _like.Transform = CGAffineTransform.MakeScale(1f, 1f);
            if (_currentPost.VoteChanging)
                Animate();
            else
            {
                _like.Layer.RemoveAllAnimations();
                _like.LayoutIfNeeded();
                _like.Image = _currentPost.Vote ? UIImage.FromBundle("ic_like_active") : UIImage.FromBundle("ic_like");
                _like.UserInteractionEnabled = true;
            }

            _verticalSeparator.Frame = new CGRect(_contentView.Frame.Width - likeButtonWidthConst - 1, _photoScroll.Frame.Bottom + underPhotoPanelHeight / 2 - verticalSeparatorHeight / 2, 1, verticalSeparatorHeight);

            /*
            _rewards.Text = BaseViewController.ToFormatedCurrencyString(_currentPost.TotalPayoutReward);
            var rewardWidth = _rewards.SizeThatFits(new CGSize(0, underPhotoPanelHeight));
            _rewards.Frame = new CGRect(_verticalSeparator.Frame.Left - rewardWidth.Width, _photoScroll.Frame.Bottom, rewardWidth.Width, underPhotoPanelHeight);
            */

            _topSeparator.Frame = new CGRect(0, _photoScroll.Frame.Bottom + underPhotoPanelHeight, UIScreen.MainScreen.Bounds.Width, 1);

            _attributedLabel.SetText(variables.Text);
            _attributedLabel.Frame = new CGRect(new CGPoint(leftMargin, _topSeparator.Frame.Bottom + 15),
                                                new CGSize(UIScreen.MainScreen.Bounds.Width - leftMargin * 2, variables.TextHeight));

            _comments.Text = _currentPost.Children == 0
                ? AppSettings.LocalizationManager.GetText(LocalizationKeys.PostFirstComment)
                : AppSettings.LocalizationManager.GetText(LocalizationKeys.ViewComments, _currentPost.Children);

            _comments.Frame = new CGRect(leftMargin - 5, _attributedLabel.Frame.Bottom + 5, _comments.SizeThatFits(new CGSize(10, 20)).Width + 10, 20 + 10);

            _bottomSeparator.Frame = new CGRect(0, _comments.Frame.Bottom + 10, UIScreen.MainScreen.Bounds.Width, 1);

            return _bottomSeparator.Frame.Bottom;
            //for constant size checking
            //var constantsSize = _bottomSeparator.Frame.Bottom - _attributedLabel.Frame.Height - _bodyImage.Frame.Height;
        }

        private void LikeTap()
        {
            if (!BasePostPresenter.IsEnableVote)
                return;
            CellAction?.Invoke(ActionType.Like, _currentPost);
        }

        private void FlagButton_TouchDown(object sender, EventArgs e)
        {
            CellAction?.Invoke(ActionType.More, _currentPost);
        }

        private void Animate()
        {
            _like.Image = UIImage.FromBundle("ic_like_active");
            Animate(0.4, 0, UIViewAnimationOptions.Autoreverse | UIViewAnimationOptions.Repeat | UIViewAnimationOptions.CurveEaseIn, () =>
            {
                _like.Transform = CGAffineTransform.MakeScale(0.5f, 0.5f);
            }, null);
        }
    }
}
