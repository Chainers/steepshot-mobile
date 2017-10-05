using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;

namespace Steepshot.Utils
{
    public class CameraOrientationEventListener : OrientationEventListener
    {
        public int Orientation;
        public int CameraId;
        // 15-degree increments 
        private static readonly int[] normalLookup = new [] { 0, 0, 0, 90, 90, 90, 90, 90, 90, 180, 180, 180, 180, 180, 180, 270, 270, 270, 270, 270, 270, 0, 0, 0 }; 
        private static readonly int[] backCameraLookup = new[] { 90, 90, 90, 180, 180, 180, 180, 180, 180, 270, 270, 270, 270, 270, 270, 0, 0, 0, 0, 0, 0, 90, 90, 90 };
        private static readonly int[] frontCameralookup = new[] { 270, 270, 270, 0, 0, 0, 0, 0, 0, 90, 90, 90, 90, 90, 90, 180, 180, 180, 180, 180, 180, 270, 270, 270 }; 

        public CameraOrientationEventListener(Context context, [GeneratedEnum] SensorDelay rate, int cameraId) : base(context, rate)
        {
            CameraId = cameraId;
        }

        public override void OnOrientationChanged(int orientation)
        {
            if (orientation != OrientationUnknown)
            {
                var newOrientation = CameraId == 0 ? backCameraLookup[orientation / 15] : frontCameralookup[orientation / 15];
                if (Orientation != newOrientation)
                {
                    Orientation = newOrientation;
                }
            }
        }
    }
}
