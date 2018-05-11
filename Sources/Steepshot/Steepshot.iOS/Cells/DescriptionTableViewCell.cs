using System;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Extensions;
using UIKit;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using CoreGraphics;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Xamarin.TTTAttributedLabel;
using PureLayout.Net;

namespace Steepshot.iOS.Cells
{
    public partial class DescriptionTableViewCell : UITableViewCell
    {
        protected DescriptionTableViewCell(IntPtr handle) : base(handle) { }
        public static readonly NSString Key = new NSString(nameof(DescriptionTableViewCell));
        public static readonly UINib Nib;

        static DescriptionTableViewCell()
        {
            Nib = UINib.FromName(nameof(DescriptionTableViewCell), NSBundle.MainBundle);
        }

        public void UpdateCell(Post post, Action<string> TagAction)
        {
            var attributedLabel = new TTTAttributedLabel();
            attributedLabel.EnabledTextCheckingTypes = NSTextCheckingType.Link;
            var prop = new NSDictionary();
            attributedLabel.LinkAttributes = prop;
            attributedLabel.ActiveLinkAttributes = prop;

            DescriptionView.AddSubview(attributedLabel);
            attributedLabel.Font = Helpers.Constants.Regular14;
            attributedLabel.Lines = 0;
            attributedLabel.UserInteractionEnabled = true;
            attributedLabel.Enabled = true;
            attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 15f);
            attributedLabel.Delegate = new TTTAttributedLabelFeedDelegate(TagAction);

            var separator = new UIView();
            separator.BackgroundColor = iOS.Helpers.Constants.R245G245B245;
            DescriptionView.AddSubview(separator);

            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, attributedLabel, 15f);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            separator.AutoSetDimension(ALDimension.Height, 1);

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
            attributedLabel.SetText(at);


        }
    }
}
