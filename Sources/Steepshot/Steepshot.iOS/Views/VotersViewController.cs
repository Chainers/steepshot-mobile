using Foundation;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class VotersViewController : BaseViewControllerWithPresenter<UserFriendPresenter>
    {
        public string PostUrl;

        protected override void CreatePresenter()
        {
            _presenter = new UserFriendPresenter();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var tableSource = new VotersTableViewSource(_presenter, votersTable);
            votersTable.Source = tableSource;
            votersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            votersTable.LayoutMargins = UIEdgeInsets.Zero;
            votersTable.RegisterClassForCellReuse(typeof(UsersSearchViewCell), nameof(UsersSearchViewCell));
            votersTable.RegisterNibForCellReuse(UINib.FromName(nameof(UsersSearchViewCell), NSBundle.MainBundle), nameof(UsersSearchViewCell));

            tableSource.RowSelectedEvent += (row) =>
            {
                var myViewController = new ProfileViewController();
                myViewController.Username = _presenter[row]?.Author;
                NavigationController.PushViewController(myViewController, true);
            };

            tableSource.ScrolledToBottom += () =>
            {
                LoadNext();
            };

            LoadNext();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(false, false);
            base.ViewWillAppear(animated);
        }

        private async void LoadNext()
        {
            if (progressBar.IsAnimating)
                return;

            progressBar.StartAnimating();

            var error = await _presenter.TryLoadNextPostVoters(PostUrl);

            if (error == null)
                votersTable.ReloadData();
            else
                ShowAlert(error);

            progressBar.StopAnimating();
        }

        public override void ViewDidUnload()
        {
            _presenter.LoadCancel();
            base.ViewDidUnload();
        }
    }
}