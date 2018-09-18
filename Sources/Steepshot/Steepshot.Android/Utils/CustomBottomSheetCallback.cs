using Android.Support.Design.Widget;
using Android.Views;

namespace Steepshot.Utils
{
    public class CustomBottomSheetCallback : BottomSheetBehavior.BottomSheetCallback
    {
        public override void OnSlide(View bottomSheet, float slideOffset)
        {
        }

        public override void OnStateChanged(View bottomSheet, int newState)
        {
            if (newState == BottomSheetBehavior.StateDragging)
            {
                var behavior = BottomSheetBehavior.From(bottomSheet);
                behavior.State = BottomSheetBehavior.StateExpanded;
            }
        }
    }
}