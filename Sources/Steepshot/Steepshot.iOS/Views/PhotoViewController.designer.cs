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
    [Register ("PhotoViewController")]
    partial class PhotoViewController
    {
        [Outlet]
        UIKit.UIButton closeButton { get; set; }


        [Outlet]
        UIKit.UIButton enableCameraAccess { get; set; }


        [Outlet]
        UIKit.UIButton flashButton { get; set; }


        [Outlet]
        UIKit.UIView liveCameraStream { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint topCloseBtnConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint topFlashBtnConstraint { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (closeButton != null) {
                closeButton.Dispose ();
                closeButton = null;
            }

            if (enableCameraAccess != null) {
                enableCameraAccess.Dispose ();
                enableCameraAccess = null;
            }

            if (flashButton != null) {
                flashButton.Dispose ();
                flashButton = null;
            }

            if (liveCameraStream != null) {
                liveCameraStream.Dispose ();
                liveCameraStream = null;
            }

            if (topCloseBtnConstraint != null) {
                topCloseBtnConstraint.Dispose ();
                topCloseBtnConstraint = null;
            }

            if (topFlashBtnConstraint != null) {
                topFlashBtnConstraint.Dispose ();
                topFlashBtnConstraint = null;
            }
        }
    }
}