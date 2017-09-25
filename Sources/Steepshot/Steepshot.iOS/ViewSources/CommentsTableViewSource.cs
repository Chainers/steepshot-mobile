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
		public event HeaderTappedHandler GoToProfile;

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (CommentTableViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);
			if (!cell.IsVotedSet)
            {
                cell.Voted += (vote, url, action) =>
                {
					Voted(vote, url, action);
                };
            }
			if (!cell.IsGoToProfileSet)
			{
				cell.GoToProfile += (username) =>
				{
					if(GoToProfile != null)
						GoToProfile(username);
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
