using System;
using Foundation;
using Xamarin.TTTAttributedLabel;

public class TTTAttributedLabelFeedDelegate : TTTAttributedLabelDelegate
{
    private Action<string> _tagAction;

    public TTTAttributedLabelFeedDelegate(Action<string> tagAction)
    {
        _tagAction = tagAction;
    }

    public override void DidSelectLinkWithURL(TTTAttributedLabel label, NSUrl url)
    {
        var t = url.Description.Replace('#', ' ');
        _tagAction?.Invoke(t);
    }
}
