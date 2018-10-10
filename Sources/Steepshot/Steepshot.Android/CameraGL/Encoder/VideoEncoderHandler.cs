using System;
using Android.OS;
using Java.Lang;
using Steepshot.CameraGL.Enums;

namespace Steepshot.CameraGL.Encoder
{
    public class VideoEncoderHandler : Handler
    {
        private readonly WeakReference<VideoEncoderWrapper> _weakEncoder;

        public VideoEncoderHandler(VideoEncoderWrapper encoderWrapper)
        {
            _weakEncoder = new WeakReference<VideoEncoderWrapper>(encoderWrapper);
        }

        public override void HandleMessage(Message inputMessage)
        {
            var what = (EncoderMessages)inputMessage.What;
            var obj = inputMessage.Obj;

            _weakEncoder.TryGetTarget(out var encoder);
            if (encoder == null)
            {
                return;
            }

            switch (what)
            {
                case EncoderMessages.StartRecording:
                    encoder.HandleStartRecording((VideoEncoderConfig)obj);
                    break;
                case EncoderMessages.StopRecording:
                    encoder.HandleStopRecording();
                    Looper.MyLooper().Quit();
                    break;
                case EncoderMessages.FrameAvailable:
                    var timestamp = ((long)inputMessage.Arg1 << 32) |
                                     (inputMessage.Arg2 & 0xffffffffL);
                    encoder.HandleFrameAvailable((float[])obj, timestamp);
                    break;
                case EncoderMessages.SetTextureId:
                    encoder.HandleSetTexture(inputMessage.Arg1);
                    break;
                case EncoderMessages.UpdateSharedContext:
                    encoder.HandleUpdateSharedContext((VideoEncoderConfig)inputMessage.Obj);
                    break;
                default:
                    throw new RuntimeException("Unhandled msg what=" + what);
            }
        }
    }
}