using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;

namespace Steepshot.Utils
{
    public class NewTextEdit : AppCompatEditText
    {
        public event Action KeyboardDownEvent;

        public NewTextEdit(Context context) : base(context)
        {
            
        }

        public NewTextEdit(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            
        }

        public NewTextEdit(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            
        }

        public override bool OnKeyPreIme(Keycode keyCode, Android.Views.KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                KeyboardDownEvent?.Invoke();
            }
            return base.OnKeyPreIme(keyCode, e);
        }
    }
}
