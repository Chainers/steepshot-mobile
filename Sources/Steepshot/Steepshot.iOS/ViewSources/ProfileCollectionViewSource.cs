using System;
using System.Collections.Generic;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class ProfileCollectionViewSource : UICollectionViewSource
    {
        public readonly List<NSMutableAttributedString> FeedStrings;
        public event VoteEventHandler<OperationResult<VoteResponse>> Voted;
        public event VoteEventHandler<OperationResult<VoteResponse>> Flagged;
        public event HeaderTappedHandler GoToProfile;
        public event HeaderTappedHandler GoToComments;
        public event HeaderTappedHandler GoToVoters;
        public event ImagePreviewHandler ImagePreview;
        public bool IsGrid = true;
        private readonly BasePostPresenter _presenter;

        public ProfileCollectionViewSource(BasePostPresenter presenter)
        {
            FeedStrings = new List<NSMutableAttributedString>();
            _presenter = presenter;
        }
        
        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return _presenter.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            BaseProfileCell cell;
            if (IsGrid)
                cell = (PhotoCollectionViewCell)collectionView.DequeueReusableCell(nameof(PhotoCollectionViewCell), indexPath);
            else
            {
                cell = (FeedCollectionViewCell)collectionView.DequeueReusableCell(nameof(FeedCollectionViewCell), indexPath);
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
                        GoToVoters?.Invoke(postUrl);
                    };
                }
                if (!((FeedCollectionViewCell)cell).IsImagePreviewSet)
                {
                    ((FeedCollectionViewCell)cell).ImagePreview += (image, url) =>
                    {
                        ImagePreview?.Invoke(image, url);
                    };
                }
            }
            try
            {
                var post = _presenter[(int)indexPath.Item];
                if (post != null)
                    cell.UpdateCell(post, FeedStrings[(int)indexPath.Item]);
            }
            catch (Exception ex)
            {
                //ignore ^^
#if DEBUG
                Console.WriteLine(ex.Message + ex.StackTrace);
#endif
            }
            return cell;
        }
    }
}
