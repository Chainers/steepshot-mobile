using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;

namespace Steepshot.Utils
{
    public sealed class NewTextEdit : AppCompatEditText
    {
        public event Action KeyboardDownEvent;
        public event Action OkKeyEvent;

        public NewTextEdit(Context context) : base(context)
        {
            
        }

        public NewTextEdit(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            
        }

        public NewTextEdit(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            
        }

        public override void OnEditorAction(Android.Views.InputMethods.ImeAction actionCode)
        {
            if (actionCode == Android.Views.InputMethods.ImeAction.Done)
            {
                OkKeyEvent?.Invoke();
            }
            base.OnEditorAction(actionCode);
        }

        public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                KeyboardDownEvent?.Invoke();
            }
            return base.OnKeyPreIme(keyCode, e);
        }
    }
}
