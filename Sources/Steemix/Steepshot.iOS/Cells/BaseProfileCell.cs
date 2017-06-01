using System;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public abstract class BaseProfileCell : UICollectionViewCell
	{
		protected BaseProfileCell(IntPtr handle) : base(handle)
        {
		}
		public string Author;
		public abstract void UpdateCell(Post post);
	}
}
