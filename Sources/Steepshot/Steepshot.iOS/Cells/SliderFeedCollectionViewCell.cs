using System;
using System.ComponentModel;
using CoreGraphics;
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
using System.Runtime.Remoting.Contexts;
using Steepshot.Core.Utils;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using System.Text.RegularExpressions;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.Helpers;
using AVFoundation;

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

        private readonly VideoView _videoView;

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
                _attributedLabel.Delegate = null;
                _tagAction = null;
            }
        }

        private readonly UITapGestureRecognizer _liketap;
        private readonly UILongPressGestureRecognizer _likelongtap;
        private readonly UITapGestureRecognizer _tap;
        private readonly UITapGestureRecognizer _profileTap;
        private readonly UITapGestureRecognizer _headerTap;
        private readonly UITapGestureRecognizer _commentTap;
        private readonly UITapGestureRecognizer _netVotesTap;
        private readonly UITapGestureRecognizer _flagersTap;

        //need to rebuild label
        private readonly Regex _tagRegex = new Regex(@"^[a-zA-Z0-9_#]+$");

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
            _photoScroll.Scrolled += PhotoScroll_Scrolled;
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

            _liketap = new UITapGestureRecognizer(LikeTap);
            _like.AddGestureRecognizer(_liketap);

            _sliderView = new SliderView(_contentScroll.Frame.Width);
            _sliderView.LikeTap += LikeTap;

            BaseViewController.SliderAction += BaseViewController_SliderAction;

            _likelongtap = new UILongPressGestureRecognizer((UILongPressGestureRecognizer obj) =>
            {
                if (AppDelegate.User.HasPostingPermission && !_currentPost.Vote)
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
            _like.AddGestureRecognizer(_likelongtap);
            _tap = new UITapGestureRecognizer(() =>
            {
                _currentPost.PageIndex = (int)(_photoScroll.ContentOffset.X / _photoScroll.Frame.Size.Width);
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

            _moreButton.TouchDown += FlagButton_TouchDown;
            _closeButton.TouchDown += Close_TouchDown;

            _videoView = new VideoView(true);
        }

        public nfloat UpdateCell(Post post, CellSizeHelper variables, nfloat direction)
        {
            if (_currentPost != null)
                _currentPost.PropertyChanged -= PostOnPropertyChanged;
            _currentPost = post;
            _currentPost.PropertyChanged += PostOnPropertyChanged;

            if (direction == 0)
                leftMargin = 0;
            else if (direction > 0)
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
                _scheduledWorkAvatar = ImageLoader.Load(_currentPost.Avatar,
                                                        _avatarImage,
                                                        placeHolder: "ic_noavatar.png");
            else
                _avatarImage.Image = UIImage.FromBundle("ic_noavatar");

            _author.Text = _currentPost.Author;
            _timestamp.Text = _currentPost.Created.ToPostTime(AppDelegate.Localization);

            _contentScroll.SetContentOffset(new CGPoint(0, 0), false);

            foreach (var subview in _photoScroll.Subviews)
                subview.RemoveFromSuperview();

            for (int i = 0; i < _scheduledWorkBody.Length; i++)
            {
                _scheduledWorkBody[i]?.Cancel();
            }
            if (MimeTypeHelper.IsVideo(_currentPost.Media[0].ContentType))
            {
                _photoScroll.Frame = new CGRect(0, 0, _contentScroll.Frame.Width, _contentScroll.Frame.Width);
                _photoScroll.ContentSize = new CGSize(_contentScroll.Frame.Width, _contentScroll.Frame.Width);
                _photoScroll.AddSubview(_videoView);
                _videoView.PlayerLayer.Frame = new CGRect(new CGPoint(0, 0), _photoScroll.Frame.Size);
                _videoView.Frame = new CGRect(new CGPoint(0, 0), _photoScroll.Frame.Size);
                _videoView.ChangeItem(_currentPost.Media[0].Url);
            }
            else
            {
                _photoScroll.Frame = new CGRect(0, 0, _contentScroll.Frame.Width, variables.PhotoHeight);
                _videoView.ChangeItem(null);
                _photoScroll.ContentSize = new CGSize(_contentScroll.Frame.Width * _currentPost.Media.Length, variables.PhotoHeight);
                _photoScroll.SetContentOffset(new CGPoint(0, 0), false);

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

                    _scheduledWorkBody[i] = ImageLoader.Load(_currentPost.Media[i].Url,
                                                             _bodyImage[i],
                                                             2, LoadingPriority.Highest,
                                                             size: new CGSize(UIScreen.MainScreen.Bounds.Size));
                }
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

            UpdateTopLikersAvatars(_currentPost);
            UpdateLikeCount(_currentPost);
            UpdateFlagCount(_currentPost);
            
            _sliderView.Frame = new CGRect(0, _photoScroll.Frame.Bottom - 5, _photoScroll.Frame.Width, 70);

            UpdateLike(_currentPost);

            _verticalSeparator.Frame = new CGRect(_contentView.Frame.Width - likeButtonWidthConst - 1, _photoScroll.Frame.Bottom + underPhotoPanelHeight / 2 - verticalSeparatorHeight / 2, 1, verticalSeparatorHeight);

            UpdateTotalPayoutReward(_currentPost);

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

                if (_tagRegex.IsMatch(tagText))
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
                ? AppDelegate.Localization.GetText(LocalizationKeys.PostFirstComment)
                : AppDelegate.Localization.GetText(LocalizationKeys.ViewComments, _currentPost.Children);

            _comments.Frame = new CGRect(leftMargin - 5, _attributedLabel.Frame.Bottom + 5, _comments.SizeThatFits(new CGSize(10, 20)).Width + 10, 20 + 10);

            _bottomSeparator.Frame = new CGRect(0, _comments.Frame.Bottom + 10, _contentScroll.Frame.Width, 1);

            _contentScroll.ContentSize = new CGSize(_contentScroll.Frame.Width, _bottomSeparator.Frame.Bottom);

            return _bottomSeparator.Frame.Bottom;
            //for constant size checking
            //var constantsSize = _bottomSeparator.Frame.Bottom - _attributedLabel.Frame.Height - _bodyImage.Frame.Height;
        }

        private async void PostOnPropertyChanged(object sender, PropertyChangedEventArgs e)
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
                case nameof(Post.TopLikersAvatars):
                    {
                        UpdateTopLikersAvatars(post);
                        break;
                    }
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

            if (post.NetFlags != 0)
            {
                _flags.Text = AppDelegate.Localization.GetText(LocalizationKeys.Flags, post.NetFlags);
                var flagsWidth = _flags.SizeThatFits(new CGSize(0, underPhotoPanelHeight));
                _flags.Frame = new CGRect(likesMargin + _likes.Frame.Width + flagMargin, _photoScroll.Frame.Bottom, flagsWidth.Width, underPhotoPanelHeight);
            }
            else
            {
                _flags.Frame = new CGRect(likesMargin, _photoScroll.Frame.Bottom, 0, 0);
            }

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
                if (BasePostPresenter.IsEnableVote)
                    _like.Image = post.Vote ? UIImage.FromBundle("ic_like_active") : UIImage.FromBundle("ic_like");
                else
                    _like.Image = post.Vote ? UIImage.FromBundle("ic_like_active_disabled") : UIImage.FromBundle("ic_like_disabled");
                _like.UserInteractionEnabled = true;
            }
        }

        private void UpdateTopLikersAvatars(Post post)
        {
            if (post.TopLikersAvatars.Any() && !string.IsNullOrEmpty(post.TopLikersAvatars[0]))
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
                _contentScroll.AddSubview(_secondLikerImage);
                _secondLikerImage.BackgroundColor = UIColor.White;
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
                _contentScroll.AddSubview(_thirdLikerImage);
                _thirdLikerImage.BackgroundColor = UIColor.White;
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

        private void BaseViewController_SliderAction(bool isOpening)
        {
            if (_sliderView.Superview != null && !isOpening)
                _sliderView.Close();
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
            _closeButton.TouchDown -= Close_TouchDown;
            _comments.RemoveGestureRecognizer(_commentTap);
            _likersTapView.RemoveGestureRecognizer(_netVotesTap);
            _flags.RemoveGestureRecognizer(_flagersTap);
            BaseViewController.SliderAction -= BaseViewController_SliderAction;
        }
    }
}
