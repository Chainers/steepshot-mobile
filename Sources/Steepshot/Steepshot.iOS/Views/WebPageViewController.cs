using System;
using Foundation;
using Steepshot.Core;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class WebPageViewController : UIViewController
    {
        public WebPageViewController()
        {
        }

        protected WebPageViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            webView.ScalesPageToFit = true;
            webView.LoadRequest(new NSUrlRequest(new NSUrl(Constants.SteemitRegUrl)));
            /*
			webView.LoadStarted += (sender, e) =>
			{
				var url = webView.Request.Url.AbsoluteUrl;
			};*/
            //https://steemit.com/enter_email?account=grisha
            //https://steemit.com/enter_mobile
            //https://steemit.com/enter_mobile?phone=296955069&country=375
            //https://steemit.com/submit_mobil
            //{https://steemit.com/approval}
        }
    }
}
