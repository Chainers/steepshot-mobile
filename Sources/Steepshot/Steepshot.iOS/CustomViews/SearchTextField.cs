using System;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class SearchTextField : UITextField
    {
        public Action ClearButtonTapped;
        private NSLayoutConstraint _loaderLeftMargin;

        public UIButton ClearButton
        {
            get;
            private set;
        }

        public UIActivityIndicatorView Loader
        {
            get;
            private set;
        }

        public SearchTextField(Action returnButtonTapped, string placeholder, BaseTextFieldDelegate deleg = null)
        {
            var rightView = new UIView();

            Loader = new UIActivityIndicatorView();
            Loader.Color = Constants.R231G72B0;
            Loader.HidesWhenStopped = true;

            ClearButton = new UIButton();
            ClearButton.Hidden = true;
            ClearButton.SetImage(UIImage.FromBundle("ic_delete_tag"), UIControlState.Normal);
            ClearButton.TouchDown += (sender, e) =>
            {
                Clear();
                ClearButtonTapped?.Invoke();
            };

            rightView.AddSubview(Loader);
            rightView.AddSubview(ClearButton);

            ClearButton.AutoSetDimensionsToSize(new CGSize(16,16));
            ClearButton.AutoPinEdge(ALEdge.Left, ALEdge.Right, Loader, 5);
            ClearButton.AutoAlignAxis(ALAxis.Horizontal, Loader);
            Loader.AutoSetDimensionsToSize(new CGSize(16, 16));
            Loader.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _loaderLeftMargin = Loader.AutoPinEdgeToSuperviewEdge(ALEdge.Left);

            RightView = rightView;
            rightView.AutoSetDimensionsToSize(new CGSize(37, 16));
            RightViewMode = UITextFieldViewMode.Always;

            var _searchPlaceholderAttributes = new UIStringAttributes
            {
                Font = Constants.Regular14,
                ForegroundColor = Constants.R151G155B158,
            };

            var at = new NSMutableAttributedString();
            at.Append(new NSAttributedString(placeholder, _searchPlaceholderAttributes));
            AttributedPlaceholder = at;
            AutocorrectionType = UITextAutocorrectionType.No;
            AutocapitalizationType = UITextAutocapitalizationType.None;
            BackgroundColor = Constants.R245G245B245;
            Font = Constants.Regular14;
            Layer.CornerRadius = 20;
            TintColor = Constants.R255G71B5;

            Delegate = deleg ?? new TagFieldDelegate() { DoneTapped = returnButtonTapped };
            EditingChanged += DoEditingChanged;
            LayoutLoader();
        }

        private void DoEditingChanged(object sender, EventArgs e)
        {
            ClearButton.Hidden = Text.Length == 0;
            LayoutLoader();
        }

        public void Clear()
        {
            Text = string.Empty;
            ClearButton.Hidden = true;
            ((BaseTextFieldDelegate)Delegate).ChangeBackground(this);
            LayoutLoader();
        }

        private void LayoutLoader()
        {
            if (ClearButton.Hidden)
                _loaderLeftMargin.Constant = 21;
            else
                _loaderLeftMargin.Constant = 0;
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
