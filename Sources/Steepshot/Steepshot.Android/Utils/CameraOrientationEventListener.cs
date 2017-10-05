using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;

namespace Steepshot.Utils
{
    public class CameraOrientationEventListener : OrientationEventListener
    {
        public int Orientation;
        //array for normal orientation handling(not camera)
        //private static readonly int[] lookup = new [] { 0, 0, 0, 90, 90, 90, 90, 90, 90, 180, 180, 180, 180, 180, 180, 270, 270, 270, 270, 270, 270, 0, 0, 0 }; // 15-degree increments 
        private static readonly int[] lookup = new[] { 90, 90, 90, 180, 180, 180, 180, 180, 180, 270, 270, 270, 270, 270, 270, 0, 0, 0, 0, 0, 0, 90, 90, 90 };

        public CameraOrientationEventListener(Context context, [GeneratedEnum] SensorDelay rate) : base(context, rate)
        {
        }

        public override void OnOrientationChanged(int orientation)
        {
            if (orientation != OrientationUnknown)
            {
                var newOrientation =  lookup[orientation / 15];
                if (Orientation != newOrientation)
                {
                    Orientation = newOrientation;
                }
            }
        }
    }
}
