using System;
using Android.Graphics;
using Android.OS;
using Java.Lang;

namespace Steepshot.CameraGL
{
    public class CameraHandler : Handler
    {
        public const int MsgSetSurfaceTexture = 0;

        private readonly WeakReference<CameraManager> _weakCameraManager;

        public CameraHandler(CameraManager activity)
        {
            _weakCameraManager = new WeakReference<CameraManager>(activity);
        }

        public void InvalidateHandler()
        {
            _weakCameraManager.SetTarget(null);
        }

        public override void HandleMessage(Message inputMessage)
        {
            int what = inputMessage.What;

            _weakCameraManager.TryGetTarget(out var activity);
            if (activity == null)
            {
                return;
            }

            switch (what)
            {
                case MsgSetSurfaceTexture:
                    activity.HandleSetSurfaceTexture((SurfaceTexture)inputMessage.Obj);
                    break;
                default:
                    throw new RuntimeException("unknown msg " + what);
            }
        }
    }
}