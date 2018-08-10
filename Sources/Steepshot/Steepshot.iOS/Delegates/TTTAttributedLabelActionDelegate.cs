using System;
using Foundation;
using Steepshot.iOS.Views;
using Xamarin.TTTAttributedLabel;

namespace Steepshot.iOS.Delegates
{
    public class TTTAttributedLabelActionDelegate : TTTAttributedLabelDelegate
    {
        private Action<PlagiarismLinkType> _linkAction;

        public TTTAttributedLabelActionDelegate(Action<PlagiarismLinkType> linkAction)
        {
            _linkAction = linkAction;
        }

        public override void DidSelectLinkWithURL(TTTAttributedLabel label, NSUrl url)
        {
            var type = (PlagiarismLinkType)Enum.Parse(typeof(PlagiarismLinkType), url.ToString(), true);
            _linkAction?.Invoke(type);
        }
    }
}
