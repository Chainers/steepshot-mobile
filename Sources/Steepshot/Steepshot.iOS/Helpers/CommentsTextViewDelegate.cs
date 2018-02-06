using Foundation;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    class CommentsTextViewDelegate : BaseTextViewDelegate
    {
        public override bool ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            if (text == "\n")
            {
                textView.ResignFirstResponder();
                return false;
            }
            return true;
        }

        public override void Changed(UITextView textView)
        {
            if (textView.SizeThatFits(textView.Frame.Size).Height >= 98)
                textView.ScrollEnabled = true;
            else
                textView.ScrollEnabled = false;
        }
    }
}
