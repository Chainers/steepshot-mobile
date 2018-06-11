using Foundation;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class TagsSearchViewController
    {
        private SearchTextField searchTextField;
        private UIButton tagsButton;
        private UIButton peopleButton;
        private UITableView tagsTable;
        private UITableView usersTable;
        private UILabel _noResultViewTags = new UILabel();
        private UILabel _noResultViewPeople = new UILabel();

        private UIActivityIndicatorView _tagsLoader;
        private UIActivityIndicatorView _peopleLoader;

        private NSLayoutConstraint _tagsNotFoundHorizontalAlignment;
        private NSLayoutConstraint _peopleNotFoundHorizontalAlignment;
        private NSLayoutConstraint _tagsHorizontalAlignment;
        private NSLayoutConstraint _peopleHorizontalAlignment;
        private NSLayoutConstraint pinToTags;
        private NSLayoutConstraint pinToPeople;
        private NSLayoutConstraint tagTableVisible;
        private NSLayoutConstraint tagTableHidden;
        private NSLayoutConstraint warningViewToBottomConstraint;
        private bool _isWarningOpen;
        private UIView warningView;

        private void CreateView()
        {
            View.BackgroundColor = UIColor.White;
            searchTextField = new SearchTextField(ShouldReturn, "Tap to search");
            searchTextField.BecomeFirstResponder();
            searchTextField.Font = Constants.Regular14;
            View.AddSubview(searchTextField);

            searchTextField.ClearButtonTapped += () => { OnTimer(null); };
            searchTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 10f);
            searchTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15f);
            searchTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15f);
            searchTextField.AutoSetDimension(ALDimension.Height, 40f);

            tagsButton = new UIButton();
            tagsButton.SetTitle("Tag", UIControlState.Normal);
            tagsButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            tagsButton.SetTitleColor(Constants.R255G34B5, UIControlState.Selected);
            tagsButton.Font = Constants.Semibold14;

            peopleButton = new UIButton();
            peopleButton.SetTitle("User", UIControlState.Normal);
            peopleButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            peopleButton.SetTitleColor(Constants.R255G34B5, UIControlState.Selected);
            peopleButton.Font = Constants.Semibold14;

            View.AddSubviews(new[] { tagsButton, peopleButton });

            tagsButton.AutoSetDimension(ALDimension.Height, 50f);
            tagsButton.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width / 2);
            tagsButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            tagsButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, searchTextField);

            peopleButton.AutoSetDimension(ALDimension.Height, 50f);
            peopleButton.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width / 2);
            peopleButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            peopleButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, searchTextField);

            var underline = new UIView();
            underline.BackgroundColor = Constants.R245G245B245;
            View.AddSubview(underline);

            underline.AutoSetDimension(ALDimension.Height, 1f);
            underline.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            underline.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            underline.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, tagsButton, 1);

            var selectedUnderline = new UIView();
            selectedUnderline.BackgroundColor = Constants.R255G34B5;
            View.AddSubview(selectedUnderline);

            selectedUnderline.AutoSetDimension(ALDimension.Height, 2f);
            selectedUnderline.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width / 2);
            selectedUnderline.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, tagsButton);

            pinToTags = selectedUnderline.AutoPinEdge(ALEdge.Left, ALEdge.Left, tagsButton);
            pinToPeople = selectedUnderline.AutoPinEdge(ALEdge.Left, ALEdge.Left, peopleButton);
            pinToPeople.Active = false;

            tagsTable = new UITableView();
            View.AddSubview(tagsTable);

            tagsTable.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, underline);
            tagTableVisible = tagsTable.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            tagTableHidden = tagsTable.AutoPinEdge(ALEdge.Right, ALEdge.Left, View, -30);
            tagTableHidden.Active = false;
            tagsTable.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width - 60);
            tagsTable.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            usersTable = new UITableView();
            View.AddSubview(usersTable);

            usersTable.AutoPinEdge(ALEdge.Top, ALEdge.Top, tagsTable);
            usersTable.AutoPinEdge(ALEdge.Left, ALEdge.Right, tagsTable, 30);
            usersTable.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width);
            usersTable.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, tagsTable);

            CreateNoResultView(_noResultViewTags, tagsTable);

            _noResultViewTags.AutoPinEdge(ALEdge.Right, ALEdge.Right, tagsTable, 12);
            _noResultViewTags.AutoPinEdge(ALEdge.Left, ALEdge.Left, tagsTable, -12);
            _tagsNotFoundHorizontalAlignment = _noResultViewTags.AutoAlignAxis(ALAxis.Horizontal, tagsTable);

            CreateNoResultView(_noResultViewPeople, usersTable);
            _noResultViewPeople.AutoPinEdge(ALEdge.Right, ALEdge.Right, usersTable, -18);
            _noResultViewPeople.AutoPinEdge(ALEdge.Left, ALEdge.Left, usersTable, 18);
            _peopleNotFoundHorizontalAlignment = _noResultViewPeople.AutoAlignAxis(ALAxis.Horizontal, usersTable);

            _tagsLoader = new UIActivityIndicatorView();
            _tagsLoader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            _tagsLoader.Color = Constants.R231G72B0;
            _tagsLoader.HidesWhenStopped = true;
            _tagsLoader.StopAnimating();
            View.AddSubview(_tagsLoader);

            _tagsHorizontalAlignment = _tagsLoader.AutoAlignAxis(ALAxis.Horizontal, tagsTable);
            _tagsLoader.AutoAlignAxis(ALAxis.Vertical, tagsTable);

            _peopleLoader = new UIActivityIndicatorView();
            _peopleLoader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            _peopleLoader.Color = Constants.R231G72B0;
            _peopleLoader.HidesWhenStopped = true;
            _peopleLoader.StopAnimating();
            View.AddSubview(_peopleLoader);

            _peopleHorizontalAlignment = _peopleLoader.AutoAlignAxis(ALAxis.Horizontal, usersTable);
            _peopleLoader.AutoAlignAxis(ALAxis.Vertical, usersTable);

            warningView = new UIView();
            warningView.ClipsToBounds = true;
            warningView.BackgroundColor = Constants.R255G34B5;
            warningView.Alpha = 0;
            Constants.CreateShadow(warningView, Constants.R231G72B0, 0.5f, 6, 10, 12);
            View.AddSubview(warningView);

            warningView.AutoSetDimension(ALDimension.Height, 60);
            warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            warningViewToBottomConstraint = warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            var warningImage = new UIImageView();
            warningImage.Image = UIImage.FromBundle("ic_info");

            var warningLabel = new UILabel();
            warningLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TagSearchWarning);
            warningLabel.Lines = 3;
            warningLabel.Font = Constants.Regular12;
            warningLabel.TextColor = UIColor.FromRGB(255, 255, 255);

            warningView.AddSubview(warningLabel);
            warningView.AddSubview(warningImage);

            warningImage.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            warningImage.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            warningImage.AutoSetDimension(ALDimension.Width, 20);
            warningImage.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 20);

            warningLabel.AutoPinEdge(ALEdge.Left, ALEdge.Right, warningImage, 20);
            warningLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            warningLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);

            var tap = new UITapGestureRecognizer(() =>
            {
                searchTextField.ResignFirstResponder();
            });
            View.AddGestureRecognizer(tap);
        }

        private void CreateNoResultView(UILabel label, UITableView tableToBind)
        {
            label.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.NoResultText);
            label.Lines = 2;
            label.Hidden = true;
            label.TextAlignment = UITextAlignment.Center;
            label.Font = Constants.Light27;
            label.TextColor = Constants.R15G24B30;

            View.AddSubview(label);
        }

        private void SetupTables()
        {
            _userTableSource = new FollowTableViewSource(_searchFacade.UserFriendPresenter, usersTable);
            _userTableSource.ScrolledToBottom += GetItems;
            _userTableSource.CellAction += CellAction;
            usersTable.Source = _userTableSource;
            usersTable.AllowsSelection = false;
            usersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            usersTable.LayoutMargins = UIEdgeInsets.Zero;
            usersTable.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            usersTable.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));
            usersTable.RegisterClassForCellReuse(typeof(LoaderCell), nameof(LoaderCell));
            usersTable.RowHeight = 70f;

            var _tagsSource = new TagsTableViewSource(_searchFacade.TagsPresenter, tagsTable, true);
            _tagsSource.CellAction += CellAction;
            tagsTable.Source = _tagsSource;
            tagsTable.AllowsSelection = false;
            tagsTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tagsTable.LayoutMargins = UIEdgeInsets.Zero;
            tagsTable.RegisterClassForCellReuse(typeof(TagTableViewCell), nameof(TagTableViewCell));
            tagsTable.RegisterNibForCellReuse(UINib.FromName(nameof(TagTableViewCell), NSBundle.MainBundle), nameof(TagTableViewCell));
            tagsTable.RowHeight = 65f;
        }
    }
}
