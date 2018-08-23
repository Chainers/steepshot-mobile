using System;
using Foundation;
using Xamarin.TTTAttributedLabel;

namespace Steepshot.iOS.Delegates
{
    public class TTTAttributedLabelSimpleDelegate : TTTAttributedLabelDelegate
    {
        private Action<string> _tagAction;

        public TTTAttributedLabelSimpleDelegate(Action<string> tagAction)
        {
            _tagAction = tagAction;
        }

        public override void DidSelectLinkWithURL(TTTAttributedLabel label, NSUrl url)
        {
            _tagAction?.Invoke(url.Description);
        }
    }
}
