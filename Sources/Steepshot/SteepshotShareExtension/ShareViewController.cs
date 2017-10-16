using System;
using System.IO;
using Foundation;
using CoreFoundation;
using Social;
using ObjCRuntime;
using MobileCoreServices;
using UIKit;

namespace SteepshotShareExtension
{
    public partial class ShareViewController : SLComposeServiceViewController
    {
        private nfloat oldAlpha = (nfloat)1.0;
        private string APP_SHARE_GROUP = "group.com.chainartsoft.Steepshot";

        protected ShareViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
        }

        public override bool IsContentValid()
        {
            // Do validation of contentText and/or NSExtensionContext attachments here
            return true;
        }

        public override void DidSelectPost()
        {
            return;
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            oldAlpha = base.View.Alpha;
            base.View.Alpha = 0f;
        }

        public override void DidMoveToParentViewController(UIViewController parent)
        {
            base.View.Alpha = oldAlpha;
        }

        public override SLComposeSheetConfigurationItem[] GetConfigurationItems()
        {
            PassSelectedItemsToApp();
            return new SLComposeSheetConfigurationItem[0];
        }

        private void PassSelectedItemsToApp()
        {
            NSExtensionItem firstItem = base.ExtensionContext.InputItems[0];

            var itemsCount = firstItem.Attachments.Length;
            var itemInd = 0;
            string invokeArgs = string.Empty;

            if (itemsCount > 1)
            {
                UIAlertController alert = UIAlertController.Create("Steepshot", "You can post only one photo for the moment", UIAlertControllerStyle.Alert);
                PresentViewController(alert, true, () =>
                {
                    DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, 5000000000), () =>
                    {
                        ExtensionContext.CompleteRequest(null, null);
                    });
                });
                base.DidSelectPost();
            }
            else
            {
                foreach (NSItemProvider itemProvider in firstItem.Attachments)
                {
                    if (itemProvider.HasItemConformingTo(UTType.Image))
                    {
                        itemProvider.LoadItemAsync(UTType.Image, null).ContinueWith((item) =>
                        {
                            var imageData = NSData.FromUrl((NSUrl)item.Result);
                            var image = UIImage.LoadFromData(imageData);
                            var path = SavePhotosToSharedStorage(image, itemInd);
                            invokeArgs += string.IsNullOrEmpty(invokeArgs) ? path : $"%{path}";
                            if (++itemInd >= itemsCount)
                            {
                                InvokeOnMainThread(() =>
                                                   InvokeApp(invokeArgs));
                            }
                        });
                    }
                }
            }
        }

        private void InvokeApp(string invokeArgs)
        {
            string className = "UIApplication";
            if (Class.GetHandle(className) != IntPtr.Zero)
            {
                NSUrl request = new NSUrl("steepshot://" + invokeArgs);
                UIApplication.SharedApplication.OpenUrl(request);
            }
            base.DidSelectPost();
        }

        private string SavePhotosToSharedStorage(UIImage photo, int imageIndex)
        {
            var nsFileManager = new NSFileManager();
            var containerUrl = nsFileManager.GetContainerUrl(APP_SHARE_GROUP);
            var fileName = string.Format("image{0}.jpg", imageIndex);
            var filePath = Path.Combine(containerUrl.Path, fileName);
            photo.AsJPEG(1).Save(filePath, true);
            return filePath;
        }
    }
}
