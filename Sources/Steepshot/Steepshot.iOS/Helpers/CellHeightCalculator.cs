using System;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.iOS.Models;
using UIKit;
using Xamarin.TTTAttributedLabel;

namespace Steepshot.iOS.Helpers
{
    public static class CellHeightCalculator
    {
        private static readonly UIStringAttributes _noLinkAttribute;

        static CellHeightCalculator()
        {
            _noLinkAttribute = new UIStringAttributes
            {
                Font = Constants.Regular14,
                ForegroundColor = Constants.R15G24B30,
            };
        }

        public static CellSizeHelper Calculate(Post post)
        {
            var attributedLabel = new TTTAttributedLabel();
            var at = new NSMutableAttributedString();
            var photoHeight = PhotoHeight.Get(post.ImageSize);

            at.Append(new NSAttributedString(post.Title, _noLinkAttribute));
            if (!string.IsNullOrEmpty(post.Description))
            {
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(post.Description, _noLinkAttribute));
            }

            foreach (var tag in post.Tags)
            {
                if (tag == "steepshot")
                    continue;
                var linkAttribute = new UIStringAttributes
                {
                    Link = new NSUrl(tag),
                    Font = Constants.Regular14,
                    ForegroundColor = Constants.R231G72B0,
                };
                at.Append(new NSAttributedString($" #{tag}", linkAttribute));
            }

            attributedLabel.Lines = 0;
            attributedLabel.SetText(at);

            var textHeight = attributedLabel.SizeThatFits(new CGSize(UIScreen.MainScreen.Bounds.Width - 15 * 2, 0)).Height;

            return new CellSizeHelper(photoHeight, textHeight, at);
        }
    }
}
