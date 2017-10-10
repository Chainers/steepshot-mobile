using Foundation;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class VotersViewController : BaseViewControllerWithPresenter<VotersPresenter>
    {
        public string PostUrl;

        protected override void CreatePresenter()
        {
            _presenter = new VotersPresenter();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var tableSource = new VotersTableViewSource(_presenter);
            votersTable.Source = tableSource;
            votersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            votersTable.LayoutMargins = UIEdgeInsets.Zero;
            votersTable.RegisterClassForCellReuse(typeof(UsersSearchViewCell), nameof(UsersSearchViewCell));
            votersTable.RegisterNibForCellReuse(UINib.FromName(nameof(UsersSearchViewCell), NSBundle.MainBundle), nameof(UsersSearchViewCell));

            tableSource.RowSelectedEvent += (row) =>
            {
                var myViewController = new ProfileViewController();
                myViewController.Username = _presenter[row]?.Username;
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

            var errors = await _presenter.TryLoadNext(PostUrl);

            if (errors != null && errors.Count > 0)
                ShowAlert(errors);
            else
                votersTable.ReloadData();

            progressBar.StopAnimating();
        }

        public override void ViewDidUnload()
        {
            _presenter.LoadCancel();
            base.ViewDidUnload();
        }
    }
}