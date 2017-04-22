using System;
using System.Collections.Generic;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public class ProfileCollectionViewSource : UICollectionViewSource
	{
		public List<Post> PhotoList = new List<Post>();

		public ProfileCollectionViewSource()
		{
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			return PhotoList.Count;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, Foundation.NSIndexPath indexPath)
		{
			var imageCell = (PhotoCollectionViewCell)collectionView.DequeueReusableCell("PhotoCollectionViewCell", indexPath);
			imageCell.UpdateImage(PhotoList[(int)indexPath.Item].Body);
			return imageCell;
		}
	}
}
