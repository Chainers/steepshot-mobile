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

        public override bool ShouldChangeCharacters(UITextField textField, Foundation.NSRange range, string replacementString)
        {
            if (replacementString == "-" && textField.Text.Length > 0)
                return true;
            if (replacementString == " "  && textField.Text.Length == 0)
                return false;
            if (!string.IsNullOrEmpty(replacementString) && Char.IsDigit(Char.Parse(replacementString)) && textField.Text.Length == 0)
                return false;
            if (!string.IsNullOrEmpty(replacementString) && !(replacementString == " " || Char.IsLetterOrDigit(Char.Parse(replacementString))))
                return false;
            if ((replacementString + textField.Text).Length > 40)
                return false;
            return true;
        }
    }
}
