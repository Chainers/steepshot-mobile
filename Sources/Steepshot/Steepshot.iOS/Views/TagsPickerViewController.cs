using System;
using System.Threading;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Errors;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Views
{
    public partial class TagsPickerViewController : BaseViewControllerWithPresenter<PostDescriptionPresenter>
    {
        private LocalTagsCollectionViewSource _viewSource;
        private LocalTagsCollectionViewFlowDelegate _flowDelegate;
        private Timer _timer;
        private string _previousQuery;
        private TextFieldWithInsets tagField;

        public TagsPickerViewController(LocalTagsCollectionViewSource viewSource, LocalTagsCollectionViewFlowDelegate flowDelegate)
        {
            _viewSource = viewSource;
            _flowDelegate = flowDelegate;
            _timer = new Timer(OnTimer);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationController.NavigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
            NavigationController.NavigationBar.ShadowImage = new UIImage();

            tagsCollectionView.RegisterClassForCell(typeof(LocalTagCollectionViewCell), nameof(LocalTagCollectionViewCell));
            tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(LocalTagCollectionViewCell), NSBundle.MainBundle), nameof(LocalTagCollectionViewCell));
            tagsCollectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                SectionInset = new UIEdgeInsets(0, 15, 0, 0),
            }, false);
            _viewSource.CellAction += CollectionCellAction;

            tagsCollectionView.Source = _viewSource;
            tagsCollectionView.Delegate = _flowDelegate;
            tagsCollectionView.BackgroundColor = UIColor.White;

            tagField = new TextFieldWithInsets();
            View.AddSubview(tagField);
            tagField.Font = Constants.Regular14;
            tagField.Placeholder = "Hashtag";
            tagField.AutocorrectionType = UITextAutocorrectionType.No;
            tagField.AutocapitalizationType = UITextAutocapitalizationType.None;
            tagField.TintColor = Constants.R255G71B5;

            tagField.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 10f);
            tagField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15f);
            tagField.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15f);

            tagsCollectionView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, tagField, 20f);
            tagField.AutoSetDimension(ALDimension.Height, 40f);

            tagField.Layer.CornerRadius = 20;
            tagField.Layer.BorderWidth = 1;
            tagField.Layer.BorderColor = Constants.R255G71B5.CGColor;
            tagField.BecomeFirstResponder();

            var clearButton = new UIButton();
            clearButton.SetImage(UIImage.FromBundle("ic_delete_tag"), UIControlState.Normal);
            clearButton.Frame = new CGRect(0, 0, 16, 16);
            clearButton.TouchDown += (sender, e) =>
            {
                tagField.Text = string.Empty;
            };
            tagField.RightView = clearButton;
            tagField.RightViewMode = UITextFieldViewMode.WhileEditing;

            var tap = new UITapGestureRecognizer(() =>
            {
                tagField.ResignFirstResponder();
            });
            View.AddGestureRecognizer(tap);

            var _tableSource = new TagsTableViewSource(_presenter);
            _tableSource.CellAction += TableCellAction;
            tagsTableView.Source = _tableSource;
            tagsTableView.LayoutMargins = UIEdgeInsets.Zero;
            tagsTableView.RegisterClassForCellReuse(typeof(TagTableViewCell), nameof(TagTableViewCell));
            tagsTableView.RegisterNibForCellReuse(UINib.FromName(nameof(TagTableViewCell), NSBundle.MainBundle), nameof(TagTableViewCell));
            tagsTableView.RowHeight = 70f;

            tagField.EditingChanged += EditingDidChange;
            tagField.Delegate = new TagFieldDelegate(() => 
            {
                AddTag(tagField.Text);
            });
            SetBackButton();
            SetCollectionHeight();
            SearchTextChanged();
        }

        protected override void KeyBoardUpNotification(NSNotification notification)
        {
            var kbSize = UIKeyboard.FrameEndFromNotification(notification);
            var contentInsets = new UIEdgeInsets(0, 0, kbSize.Height, 0);
            tagsTableView.ContentInset = contentInsets;
            tagsTableView.ScrollIndicatorInsets = contentInsets;
            //TODO:scroll view to the current position when keyboard is open
            //tagsTableView.ScrollRectToVisible(new CGRect(tagsTableView.ContentOffset, new CGSize(tagsTableView.Frame.Width, tagsTableView.Frame.Height - kbSize.Height)), true);
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

        protected override void CreatePresenter()
        {
            _presenter = new PostDescriptionPresenter();
            _presenter.SourceChanged += SourceChanged;
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = "Add hashtags";
            NavigationController.NavigationBar.Translucent = false;
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        private void CollectionCellAction(ActionType type, string tag)
        {
            RemoveTag(tag);
        }

        private void TableCellAction(ActionType type, string tag)
        {
            AddTag(tag);
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
            if (_previousQuery == tagField.Text || tagField.Text.Length == 1)
                return;

            _previousQuery = tagField.Text;
            _presenter.Clear();

            ErrorBase error = null;
            if (tagField.Text.Length == 0)
                error = await _presenter.TryGetTopTags();
            else if (tagField.Text.Length > 1)
                error = await _presenter.TryLoadNext(tagField.Text);

            ShowAlert(error);
        }

        private void EditingDidChange(object sender, EventArgs e)
        {
            var txt = ((UITextField)sender).Text;
            if (!string.IsNullOrWhiteSpace(txt))
            {
                if (txt.EndsWith(" "))
                {
                    AddTag(txt);
                }
            }
            _timer.Change(500, Timeout.Infinite);
        }

        private void AddTag(string txt)
        {
            if (!_viewSource.LocalTags.Contains(txt))
            {
                tagField.Text = string.Empty;
                _viewSource.LocalTags.Add(txt);
                SetCollectionHeight();
                _flowDelegate.GenerateVariables();
                tagsCollectionView.ReloadData();
                tagsCollectionView.ScrollToItem(NSIndexPath.FromItemSection(_viewSource.LocalTags.Count - 1, 0), UICollectionViewScrollPosition.Right, true);
            }
        }

        private void RemoveTag(string tag)
        {
            _viewSource.LocalTags.Remove(tag);
            _flowDelegate.GenerateVariables();
            tagsCollectionView.ReloadData();
            SetCollectionHeight();
        }
    }

    public class TextFieldWithInsets : UITextField
    {
        public override CGRect TextRect(CGRect forBounds)
        {
            return base.TextRect(forBounds.Inset(20, 0));
        }

        public override CGRect EditingRect(CGRect forBounds)
        {
            return base.EditingRect(forBounds.Inset(20, 0));
        }

        public override CGRect RightViewRect(CGRect forBounds)
        {
            return base.RightViewRect(forBounds.Inset(20, 0));
        }
    }
}
