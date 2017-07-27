using System;
using System.Collections.Generic;
using Foundation;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public class FollowTableViewSource : BaseTableSource<UserFriend>
	{
		string _cellIdentifier = nameof(FollowViewCell);
		public event FollowEventHandler Follow;
		public event HeaderTappedHandler GoToProfile;

		public FollowTableViewSource()
		{
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (FollowViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);
			if (!cell.IsFollowSet)
			{
				cell.Follow += (followType, authorName, success) =>
				{
					if(Follow != null)
                        Follow(followType, authorName, success);
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
	}
}
