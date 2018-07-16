using System;
using Foundation;

namespace Steepshot.iOS.Models
{
    public class CellSizeHelper
    {
        public nfloat PhotoHeight{ get; set; }
        public nfloat TextHeight { get; set; }
        public NSMutableAttributedString Text { get; set; }
        public nfloat CellHeight => PhotoHeight + TextHeight + 182;

        public CellSizeHelper(nfloat PhotoHeight, nfloat TextHeight, NSMutableAttributedString Text)
        {
            this.PhotoHeight = PhotoHeight;
            this.TextHeight = TextHeight;
            this.Text = Text;
        }
    }
}
