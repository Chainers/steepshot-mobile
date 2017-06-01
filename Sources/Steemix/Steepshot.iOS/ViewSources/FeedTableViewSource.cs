using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Foundation;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
    
	public class FeedTableViewSource : BaseTableSource<Post>
    {
		string CellIdentifier = nameof(FeedTableViewCell);
		//public event VoteEventHandler<VoteResponse> Voted;
		//public event VoteEventHandler<FlagResponse> Flagged;
		public event HeaderTappedHandler GoToProfile;
		public event HeaderTappedHandler GoToComments;
		public event ImagePreviewHandler ImagePreview;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (FeedTableViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);
            if (!cell.IsVotedSet)
            {
                cell.Voted += (vote, url, action) =>
                {
                    //Voted(vote, url, action);
                };
            }
			/*if (!cell.IsFlaggedSet)
            {
				cell.Flagged += (vote, url, action) =>
                {
                    Flagged(vote, url, action);
                };
            }*/
			if (!cell.IsGoToProfileSet)
			{
				cell.GoToProfile += (username) =>
				{
					if(GoToProfile != null)
						GoToProfile(username);
				};
			}
			if (!cell.IsGoToCommentsSet)
			{
				cell.GoToComments += (postUrl) =>
				{
					if(GoToComments != null)
                        GoToComments(postUrl);
				};
			}
			if (!cell.IsImagePreviewSet)
			{
				cell.ImagePreview += (image, url) =>
				{
					if(ImagePreview != null)
                        ImagePreview(image, url);
				};
			}
			cell.UpdateCell(TableItems[indexPath.Row]);
            return cell;
        }  
    }
}
