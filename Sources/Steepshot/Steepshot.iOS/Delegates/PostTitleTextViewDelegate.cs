using System;
using Foundation;
using UIKit;

namespace Steepshot.iOS.Delegates
{
    public class BaseTextViewDelegate : UITextViewDelegate
    {
        public UILabel Placeholder;
        public Action EditingStartedAction;
        public Action EditingEndedAction;

        public override void EditingStarted(UITextView textView)
        {
            Placeholder.Hidden = true;
            EditingStartedAction?.Invoke();
        }

        public override void EditingEnded(UITextView textView)
        {
            if (textView.Text.Length > 0)
                Placeholder.Hidden = true;
            else
                Placeholder.Hidden = false;
            EditingEndedAction?.Invoke();
        }

        public override bool ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            if (text == "\n")
            {
                textView.ResignFirstResponder();
                return false;
            }
            return true;
        }
    }

    public class PostTitleTextViewDelegate : BaseTextViewDelegate
    {
        private readonly int _textLimit;

        public PostTitleTextViewDelegate(int textLimit = 255)
        {
            _textLimit = textLimit;
        }

        public override bool ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            if ((textView.Text + text).Length > _textLimit)
                return false;
            return true;
        }
    }
}
