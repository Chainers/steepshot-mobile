using System;
using System.Collections.Generic;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public class ProfileCollectionViewSource : UICollectionViewSource
	{
		public List<Post> PhotoList = new List<Post>();

		public bool IsGrid = true;
		public event VoteEventHandler<VoteResponse> Voted;
		public event VoteEventHandler<OperationResult<FlagResponse>> Flagged;
		public event HeaderTappedHandler GoToProfile;
		public event HeaderTappedHandler GoToComments;
		public event ImagePreviewHandler ImagePreview;

		public ProfileCollectionViewSource()
		{
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			return PhotoList.Count;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, Foundation.NSIndexPath indexPath)
		{
			BaseProfileCell cell;
			if (IsGrid)
				cell = (PhotoCollectionViewCell)collectionView.DequeueReusableCell("PhotoCollectionViewCell", indexPath);
			else
			{
				cell = (FeedCollectionViewCell)collectionView.DequeueReusableCell("FeedCollectionViewCell", indexPath);
			if (!((FeedCollectionViewCell)cell).IsVotedSet)
            {
                ((FeedCollectionViewCell)cell).Voted += (vote, url, action) =>
                {
                    Voted(vote, url, action);
                };
            }
			if (!((FeedCollectionViewCell)cell).IsFlaggedSet)
            {
                ((FeedCollectionViewCell)cell).Flagged += (vote, url, action) =>
                {
                    Flagged(vote, url, action);
                };
            }
			if (!((FeedCollectionViewCell)cell).IsGoToProfileSet)
			{
				((FeedCollectionViewCell)cell).GoToProfile += (username) =>
				{
					if(GoToProfile != null)
						GoToProfile(username);
				};
			}
			if (!((FeedCollectionViewCell)cell).IsGoToCommentsSet)
			{
				((FeedCollectionViewCell)cell).GoToComments += (postUrl) =>
				{
					if(GoToComments != null)
						GoToComments(postUrl);
				};
			}
			if (!((FeedCollectionViewCell)cell).IsImagePreviewSet)
			{
				((FeedCollectionViewCell)cell).ImagePreview += (image, url) =>
				{
					if(ImagePreview != null)
						ImagePreview(image, url);
				};
			}
			}
			try
			{
				cell.UpdateCell(PhotoList[(int)indexPath.Item]);
			}
			catch (ArgumentOutOfRangeException ex)
			{
				//ignore ^^
			}
			return cell;
		}
	}
}
