using System;
using System.Threading;
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
        private readonly VotersTableViewSource _tableSource = new VotersTableViewSource();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            votersTable.Source = _tableSource;
            votersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            votersTable.LayoutMargins = UIEdgeInsets.Zero;
            votersTable.RegisterClassForCellReuse(typeof(UsersSearchViewCell), nameof(UsersSearchViewCell));
            votersTable.RegisterNibForCellReuse(UINib.FromName(nameof(UsersSearchViewCell), NSBundle.MainBundle), nameof(UsersSearchViewCell));
            _tableSource.TableItems = _presenter.Voters;
            _tableSource.RowSelectedEvent += (row) =>
            {
                var myViewController = new ProfileViewController();
                myViewController.Username = _tableSource.TableItems[row].Username;
                NavigationController.PushViewController(myViewController, true);
            };

            _tableSource.ScrolledToBottom += () =>
            {
                LoadNext();
            };

            LoadNext();
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

        private async void LoadNext()
        {
            if (progressBar.IsAnimating)
                return;

            try
            {
                progressBar.StartAnimating();

                var errors = await _presenter.LoadNext(PostUrl, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None));

                if (errors != null && errors.Count > 0)
                    ShowAlert(errors[0]);
                else
                    votersTable.ReloadData();
            }
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex);
            }
            finally
            {
                progressBar.StopAnimating();
            }
        }
    }
}
