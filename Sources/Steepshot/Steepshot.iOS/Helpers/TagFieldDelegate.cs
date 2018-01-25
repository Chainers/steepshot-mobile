using System;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public class TagFieldDelegate : UITextFieldDelegate
    {
        private Action _doneTapped;

        public TagFieldDelegate(Action doneTapped)
        {
            _doneTapped = doneTapped;
        }

        public override bool ShouldReturn(UITextField textField)
        {
            _doneTapped?.Invoke();
            return true;
        }
    }
}
