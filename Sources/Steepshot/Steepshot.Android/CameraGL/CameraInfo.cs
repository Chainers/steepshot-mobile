#pragma warning disable 618
using Android.Hardware;

namespace Steepshot.CameraGL
{
    public class CameraInfo
    {
        public int Index { get; set; }
        public Camera.CameraInfo Info { get; set; }
        public string FlashMode { get; set; }
        public int Rotation { get; set; }
    }
}