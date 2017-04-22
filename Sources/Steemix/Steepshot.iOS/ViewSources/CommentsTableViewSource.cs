using System;
using Foundation;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public class CommentsTableViewSource : BaseTableSource<Post>
	{
		string CellIdentifier = nameof(CommentTableViewCell);
		public event VoteEventHandler Voted;
		public event HeaderTappedHandler GoToProfile;

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (CommentTableViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);
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
