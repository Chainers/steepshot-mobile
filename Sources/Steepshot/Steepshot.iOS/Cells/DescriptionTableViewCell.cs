using System;
using Foundation;
using Steepshot.Core.Models.Common;
using UIKit;
using Xamarin.TTTAttributedLabel;
using PureLayout.Net;
using Steepshot.Core.Utils;

namespace Steepshot.iOS.Cells
{
    public partial class DescriptionTableViewCell : UITableViewCell
    {
        private readonly TTTAttributedLabel _attributedLabel = new TTTAttributedLabel();
        private bool _isInitialized;

        protected DescriptionTableViewCell(IntPtr handle) : base(handle)
        { 
            _attributedLabel.EnabledTextCheckingTypes = NSTextCheckingType.Link;
            var prop = new NSDictionary();
            _attributedLabel.LinkAttributes = prop;
            _attributedLabel.ActiveLinkAttributes = prop;

            AddSubview(_attributedLabel);
            _attributedLabel.Font = Helpers.Constants.Regular14;
            _attributedLabel.Lines = 0;
            _attributedLabel.UserInteractionEnabled = true;
            _attributedLabel.Enabled = true;
            _attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            _attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 15f);

            var separator = new UIView();
            separator.BackgroundColor = Helpers.Constants.R245G245B245;
            AddSubview(separator);

            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _attributedLabel, 15f);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            separator.AutoSetDimension(ALDimension.Height, 1);
        }

        public void Initialize(Post post, Action<string> TagAction)
        {
            if (_isInitialized)
                return;
            _isInitialized = true;
            var noLinkAttribute = new UIStringAttributes
            {
                Font = Helpers.Constants.Regular14,
                ForegroundColor = Helpers.Constants.R15G24B30,
            };

            var at = new NSMutableAttributedString();

            at.Append(new NSAttributedString(post.Title, noLinkAttribute));
            if (!string.IsNullOrEmpty(post.Description))
            {
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(post.Description, noLinkAttribute));
            }

            foreach (var tag in post.Tags)
            {
                if (tag == "steepshot")
                    continue;
                var linkAttribute = new UIStringAttributes
                {
                    Link = new NSUrl(tag),
                    Font = Helpers.Constants.Regular14,
                    ForegroundColor = Helpers.Constants.R231G72B0,
                };
                at.Append(new NSAttributedString($" #{tag}", linkAttribute));
            }
            _attributedLabel.SetText(at);

            _attributedLabel.Delegate = new TTTAttributedLabelFeedDelegate(TagAction);
        }

        public void ReleaseCell()
        {
            _attributedLabel.Delegate = null;
        }
    }
}
