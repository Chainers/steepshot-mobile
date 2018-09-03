using System;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Popups
{
    public static class PostCreatedPopup
    {
        public static void Show(UIView viewToAdd)
        {
            var warningView = new CustomView();
            warningView.ClipsToBounds = true;
            warningView.BackgroundColor = Constants.R255G34B5;
            warningView.Alpha = 0;
            Constants.CreateShadow(warningView, Constants.R231G72B0, 0.5f, 6, 10, 12);
            viewToAdd.AddSubview(warningView);

            warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            var warningViewToTopConstraint = warningView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, viewToAdd);
            var warningViewToBottomConstraint = warningView.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, viewToAdd, -20);
            warningViewToBottomConstraint.Active = false;

            var warningImage = new UIImageView();
            warningImage.Image = UIImage.FromBundle("ic_info");

            var warningLabel = new UILabel();
            warningLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PostDelay);
            warningLabel.Lines = 5;
            warningLabel.Font = Constants.Regular12;
            warningLabel.TextColor = UIColor.FromRGB(255, 255, 255);

            warningView.AddSubview(warningLabel);
            warningView.AddSubview(warningImage);

            warningImage.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            warningImage.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            warningImage.SetContentCompressionResistancePriority(999, UILayoutConstraintAxis.Horizontal);

            warningLabel.AutoPinEdge(ALEdge.Left, ALEdge.Right, warningImage, 20);
            warningLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            warningLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 20);
            warningLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);

            warningView.SubviewLayouted += () =>
            {
                UIView.Animate(0.3f, 0f, UIViewAnimationOptions.CurveEaseOut, () =>
                {
                    warningViewToTopConstraint.Active = false;
                    warningViewToBottomConstraint.Active = true;
                    warningView.Alpha = 1;
                    viewToAdd.LayoutIfNeeded();
                }, () =>
                {
                    UIView.Animate(0.2f, 7f, UIViewAnimationOptions.CurveEaseIn, () =>
                    {
                        warningViewToTopConstraint.Active = true;
                        warningViewToBottomConstraint.Active = false;
                        warningView.Alpha = 0;
                        viewToAdd.LayoutIfNeeded();
                    }, () => {
                        warningView.RemoveFromSuperview();
                        warningView = null;
                    });
                });
            };
        }
    }
}
