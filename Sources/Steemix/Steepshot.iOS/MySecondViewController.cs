using System;

using UIKit;

namespace Steepshot.iOS
{
    public partial class MySecondViewController : UIViewController
    {
        protected MySecondViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            //var lol = this.NavigationController.ViewControllers;
            //this.NavigationController.ViewControllers = new UIViewController[] { lol[0], lol[2] };
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

