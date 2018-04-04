using System;
using System.Threading;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
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

            tagField.Font = Constants.Regular14;
            //tagField.TextColor = Constants.R151G155B158;
            tagField.Placeholder = "Hashtag";

            tagField.Layer.CornerRadius = 20;
            tagField.Layer.BorderWidth = 1;
            tagField.Layer.BorderColor = Constants.R255G71B5.CGColor;
            tagField.BecomeFirstResponder();
            //tagField.TextRect(new CGRect(0, 0, 0, 0));
            //tagField.EditingRect =   .TextRect


            var _tableSource = new TagsTableViewSource(_presenter);
            _tableSource.CellAction += TableCellAction;
            tagsTableView.Source = _tableSource;
            tagsTableView.LayoutMargins = UIEdgeInsets.Zero;
            tagsTableView.RegisterClassForCellReuse(typeof(TagTableViewCell), nameof(TagTableViewCell));
            tagsTableView.RegisterNibForCellReuse(UINib.FromName(nameof(TagTableViewCell), NSBundle.MainBundle), nameof(TagTableViewCell));
            tagsTableView.RowHeight = 65f;

            //tagField.Delegate = new TagFieldDelegate(DoneTapped);
            tagField.EditingChanged += EditingDidChange;
            //tagField.EditingDidBegin += EditingDidBegin;
            //tagField.EditingDidEnd += EditingDidEnd;
            SetBackButton();
            SetCollectionHeight();
            SearchTextChanged();
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
                    ((UITextField)sender).Text = string.Empty;
                    AddTag(txt);
                }
            }
            _timer.Change(500, Timeout.Infinite);
        }

        private void AddTag(string txt)
        {
            if (!_viewSource.LocalTags.Contains(txt))
            {
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
}

