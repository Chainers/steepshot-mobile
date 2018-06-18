using System;
using System.IO;
using System.Linq;
using Foundation;
using MobileCoreServices;
using Social;
using UIKit;

namespace Steepshot
{
    public partial class ShareViewController : SLComposeServiceViewController
    {
        private const string APP_SHARE_GROUP = "group.com.chainartsoft.steepshot.post";

        protected ShareViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            var attachments = ExtensionContext.InputItems.First().Attachments;
            foreach (var attachment in attachments)
            {
                if (attachment.HasItemConformingTo(UTType.Image))
                {
                    attachment.LoadItem(UTType.Image, null, (NSObject arg1, NSError arg2) =>
                    {
                        if (arg1.GetType() == typeof(NSUrl))
                        {
                            var imageData = NSData.FromUrl((NSUrl)arg1);
                            var path = SavePhotosToSharedStorage(imageData, 0);
                            InvokeOnMainThread(() => InvokeApp(path));
                        }
                        else if (arg1.GetType() == typeof(UIImage))
                        {
                            var path = SavePhotosToSharedStorage(((UIImage)arg1).AsJPEG(), 0);
                            InvokeOnMainThread(() => InvokeApp(path));
                        }
                    });
                }
            }
        }

        public override bool IsContentValid()
        {
            return true;
        }

        public override SLComposeSheetConfigurationItem[] GetConfigurationItems()
        {
            return new SLComposeSheetConfigurationItem[0];
        }

        private void InvokeApp(string invokeArgs)
        {
            NSUrl request = new NSUrl("steepshot://" + invokeArgs);
            UIApplication.SharedApplication.OpenUrl(request);
            ExtensionContext.CompleteRequest(new NSExtensionItem[0], null);
        }

        private string SavePhotosToSharedStorage(NSData photo, int imageIndex)
        {
            var nsFileManager = new NSFileManager();
            var containerUrl = nsFileManager.GetContainerUrl(APP_SHARE_GROUP);
            var fileName = string.Format("image{0}.jpg", imageIndex);
            var filePath = Path.Combine(containerUrl.Path, fileName);
            photo.Save(filePath, false);
            return filePath;
        }
    }
}
