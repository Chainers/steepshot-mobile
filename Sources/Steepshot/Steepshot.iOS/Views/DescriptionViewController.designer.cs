// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Steepshot.iOS.Views
{
    [Register ("DescriptionViewController")]
    partial class DescriptionViewController
    {
        [Outlet]
        UIKit.UITextView descriptionTextField { get; set; }

        [Outlet]
        UIKit.UIActivityIndicatorView loadingView { get; set; }

        [Outlet]
        UIKit.NSLayoutConstraint localTagsHeight { get; set; }

        [Outlet]
        UIKit.NSLayoutConstraint localTagsTopSpace { get; set; }

        [Outlet]
        UIKit.UIImageView photoView { get; set; }

        [Outlet]
        UIKit.UIButton postPhotoButton { get; set; }

        [Outlet]
        UIKit.UIImageView rotateImage { get; set; }

        [Outlet]
        UIKit.NSLayoutConstraint tagDefault { get; set; }

        [Outlet]
        UIKit.UITextField tagField { get; set; }

        [Outlet]
        UIKit.UICollectionView tagsCollectionView { get; set; }

        [Outlet]
        UIKit.UITableView tagsTableView { get; set; }

        [Outlet]
        UIKit.NSLayoutConstraint tagToTop { get; set; }

        [Outlet]
        UIKit.UIView titleBottomView { get; set; }

        [Outlet]
        UIKit.UIImageView titleEditImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView titleTextField { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (descriptionTextField != null) {
                descriptionTextField.Dispose ();
                descriptionTextField = null;
            }

            if (loadingView != null) {
                loadingView.Dispose ();
                loadingView = null;
            }

            if (localTagsHeight != null) {
                localTagsHeight.Dispose ();
                localTagsHeight = null;
            }

            if (localTagsTopSpace != null) {
                localTagsTopSpace.Dispose ();
                localTagsTopSpace = null;
            }

            if (photoView != null) {
                photoView.Dispose ();
                photoView = null;
            }

            if (postPhotoButton != null) {
                postPhotoButton.Dispose ();
                postPhotoButton = null;
            }

            if (tagDefault != null) {
                tagDefault.Dispose ();
                tagDefault = null;
            }

            if (tagField != null) {
                tagField.Dispose ();
                tagField = null;
            }

            if (tagsCollectionView != null) {
                tagsCollectionView.Dispose ();
                tagsCollectionView = null;
            }

            if (tagsTableView != null) {
                tagsTableView.Dispose ();
                tagsTableView = null;
            }

            if (tagToTop != null) {
                tagToTop.Dispose ();
                tagToTop = null;
            }

            if (titleBottomView != null) {
                titleBottomView.Dispose ();
                titleBottomView = null;
            }

            if (titleEditImage != null) {
                titleEditImage.Dispose ();
                titleEditImage = null;
            }

            if (titleTextField != null) {
                titleTextField.Dispose ();
                titleTextField = null;
            }

            if (rotateImage != null) {
                rotateImage.Dispose ();
                rotateImage = null;
            }
        }
    }
}
