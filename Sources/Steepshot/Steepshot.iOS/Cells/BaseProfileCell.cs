using System;
using Foundation;
using Steepshot.Core.Models.Common;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public abstract class BaseProfileCell : UICollectionViewCell
    {
        protected BaseProfileCell(IntPtr handle) : base(handle)
        {
        }
        public string Author;
        public abstract void UpdateCell(Post post, NSMutableAttributedString comment = null);
    }
}
