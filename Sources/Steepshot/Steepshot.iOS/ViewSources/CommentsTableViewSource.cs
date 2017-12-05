using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class CommentsTableViewSource : BaseTableSource<Post>
    {
        string _cellIdentifier = nameof(CommentTableViewCell);
        public event VoteEventHandler<VoteResponse> Voted;
        public event VoteEventHandler<VoteResponse> Flaged;
        public event HeaderTappedHandler GoToProfile;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (CommentTableViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);
            if (!cell.IsVotedSet)
            {
                cell.Voted += (vote, url, action) =>
                {
                    Voted?.Invoke(vote, url, action);
                };
            }
            if (!cell.IsFlagedSet)
            {
                cell.Flaged += (vote, postUri, action) =>
                {
                    Flaged?.Invoke(vote, postUri, action);
                };
            }
            if (!cell.IsGoToProfileSet)
            {
                cell.GoToProfile += (username) =>
                {
                    GoToProfile?.Invoke(username);
                };
            }
            cell.UpdateCell(TableItems[indexPath.Row]);
            return cell;
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            /*if (indexPath.Row == TableItems.Count - 1 && ScrolledToBottom != null)
			{
				ScrolledToBottom();
			}*/
        }
    }
}
