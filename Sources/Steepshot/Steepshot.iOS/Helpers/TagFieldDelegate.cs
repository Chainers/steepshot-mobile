using System;
using Foundation;
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
            if (replacementString == " " && textField.Text.Length == 0)
                return false;
            if (replacementString.Length > 1)
                return false;
            if (!string.IsNullOrEmpty(replacementString) && Char.IsDigit(Char.Parse(replacementString)) && textField.Text.Length == 0)
                return false;
            if (!string.IsNullOrEmpty(replacementString) && !(replacementString == " " || Char.IsLetterOrDigit(Char.Parse(replacementString))))
                return false;
            if ((replacementString + textField.Text).Length > 40)
                return false;
            return true;
        }

        public override void EditingStarted(UITextField textField)
        {
            textField.Layer.BorderWidth = 1;
            textField.Layer.BorderColor = Constants.R255G71B5.CGColor;
            textField.BackgroundColor = UIColor.White;
        }

        public override void EditingEnded(UITextField textField)
        {
            ChangeBackground(textField);
        }

        public void ChangeBackground(UITextField textField)
        {
            if (textField.IsFirstResponder || textField.Text.Length > 0)
            {
                textField.Layer.BorderColor = Constants.R244G244B246.CGColor;
            }
            else
            {
                textField.Layer.BorderWidth = 0;
                textField.BackgroundColor = Constants.R245G245B245;
            }
        }
    }
}
