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

        public SearchTextField(Action returnButtonTapped, string placeholder)
        {
            var rightView = new UIView();

            Loader = new UIActivityIndicatorView();
            Loader.StartAnimating();
            Loader.Color = Constants.R231G72B0;
            Loader.HidesWhenStopped = true;

            ClearButton = new UIButton();
            ClearButton.Hidden = true;
            ClearButton.SetImage(UIImage.FromBundle("ic_delete_tag"), UIControlState.Normal);
            ClearButton.TouchDown += (sender, e) =>
            {
                Text = string.Empty;
                ClearButton.Hidden = true;
                ((TagFieldDelegate)Delegate).ChangeBackground(this);
                ClearButtonTapped?.Invoke();
            };

            rightView.AddSubview(Loader);
            rightView.AddSubview(ClearButton);

            ClearButton.AutoSetDimensionsToSize(new CGSize(16,16));
            Loader.AutoSetDimensionsToSize(new CGSize(16, 16));
            ClearButton.AutoPinEdge(ALEdge.Left, ALEdge.Right, Loader, 5);
            _loaderLeftMargin = Loader.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            Loader.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

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

            Delegate = new TagFieldDelegate(returnButtonTapped);
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
