using System;
using System.Text.RegularExpressions;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.iOS.Models;
using UIKit;
using Xamarin.TTTAttributedLabel;

namespace Steepshot.iOS.Helpers
{
    public static class CellHeightCalculator
    {
        private static readonly UIStringAttributes _noLinkAttribute;
        private static readonly Regex _tagRegex = new Regex(@"^[a-zA-Z0-9_#]+$");

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
            var photoHeight = post.Media[0].OptimalPhotoSize((float)UIScreen.MainScreen.Bounds.Width, 180, (float)UIScreen.MainScreen.Bounds.Width + 50);

            var attributedLabel = new TTTAttributedLabel();
            var at = new NSMutableAttributedString();

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
                NSUrl tagUrlWithoutWhitespaces = null;
                var tagText = tag.Replace(" ", string.Empty);
                if (_tagRegex.IsMatch(tagText))
                    tagUrlWithoutWhitespaces = new NSUrl(tagText);
                var linkAttribute = new UIStringAttributes
                {
                    Link = tagUrlWithoutWhitespaces,
                    Font = Constants.Regular14,
                    ForegroundColor = Constants.R231G72B0,
                };
                at.Append(new NSAttributedString($" #{tag}", linkAttribute));
            }

            attributedLabel.Lines = 3;
            attributedLabel.SetText(at);

            var textHeight = attributedLabel.SizeThatFits(new CGSize(UIScreen.MainScreen.Bounds.Width - 15 * 2, 0)).Height;

            return new CellSizeHelper(photoHeight, textHeight, at);
        }

        public static CGSize GetDescriptionPostSize(nfloat width, nfloat height, int listCount)
        {
            if (height > width)
            {
                var ratio = width / height;
                if (listCount == 1)
                {
                    return new CGSize(UIScreen.MainScreen.Bounds.Width - Constants.DescriptionSeparatorMargin * 2, (UIScreen.MainScreen.Bounds.Width - Constants.DescriptionSeparatorMargin * 2) / ratio);
                }
                else
                    return new CGSize(Constants.DescriptionCellSide * ratio, Constants.DescriptionCellSide);
            }
            else
            {
                var ratio = height / width;
                if (listCount == 1)
                {
                    return new CGSize(UIScreen.MainScreen.Bounds.Width - 30, (UIScreen.MainScreen.Bounds.Width - 30) * ratio);
                }
                else
                    return new CGSize(UIScreen.MainScreen.Bounds.Width - Constants.DescriptionSectionInset * 2, (UIScreen.MainScreen.Bounds.Width - Constants.DescriptionSectionInset * 2) * ratio);
            }
        }
    }
}
