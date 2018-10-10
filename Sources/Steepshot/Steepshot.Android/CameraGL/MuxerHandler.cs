using System;
using Android.Media;
using Android.OS;
using Java.Lang;
using Steepshot.CameraGL.Enums;

namespace Steepshot.CameraGL
{
    public class MuxerHandler : Handler
    {
        private readonly WeakReference<MuxerWrapper> _weakEncoder;

        public MuxerHandler(MuxerWrapper muxer)
        {
            _weakEncoder = new WeakReference<MuxerWrapper>(muxer);
        }

        public override void HandleMessage(Message inputMessage)
        {
            var what = (MuxerMessages)inputMessage.What;
            var obj = inputMessage.Obj;

            _weakEncoder.TryGetTarget(out var muxer);
            if (muxer == null)
            {
                return;
            }

            switch (what)
            {
                case MuxerMessages.Start:
                    muxer.HandleStart();
                    break;
                case MuxerMessages.Stop:
                    muxer.HandleStop();
                    Looper.MyLooper().Quit();
                    break;
                case MuxerMessages.WriteSampleData:
                    muxer.HandleWriteSampleData(inputMessage.Arg1, (MediaCodec.BufferInfo)obj);
                    break;
                default:
                    throw new RuntimeException("Unhandled msg what=" + what);
            }
        }
    }
}