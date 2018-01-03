using System;

using Foundation;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class TagTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("TagTableViewCell");
        public static readonly UINib Nib;

        static TagTableViewCell()
        {
            Nib = UINib.FromName("TagTableViewCell", NSBundle.MainBundle);
        }

        protected TagTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }
    }
}
