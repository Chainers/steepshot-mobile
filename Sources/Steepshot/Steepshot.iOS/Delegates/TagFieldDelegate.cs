using System;
using System.Text.RegularExpressions;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Delegates
{
    public class BaseTextFieldDelegate : UITextFieldDelegate
    {
        public Action DoneTapped;

        public override bool ShouldReturn(UITextField textField)
        {
            DoneTapped?.Invoke();
            return true;
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

    public class UsernameDelegate : BaseTextFieldDelegate
    {
        public override bool ShouldChangeCharacters(UITextField textField, Foundation.NSRange range, string replacementString)
        {
            if (replacementString == string.Empty)
                return true;
            if ((textField.Text + replacementString).Length == 17)
                return false;
            if (Regex.IsMatch(replacementString, @"[_\w\.-]+"))
                return true;
            return false;
        }
    }

    public class AmountFieldDelegate : BaseTextFieldDelegate
    {
        public override bool ShouldChangeCharacters(UITextField textField, Foundation.NSRange range, string replacementString)
        {
            if (Regex.IsMatch(textField.Text + replacementString, @"^\d+(,|\.)?\d{0,3}?$"))
                return true;
            return false;
        }
    }

    public class TagFieldDelegate : BaseTextFieldDelegate
    {
        public override bool ShouldChangeCharacters(UITextField textField, Foundation.NSRange range, string replacementString)
        {
            if (replacementString == "")
                return true;

            if ((replacementString + textField.Text).Length > 40)
                return false;

            if (replacementString.Contains(" "))
                return false;
            
            if (Regex.IsMatch(replacementString, @"[_\w\.-]+"))
                return true;
            return false;
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
    }
}
