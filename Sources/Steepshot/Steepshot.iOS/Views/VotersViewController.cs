using System;
using System.Threading.Tasks;
using Foundation;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class VotersViewController : BaseViewControllerWithPresenter<VotersPresenter>
    {
        protected override void CreatePresenter()
        {
            _presenter = new VotersPresenter();
        }

        public string PostUrl;
        private VotersTableViewSource _tableSource = new VotersTableViewSource();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            votersTable.Source = _tableSource;
            votersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            votersTable.LayoutMargins = UIEdgeInsets.Zero;
            votersTable.RegisterClassForCellReuse(typeof(UsersSearchViewCell), nameof(UsersSearchViewCell));
            votersTable.RegisterNibForCellReuse(UINib.FromName(nameof(UsersSearchViewCell), NSBundle.MainBundle), nameof(UsersSearchViewCell));
            _tableSource.TableItems = _presenter.Users;
            _tableSource.RowSelectedEvent += (row) =>
            {
                var myViewController = new ProfileViewController();
                myViewController.Username = _tableSource.TableItems[row].Username;
                NavigationController.PushViewController(myViewController, true);
            };

            _tableSource.ScrolledToBottom += async () =>
            {
                if (_presenter._hasItems)
                    await GetItems();
            };

            GetItems();
        }

        private void OnPostLoaded()
        {
            votersTable.ReloadData();
            progressBar.StopAnimating();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(false, false);
            base.ViewWillAppear(animated);
        }

        public async Task GetItems()
        {
            if (progressBar.IsAnimating)
                return;

            try
            {
                progressBar.StartAnimating();
                await _presenter.GetItems(PostUrl).ContinueWith((g) =>
                {
                    var errors = g.Result;
                    InvokeOnMainThread(() =>
                    {
                        if (errors != null && errors.Count > 0)
                            ShowAlert(errors[0]);
                        votersTable.ReloadData();
                        progressBar.StopAnimating();
                    });
                });
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, AppVersion);
            }
            finally
            {
                progressBar.StopAnimating();
            }
        }
    }
}
