using Foundation;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    class CommentsTextViewDelegate : UITextViewDelegate
    {
        public UILabel Placeholder;

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

        public override void EditingStarted(UITextView textView)
        {
            Placeholder.Hidden = true;
        }

        public override void EditingEnded(UITextView textView)
        {
            if (textView.Text.Length > 0)
                Placeholder.Hidden = true;
            else
                Placeholder.Hidden = false;
        }
    }
}
