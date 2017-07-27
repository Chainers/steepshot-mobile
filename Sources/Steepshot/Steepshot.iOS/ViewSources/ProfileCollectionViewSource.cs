using System;
using System.Collections.Generic;
using Foundation;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
    public class ProfileCollectionViewSource : UICollectionViewSource
    {
        public List<Post> PhotoList = new List<Post>();
        public List<NSMutableAttributedString> FeedStrings = new List<NSMutableAttributedString>();

        public bool IsGrid = true;
        public event VoteEventHandler<OperationResult<VoteResponse>> Voted;
        public event VoteEventHandler<OperationResult<FlagResponse>> Flagged;
        public event HeaderTappedHandler GoToProfile;
        public event HeaderTappedHandler GoToComments;
        public event HeaderTappedHandler GoToVoters;
        public event ImagePreviewHandler ImagePreview;

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return PhotoList.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
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
                        Voted?.Invoke(vote, url, action);
                    };
                }
                if (!((FeedCollectionViewCell)cell).IsFlaggedSet)
                {
                    ((FeedCollectionViewCell)cell).Flagged += (vote, url, action) =>
                    {
                        Flagged?.Invoke(vote, url, action);
                    };
                }
                if (!((FeedCollectionViewCell)cell).IsGoToProfileSet)
                {
                    ((FeedCollectionViewCell)cell).GoToProfile += (username) =>
                    {
                        GoToProfile?.Invoke(username);
                    };
                }
                if (!((FeedCollectionViewCell)cell).IsGoToCommentsSet)
                {
                    ((FeedCollectionViewCell)cell).GoToComments += (postUrl) =>
                    {
                        GoToComments?.Invoke(postUrl);
                    };
                }
                if (!((FeedCollectionViewCell)cell).IsGoToVotersSet)
                {
                    ((FeedCollectionViewCell)cell).GoToVoters += (postUrl) =>
                    {
                        if (GoToVoters != null)
                            GoToVoters(postUrl);
                    };
                }
                if (!((FeedCollectionViewCell)cell).IsImagePreviewSet)
                {
                    ((FeedCollectionViewCell)cell).ImagePreview += (image, url) =>
                    {
                        if (ImagePreview != null)
                            ImagePreview(image, url);
                    };
                }
            }
            try
            {
                cell.UpdateCell(PhotoList[(int)indexPath.Item], FeedStrings[(int)indexPath.Item]);
            }
            catch (Exception ex)
            {
                //ignore ^^
            }
            return cell;
        }
    }
}
