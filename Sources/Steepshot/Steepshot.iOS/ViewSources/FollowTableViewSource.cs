using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class FollowTableViewSource : BaseUiTableViewSource<UserFriend>
    {
        string _cellIdentifier = nameof(FollowViewCell);
        public event FollowEventHandler Follow;
        //public event HeaderTappedHandler GoToProfile;

        public FollowTableViewSource(UserFriendPresenter presenter) : base(presenter) { }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (FollowViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);
            if (!cell.IsFollowSet)
            {
                cell.Follow += (followType, authorName, success) =>
                {
                    Follow?.Invoke(followType, authorName, success);
                };
            }
            /*
            if (!cell.IsGoToProfileSet)
            {
                
                cell.GoToProfile += (username) =>
                {
                    GoToProfile?.Invoke(username);
                };
            }*/
            var user = Presenter[indexPath.Row];
            if (user != null)
                cell.UpdateCell(user);
            return cell;
        }
    }
}
