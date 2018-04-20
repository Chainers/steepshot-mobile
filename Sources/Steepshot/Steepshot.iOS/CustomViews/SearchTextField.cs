using System;
using CoreGraphics;
using Foundation;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class SearchTextField : UITextField
    {
        public UIButton ClearButton
        {
            get;
            private set;
        }

        public Action ClearButtonTapped;

        public SearchTextField(Action returnButtonTapped)
        {
            ClearButton = new UIButton();
            ClearButton.Hidden = true;
            ClearButton.SetImage(UIImage.FromBundle("ic_delete_tag"), UIControlState.Normal);
            ClearButton.Frame = new CGRect(0, 0, 16, 16);
            ClearButton.TouchDown += (sender, e) =>
            {
                Text = string.Empty;
                ClearButton.Hidden = true;
                ((TagFieldDelegate)Delegate).ChangeBackground(this);
                ClearButtonTapped?.Invoke();
            };
            RightView = ClearButton;
            RightViewMode = UITextFieldViewMode.Always;

            var _searchPlaceholderAttributes = new UIStringAttributes
            {
                Font = Constants.Regular14,
                ForegroundColor = Constants.R151G155B158,
            };

            var at = new NSMutableAttributedString();
            at.Append(new NSAttributedString("Hashtag", _searchPlaceholderAttributes));
            AttributedPlaceholder = at;
            AutocorrectionType = UITextAutocorrectionType.No;
            AutocapitalizationType = UITextAutocapitalizationType.None;
            Font = Constants.Regular14;
            Layer.CornerRadius = 20;

            Delegate = new TagFieldDelegate(returnButtonTapped);
        }

        public override CGRect TextRect(CGRect forBounds)
        {
            return base.TextRect(forBounds.Inset(20, 0));
        }

        public override CGRect EditingRect(CGRect forBounds)
        {
            return base.EditingRect(forBounds.Inset(20, 0));
        }

        public override CGRect RightViewRect(CGRect forBounds)
        {
            return base.RightViewRect(forBounds.Inset(20, 0));
        }
    }
}
