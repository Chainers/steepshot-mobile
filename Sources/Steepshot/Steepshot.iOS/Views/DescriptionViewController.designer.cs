// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Steepshot.iOS.Views
{
    [Register ("DescriptionViewController")]
    partial class DescriptionViewController
    {
        [Outlet]
        UIKit.NSLayoutConstraint collectionHeight { get; set; }


        [Outlet]
        UIKit.UITextView descriptionTextField { get; set; }


        [Outlet]
        UIKit.UIView loadingView { get; set; }


        [Outlet]
        UIKit.UIImageView photoView { get; set; }


        [Outlet]
        UIKit.UIButton postPhotoButton { get; set; }


        [Outlet]
        UIKit.UIScrollView scrollView { get; set; }


        [Outlet]
        UIKit.UICollectionView tagsCollectionView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel descriptionLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint descriptionVerticalSpacing { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint tagsCollectionVerticalSpacing { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint tagsCollectionVerticalSpacingHidden { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView titleTextField { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (collectionHeight != null) {
                collectionHeight.Dispose ();
                collectionHeight = null;
            }

            if (descriptionLabel != null) {
                descriptionLabel.Dispose ();
                descriptionLabel = null;
            }

            if (descriptionTextField != null) {
                descriptionTextField.Dispose ();
                descriptionTextField = null;
            }

            if (descriptionVerticalSpacing != null) {
                descriptionVerticalSpacing.Dispose ();
                descriptionVerticalSpacing = null;
            }

            if (loadingView != null) {
                loadingView.Dispose ();
                loadingView = null;
            }

            if (photoView != null) {
                photoView.Dispose ();
                photoView = null;
            }

            if (postPhotoButton != null) {
                postPhotoButton.Dispose ();
                postPhotoButton = null;
            }

            if (scrollView != null) {
                scrollView.Dispose ();
                scrollView = null;
            }

            if (tagsCollectionVerticalSpacing != null) {
                tagsCollectionVerticalSpacing.Dispose ();
                tagsCollectionVerticalSpacing = null;
            }

            if (tagsCollectionVerticalSpacingHidden != null) {
                tagsCollectionVerticalSpacingHidden.Dispose ();
                tagsCollectionVerticalSpacingHidden = null;
            }

            if (tagsCollectionView != null) {
                tagsCollectionView.Dispose ();
                tagsCollectionView = null;
            }

            if (titleTextField != null) {
                titleTextField.Dispose ();
                titleTextField = null;
            }
        }
    }
}