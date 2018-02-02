using System;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;

namespace Steepshot.Utils
{
    public sealed class CameraOrientationEventListener : OrientationEventListener
    {
        public event Action<int> OrientationChanged;

        public CameraOrientationEventListener(Context context, [GeneratedEnum] SensorDelay rate) : base(context, rate)
        {
        }

        public override void OnOrientationChanged(int orientation)
        {
            if (orientation != OrientationUnknown)
            {
                OrientationChanged?.Invoke(orientation);
            }
        }
    }
}
