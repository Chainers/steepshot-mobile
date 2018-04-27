using System;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Models;
using UIKit;
using Xamarin.TTTAttributedLabel;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Extensions;
using System.Linq;
using Steepshot.Core.Utils;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using System.Text.RegularExpressions;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.ViewControllers;

namespace Steepshot.iOS.Cells
{
    public class SliderFeedCollectionViewCell : UICollectionViewCell
    {
        private Post _currentPost;
        private UIView _contentView;

        private UIImageView _avatarImage;
        private UILabel _author;
        private UILabel _timestamp;
        private UIButton _moreButton;
        private UIButton _closeButton;
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
        private UIScrollView _contentScroll;
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
        private nfloat leftMargin = 0;
        private readonly nfloat likesMarginConst = 10;
        private readonly nfloat flagsMarginConst = 10;
        private readonly nfloat likeButtonWidthConst = 70;
        private readonly nfloat likersImageSide = 24;
        private readonly nfloat likersMargin = 6;
        private readonly nfloat underPhotoPanelHeight = 60;
        private readonly nfloat verticalSeparatorHeight = 30;
        private readonly nfloat moreButtonWidth = 50;
        private readonly nfloat likersCornerRadius;
        private readonly nfloat distinction = 5f / (UIScreen.MainScreen.Bounds.Width - 10f);
        private nfloat authorX;

        public bool IsCellActionSet => CellAction != null;
        public event Action<ActionType, Post> CellAction;
        private Action<string> _tagAction;

        public event Action<string> TagAction
        {
            add
            {
                _attributedLabel.Delegate = new TTTAttributedLabelFeedDelegate(value);
                _tagAction = value;
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        //need to rebuild label
        private readonly Regex _tagRegex = new Regex(@"^[a-zA-Z0-9_#]+$");

        public void MoveData(nfloat step)
        {
            leftMargin += step * distinction;
       
            _avatarImage.Frame = new CGRect(leftMargin, 20, 30, 30);
            _attributedLabel.Frame = new CGRect(new CGPoint(leftMargin, _topSeparator.Frame.Bottom + 15),
                                                _attributedLabel.Frame.Size);
            _comments.Frame = new CGRect(new CGPoint(leftMargin - 5, _attributedLabel.Frame.Bottom + 5), _comments.Frame.Size);
            if (_firstLikerImage != null && !_firstLikerImage.Hidden)
            {
                _firstLikerImage.Frame = new CGRect(leftMargin, _photoScroll.Frame.Bottom + likersY, likersImageSide, likersImageSide);
                likesMargin = _firstLikerImage.Frame.Right + likesMarginConst;
            }
            if (_secondLikerImage != null && !_secondLikerImage.Hidden)
            {
                _secondLikerImage.Frame = new CGRect(_firstLikerImage.Frame.Right - likersMargin, _photoScroll.Frame.Bottom + likersY, likersImageSide, likersImageSide);
                likesMargin = _secondLikerImage.Frame.Right + likesMarginConst;
            }
            if (_thirdLikerImage != null && !_thirdLikerImage.Hidden)
            {
                _thirdLikerImage.Frame = new CGRect(_secondLikerImage.Frame.Right - likersMargin, _photoScroll.Frame.Bottom + likersY, likersImageSide, likersImageSide);
                likesMargin = _thirdLikerImage.Frame.Right + likesMarginConst;
            }
            SetLikesFrame();
            _topSeparator.Frame = new CGRect(leftMargin, _photoScroll.Frame.Bottom + underPhotoPanelHeight, _contentScroll.Frame.Width, 1);
        }

        protected SliderFeedCollectionViewCell(IntPtr handle) : base(handle)
        {
            _contentView = ContentView;

            _closeButton = new UIButton();
            _closeButton.Frame = new CGRect(_contentView.Frame.Width - moreButtonWidth, 0, moreButtonWidth, likeButtonWidthConst);
            _closeButton.SetImage(UIImage.FromBundle("ic_close_black"), UIControlState.Normal);
            //_closeButton.BackgroundColor = UIColor.Yellow;
            _contentView.AddSubview(_closeButton);

            _moreButton = new UIButton();
            _moreButton.Frame = new CGRect(_closeButton.Frame.Left - moreButtonWidth, 0, moreButtonWidth, likeButtonWidthConst);
            _moreButton.SetImage(UIImage.FromBundle("ic_more"), UIControlState.Normal);
            //_moreButton.BackgroundColor = UIColor.Black;
            _contentView.AddSubview(_moreButton);

            _avatarImage = new UIImageView();
            _avatarImage.Frame = new CGRect(leftMargin, 20, 30, 30);
            _contentView.AddSubview(_avatarImage);

            authorX = _avatarImage.Frame.Right + 10;

            _author = new UILabel(new CGRect(authorX, _avatarImage.Frame.Top - 2, _closeButton.Frame.Left - authorX, 18));
            _author.Font = Constants.Semibold14;
            //_author.BackgroundColor = UIColor.Yellow;
            _author.LineBreakMode = UILineBreakMode.TailTruncation;
            _author.TextColor = Constants.R15G24B30;
            _contentView.AddSubview(_author);

            _timestamp = new UILabel(new CGRect(authorX, _author.Frame.Bottom, _closeButton.Frame.Left - authorX, 16));
            _timestamp.Font = Constants.Regular12;
            //_timestamp.BackgroundColor = UIColor.Green;
            _timestamp.LineBreakMode = UILineBreakMode.TailTruncation;
            _timestamp.TextColor = Constants.R151G155B158;
            _contentView.AddSubview(_timestamp);

            _contentScroll = new UIScrollView();
            _contentScroll.Frame = new CGRect(0, _avatarImage.Frame.Bottom + 20, _contentView.Frame.Width,
                                              _contentView.Frame.Height - (_avatarImage.Frame.Bottom + 20));
            _contentScroll.ShowsVerticalScrollIndicator = false;
            _contentScroll.Bounces = false;
            //_contentScroll.BackgroundColor = UIColor.LightGray;
            _contentView.AddSubview(_contentScroll);

            _photoScroll = new UIScrollView();
            _photoScroll.ShowsHorizontalScrollIndicator = false;
            _photoScroll.Bounces = false;
            _photoScroll.PagingEnabled = true;
            _photoScroll.Scrolled += (sender, e) =>
            {
                var pageWidth = _photoScroll.Frame.Size.Width;
                _pageControl.CurrentPage = (int)Math.Floor((_photoScroll.ContentOffset.X - pageWidth / 2) / pageWidth) + 1;
            };
            _photoScroll.Layer.CornerRadius = 10;
            _contentScroll.AddSubview(_photoScroll);

            _pageControl = new UIPageControl();
            _pageControl.Hidden = true;
            _pageControl.UserInteractionEnabled = false;
            _contentScroll.AddSubview(_pageControl);

            _likes = new UILabel();
            _likes.Font = Constants.Semibold14;
            _likes.LineBreakMode = UILineBreakMode.TailTruncation;
            _likes.TextColor = Constants.R15G24B30;
            _likes.UserInteractionEnabled = true;
            _contentScroll.AddSubview(_likes);
            //_likes.BackgroundColor = UIColor.Purple;

            _flags = new UILabel();
            _flags.Font = Constants.Semibold14;
            //_flags.BackgroundColor = UIColor.Orange;
            _flags.LineBreakMode = UILineBreakMode.TailTruncation;
            _flags.TextColor = Constants.R15G24B30;
            _flags.UserInteractionEnabled = true;
            _contentScroll.AddSubview(_flags);

            _rewards = new UILabel();
            _rewards.Font = Constants.Semibold14;
            //_rewards.BackgroundColor = UIColor.Orange;
            _rewards.LineBreakMode = UILineBreakMode.TailTruncation;
            _rewards.TextColor = Constants.R15G24B30;
            _rewards.UserInteractionEnabled = true;
            _contentScroll.AddSubview(_rewards);

            _like = new UIImageView();
            _like.ContentMode = UIViewContentMode.Center;
            //_like.BackgroundColor = UIColor.Orange;
            _contentScroll.AddSubview(_like);

            _verticalSeparator = new UIView();
            _verticalSeparator.BackgroundColor = Constants.R244G244B246;
            _contentScroll.AddSubview(_verticalSeparator);

            _topSeparator = new UIView();
            _topSeparator.BackgroundColor = Constants.R244G244B246;
            _contentScroll.AddSubview(_topSeparator);

            _comments = new UILabel();
            _comments.Font = Constants.Regular14;
            //_comments.BackgroundColor = UIColor.DarkGray;
            _comments.LineBreakMode = UILineBreakMode.TailTruncation;
            _comments.TextColor = Constants.R151G155B158;
            _comments.UserInteractionEnabled = true;
            _comments.TextAlignment = UITextAlignment.Center;
            _contentScroll.AddSubview(_comments);

            _bottomSeparator = new UIView();
            _bottomSeparator.BackgroundColor = Constants.R244G244B246;
            _contentScroll.AddSubview(_bottomSeparator);

            _profileTapView = new UIView(new CGRect(0, 0, _contentView.Frame.Width / 2, likeButtonWidthConst));
            _profileTapView.UserInteractionEnabled = true;
            _contentView.AddSubview(_profileTapView);

            _likersTapView = new UIView();
            _likersTapView.UserInteractionEnabled = true;
            _contentScroll.AddSubview(_likersTapView);

            likersY = underPhotoPanelHeight / 2 - likersImageSide / 2;
            likersCornerRadius = likersImageSide / 2;

            var liketap = new UITapGestureRecognizer(LikeTap);
            _like.AddGestureRecognizer(liketap);

            _sliderView = new SliderView(_contentScroll.Frame.Width);
            _sliderView.LikeTap += () =>
            {
                LikeTap();
            };
            BaseViewController.SliderAction += (isOpening) =>
            {
                if (_sliderView.Superview != null && !isOpening)
                    _sliderView.Close();
            };

            var likelongtap = new UILongPressGestureRecognizer((UILongPressGestureRecognizer obj) =>
            {
                if (BasePresenter.User.IsAuthenticated && !_currentPost.Vote)
                {
                    if (obj.State == UIGestureRecognizerState.Began)
                    {
                        if (!BasePostPresenter.IsEnableVote)
                            return;
                        BaseViewController.IsSliderOpen = true;
                        _sliderView.Show(_contentScroll);
                    }
                }
            });
            _like.AddGestureRecognizer(likelongtap);

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
            _closeButton.TouchDown += Close_TouchDown;
        }

        public nfloat UpdateCell(Post post, CellSizeHelper variables, nfloat direction)
        {
            _currentPost = post;

            if(direction == 0)
                leftMargin = 0;
            else if(direction > 0)
                leftMargin = 5;
            else
                leftMargin = -5;

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

            _contentScroll.SetContentOffset(new CGPoint(0, 0), false);

            _photoScroll.Frame = new CGRect(0, 0, _contentScroll.Frame.Width, variables.PhotoHeight);
            _photoScroll.ContentSize = new CGSize(_contentScroll.Frame.Width * _currentPost.Media.Length, variables.PhotoHeight);
            _photoScroll.SetContentOffset(new CGPoint(0, 0), false);

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
                _bodyImage[i].Frame = new CGRect(_contentScroll.Frame.Width * i, 0, _contentScroll.Frame.Width, variables.PhotoHeight);
                _photoScroll.AddSubview(_bodyImage[i]);

                _scheduledWorkBody[i] = ImageService.Instance.LoadUrl(_currentPost.Media[i].Url)
                                             .Retry(2)
                                             .FadeAnimation(false)
                                             .WithCache(FFImageLoading.Cache.CacheType.All)
                                             .WithPriority(LoadingPriority.Highest)
                                               //.DownloadProgress((f)=>
                                             //{
                                             //})
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
                _contentScroll.AddSubview(_firstLikerImage);
                _firstLikerImage.BackgroundColor = UIColor.White;
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
            else if (_firstLikerImage != null)
                _firstLikerImage.Hidden = true;

            if (_currentPost.TopLikersAvatars.Count() >= 2 && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[1]))
            {
                _secondLikerImage?.RemoveFromSuperview();
                _secondLikerImage = new UIImageView();
                _contentScroll.AddSubview(_secondLikerImage);
                _secondLikerImage.BackgroundColor = UIColor.White;
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
            else if (_secondLikerImage != null)
                _secondLikerImage.Hidden = true;

            if (_currentPost.TopLikersAvatars.Count() >= 3 && !string.IsNullOrEmpty(_currentPost.TopLikersAvatars[2]))
            {
                _thirdLikerImage?.RemoveFromSuperview();
                _thirdLikerImage = new UIImageView();
                _contentScroll.AddSubview(_thirdLikerImage);
                _thirdLikerImage.BackgroundColor = UIColor.White;
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

            _sliderView.Frame = new CGRect(0, _photoScroll.Frame.Bottom - 5, _photoScroll.Frame.Width, 70);

            _like.Transform = CGAffineTransform.MakeScale(1f, 1f);
            if (_currentPost.VoteChanging)
                Animate();
            else
            {
                _like.Layer.RemoveAllAnimations();
                _like.LayoutIfNeeded();
                if (BasePostPresenter.IsEnableVote)
                    _like.Image = _currentPost.Vote ? UIImage.FromBundle("ic_like_active") : UIImage.FromBundle("ic_like");
                else
                    _like.Image = _currentPost.Vote ? UIImage.FromBundle("ic_like_active_disabled") : UIImage.FromBundle("ic_like_disabled");
                _like.UserInteractionEnabled = true;
            }

            _verticalSeparator.Frame = new CGRect(_contentView.Frame.Width - likeButtonWidthConst - 1, _photoScroll.Frame.Bottom + underPhotoPanelHeight / 2 - verticalSeparatorHeight / 2, 1, verticalSeparatorHeight);

            /*
            _rewards.Text = BaseViewController.ToFormatedCurrencyString(_currentPost.TotalPayoutReward);
            var rewardWidth = _rewards.SizeThatFits(new CGSize(0, underPhotoPanelHeight));
            _rewards.Frame = new CGRect(_verticalSeparator.Frame.Left - rewardWidth.Width, _photoScroll.Frame.Bottom, rewardWidth.Width, underPhotoPanelHeight);
            */

            _topSeparator.Frame = new CGRect(0, _photoScroll.Frame.Bottom + underPhotoPanelHeight, _contentScroll.Frame.Width, 1);

            var at = new NSMutableAttributedString();

            var _noLinkAttribute = new UIStringAttributes
            {
                Font = Constants.Regular14,
                ForegroundColor = Constants.R15G24B30,
            };

            at.Append(new NSAttributedString(_currentPost.Title, _noLinkAttribute));
            if (!string.IsNullOrEmpty(_currentPost.Description))
            {
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(post.Description, _noLinkAttribute));
            }

            foreach (var tag in _currentPost.Tags)
            {
                if (tag == "steepshot")
                    continue;
                NSUrl tagUrlWithoutWhitespaces = null;

                var tagText = tag.Replace(" ", string.Empty);

                if(_tagRegex.IsMatch(tagText))
                    tagUrlWithoutWhitespaces = new NSUrl(tagText);

                var linkAttribute = new UIStringAttributes
                {
                    Link = tagUrlWithoutWhitespaces,
                    Font = Constants.Regular14,
                    ForegroundColor = Constants.R231G72B0,
                };
                at.Append(new NSAttributedString($" ", _noLinkAttribute));
                at.Append(new NSAttributedString($"#{tag}", linkAttribute));
            }

            _attributedLabel?.RemoveFromSuperview();
            _attributedLabel = new TTTAttributedLabel();
            _attributedLabel.EnabledTextCheckingTypes = NSTextCheckingType.Link;
            var prop = new NSDictionary();
            _attributedLabel.LinkAttributes = prop;
            _attributedLabel.ActiveLinkAttributes = prop;
            _attributedLabel.Font = Constants.Regular14;
            _attributedLabel.Lines = 0;
            _attributedLabel.UserInteractionEnabled = true;
            _attributedLabel.Enabled = true;
            //_attributedLabel.BackgroundColor = UIColor.Blue;
            _contentScroll.AddSubview(_attributedLabel);
            _attributedLabel.Delegate = new TTTAttributedLabelFeedDelegate(_tagAction);
            _attributedLabel.SetText(at);

            var textHeight = _attributedLabel.SizeThatFits(new CGSize(_contentScroll.Frame.Width, 0)).Height;

            _attributedLabel.Frame = new CGRect(new CGPoint(leftMargin, _topSeparator.Frame.Bottom + 15),
                                                new CGSize(_contentScroll.Frame.Width, textHeight));

            _comments.Text = _currentPost.Children == 0
                ? AppSettings.LocalizationManager.GetText(LocalizationKeys.PostFirstComment)
                : AppSettings.LocalizationManager.GetText(LocalizationKeys.ViewComments, _currentPost.Children);

            _comments.Frame = new CGRect(leftMargin - 5, _attributedLabel.Frame.Bottom + 5, _comments.SizeThatFits(new CGSize(10, 20)).Width + 10, 20 + 10);

            _bottomSeparator.Frame = new CGRect(0, _comments.Frame.Bottom + 10, _contentScroll.Frame.Width, 1);

            _contentScroll.ContentSize = new CGSize(_contentScroll.Frame.Width, _bottomSeparator.Frame.Bottom);

            return _bottomSeparator.Frame.Bottom;
            //for constant size checking
            //var constantsSize = _bottomSeparator.Frame.Bottom - _attributedLabel.Frame.Height - _bodyImage.Frame.Height;
        }

        private void SetLikesFrame()
        {
            nfloat flagMargin = 0;

            if (_currentPost.NetLikes != 0)
            {
                var likesWidth = _likes.SizeThatFits(new CGSize(0, underPhotoPanelHeight));
                _likes.Frame = new CGRect(likesMargin, _photoScroll.Frame.Bottom, likesWidth.Width, underPhotoPanelHeight);
                flagMargin = flagsMarginConst;
            }
            else
                _likes.Frame = new CGRect(likesMargin, _photoScroll.Frame.Bottom, 0, 0);

            _likersTapView.Frame = new CGRect(leftMargin, _photoScroll.Frame.Bottom, _likes.Frame.Right - leftMargin, _likes.Frame.Height);

            if (_currentPost.NetFlags != 0)
            {
                var flagsWidth = _flags.SizeThatFits(new CGSize(0, underPhotoPanelHeight));
                _flags.Frame = new CGRect(likesMargin + _likes.Frame.Width + flagMargin, _photoScroll.Frame.Bottom, flagsWidth.Width, underPhotoPanelHeight);
            }
            else
                _flags.Frame = new CGRect(likesMargin, _photoScroll.Frame.Bottom, 0, 0);
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

        private void Close_TouchDown(object sender, EventArgs e)
        {
            CellAction?.Invoke(ActionType.Close, _currentPost);
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
