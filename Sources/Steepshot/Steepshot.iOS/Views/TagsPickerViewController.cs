using System;
using System.Threading;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Views
{
    public partial class TagsPickerViewController : BaseViewControllerWithPresenter<TagsPresenter>
    {
        private readonly LocalTagsCollectionViewSource _viewSource;
        private readonly LocalTagsCollectionViewFlowDelegate _flowDelegate;
        private readonly Timer _timer;
        private string _previousQuery;
        private SearchTextField _tagField;
        private TagsTableViewSource _tableSource;

        public TagsPickerViewController(LocalTagsCollectionViewSource viewSource, LocalTagsCollectionViewFlowDelegate flowDelegate)
        {
            _viewSource = viewSource;
            _flowDelegate = flowDelegate;
            _timer = new Timer(OnTimer);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _presenter.SourceChanged += SourceChanged;

            NavigationController.NavigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
            NavigationController.NavigationBar.ShadowImage = new UIImage();

            tagsCollectionView.RegisterClassForCell(typeof(LocalTagCollectionViewCell), nameof(LocalTagCollectionViewCell));
            tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(LocalTagCollectionViewCell), NSBundle.MainBundle), nameof(LocalTagCollectionViewCell));
            tagsCollectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                SectionInset = new UIEdgeInsets(0, 15, 0, 15),
            }, false);

            tagsCollectionView.Source = _viewSource;
            tagsCollectionView.Delegate = _flowDelegate;
            tagsCollectionView.BackgroundColor = UIColor.White;

            _tagField = new SearchTextField("Hashtag");
            _tagField.ReturnButtonTapped += () => { AddLocalTag(_tagField.Text); };
            View.AddSubview(_tagField);

            _tagField.ClearButtonTapped += () => { OnTimer(null); };
            _tagField.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 10f);
            _tagField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15f);
            _tagField.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15f);
            tagsCollectionView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _tagField, 20f);
            _tagField.AutoSetDimension(ALDimension.Height, 40f);

            var tap = new UITapGestureRecognizer(() =>
            {
                _tagField.ResignFirstResponder();
            });
            View.AddGestureRecognizer(tap);

            _tableSource = new TagsTableViewSource(_presenter, tagsTableView);
            _tableSource.ScrolledToBottom += async () =>
            {
                _tagField.Loader.StartAnimating();
                var exception = await _presenter.TryLoadNextAsync(_tagField.Text, false);
                _tagField.Loader.StopAnimating();
                ShowAlert(exception);
            };

            tagsTableView.Source = _tableSource;
            tagsTableView.LayoutMargins = UIEdgeInsets.Zero;
            tagsTableView.RegisterClassForCellReuse(typeof(TagTableViewCell), nameof(TagTableViewCell));
            tagsTableView.RegisterNibForCellReuse(UINib.FromName(nameof(TagTableViewCell), NSBundle.MainBundle), nameof(TagTableViewCell));
            tagsTableView.RowHeight = 70f;
            _tagField.EditingChanged += EditingDidChange;

            SetBackButton();
            SetCollectionHeight();
            SearchTextChanged();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _tagField.BecomeFirstResponder();
        }

        public override void ViewWillAppear(bool animated)
        {
            _tableSource.CellAction += TableCellAction;
            _viewSource.CellAction += CollectionCellAction;
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            _tableSource.CellAction -= TableCellAction;
            _viewSource.CellAction -= CollectionCellAction;
        }

        protected override void KeyBoardUpNotification(NSNotification notification)
        {
            var kbSize = UIKeyboard.FrameEndFromNotification(notification);
            var contentInsets = new UIEdgeInsets(0, 0, kbSize.Height, 0);
            tagsTableView.ContentInset = contentInsets;
            tagsTableView.ScrollIndicatorInsets = contentInsets;
        }

        protected override void KeyBoardDownNotification(NSNotification notification)
        {
            var contentInsets = new UIEdgeInsets(0, 0, 0, 0);
            tagsTableView.ContentInset = contentInsets;
            tagsTableView.ScrollIndicatorInsets = contentInsets;
        }

        private void SetCollectionHeight()
        {
            if (_viewSource.LocalTags.Count > 0)
                collectionViewHeight.Constant = 40;
            else
                collectionViewHeight.Constant = 0;
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = "Add hashtags";
            NavigationController.NavigationBar.Translucent = false;
        }

        private void CollectionCellAction(ActionType type, string tag)
        {
            RemoveLocalTag(tag);
        }

        private void TableCellAction(ActionType type, string tag)
        {
            AddLocalTag(tag, false);
        }

        private void SourceChanged(Status obj)
        {
            tagsTableView.ReloadData();
        }

        private void OnTimer(object state)
        {
            InvokeOnMainThread(() =>
            {
                SearchTextChanged();
            });
        }

        private async void SearchTextChanged()
        {
            if (_previousQuery == _tagField.Text || _tagField.Text.Length == 1)
                return;

            _tagField.Loader.StartAnimating();
            _previousQuery = _tagField.Text;

            Exception exception = null;
            if (_tagField.Text.Length == 0)
                exception = await _presenter.TryGetTopTagsAsync();
            else if (_tagField.Text.Length > 1)
                exception = await _presenter.TryLoadNextAsync(_tagField.Text, showUnknownTag : true);

            if(!(exception is OperationCanceledException))
                _tagField.Loader.StopAnimating();
            ShowAlert(exception);
        }

        private void EditingDidChange(object sender, EventArgs e)
        {
            _tagField.ClearButton.Hidden = _tagField.Text.Length == 0;
            _timer.Change(1300, Timeout.Infinite);
        }

        private void AddLocalTag(string txt, bool shouldClear = true)
        {
            if (_viewSource.LocalTags.Contains(txt))
            {
                ShowAlert(Core.Localization.LocalizationKeys.DuplicateTag);
                return;
            }

            if (!string.IsNullOrWhiteSpace(txt) && _viewSource.LocalTags.Count < 20)
            {
                if (shouldClear)
                    _tagField.Clear();
                _viewSource.LocalTags.Add(txt);
                SetCollectionHeight();
                _flowDelegate.GenerateVariables();
                tagsCollectionView.ReloadData();
                tagsCollectionView.ScrollToItem(NSIndexPath.FromItemSection(_viewSource.LocalTags.Count - 1, 0), UICollectionViewScrollPosition.Right, true);
            }
        }

        private void RemoveLocalTag(string tag)
        {
            _viewSource.LocalTags.Remove(tag);
            _flowDelegate.GenerateVariables();
            tagsCollectionView.ReloadData();
            SetCollectionHeight();
        }
    }
}
