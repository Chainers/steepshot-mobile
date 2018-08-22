using System;
using System.Globalization;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class BaseTextField : UITextField
    {
        protected UIEdgeInsets _insets = new UIEdgeInsets();

        public BaseTextField(UIEdgeInsets insets)
        {
            _insets = insets;
        }

        public override CGRect TextRect(CGRect forBounds)
        {
            return base.TextRect(CalculateRect(forBounds));
        }

        public override CGRect EditingRect(CGRect forBounds)
        {
            return base.EditingRect(CalculateRect(forBounds));
        }

        public override CGRect RightViewRect(CGRect forBounds)
        {
            return base.RightViewRect(CalculateRect(forBounds));;
        }



        private CGRect CalculateRect(CGRect forBounds)
        {
            return new CGRect(_insets.Left,
                              _insets.Top,
                              forBounds.Width - _insets.Left - _insets.Right,
                              forBounds.Height - _insets.Top - _insets.Bottom);
        }
    }

    public class SearchTextField : BaseTextField
    {
        public Action ClearButtonTapped;
        private UIView _customRightView;

        public UIButton ClearButton
        {
            get;
            private set;
        }

        public bool IsClearButtonNeeded
        {
            get;
            private set;
        }

        public UIActivityIndicatorView Loader
        {
            get;
            private set;
        }

        public event Action ReturnButtonTapped
        {
            add
            {
                ((BaseTextFieldDelegate)Delegate).DoneTapped = value;
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        public SearchTextField(string placeholder, UIEdgeInsets insets, BaseTextFieldDelegate deleg = null, bool isClearButtonNeeded = true, UIView customRightView = null) : this(placeholder, deleg, isClearButtonNeeded, customRightView)
        {
            _insets = insets;
        }

        public SearchTextField(string placeholder, BaseTextFieldDelegate deleg = null, bool isClearButtonNeeded = true, UIView customRightView = null) : base(new UIEdgeInsets(0, 20, 0, 20))
        {
            _customRightView = customRightView;
            if (isClearButtonNeeded)
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

                ClearButton.AutoCenterInSuperview();
                ClearButton.AutoSetDimensionsToSize(new CGSize(37, 37));
                ClearButton.AutoAlignAxis(ALAxis.Horizontal, Loader);
                Loader.AutoSetDimensionsToSize(new CGSize(16, 16));
                Loader.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
                Loader.AutoPinEdgeToSuperviewEdge(ALEdge.Right, -10);

                RightView = rightView;
                rightView.AutoSetDimensionsToSize(new CGSize(37, 37));
                RightViewMode = UITextFieldViewMode.Always;
            }
            else if(_customRightView != null)
            {
                var rightView = new UIView();
                RightView = rightView;
                RightViewMode = UITextFieldViewMode.Always;
                UpdateRightViewRect();
            }

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

            Delegate = deleg ?? new TagFieldDelegate();
            EditingChanged += DoEditingChanged;
        }

        public double GetDoubleValue()
        {
            double result;
            double.TryParse(Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
            return result;
        }

        public void UpdateRightViewRect()
        {
            if (_customRightView != null)
                RightView.Frame = new CGRect(RightView.Frame.X, RightView.Frame.Y, _customRightView.Frame.Width, _customRightView.Frame.Height);
        }

        private void DoEditingChanged(object sender, EventArgs e)
        {
            if(IsClearButtonNeeded)
                ClearButton.Hidden = Text.Length == 0;
        }

        public void Clear()
        {
            Text = string.Empty;
            ClearButton.Hidden = true;
            ((BaseTextFieldDelegate)Delegate).ChangeBackground(this);
        }
    }
}
