using System;
using Android.OS;
using CameraTest.VideoRecordEnums;
using Java.Lang;
using Java.Nio;

namespace Steepshot.CameraGL.Encoder
{
    public class AudioEncoderHandler : Handler
    {
        private readonly WeakReference<AudioEncoderWrapper> _weakEncoder;

        public AudioEncoderHandler(AudioEncoderWrapper encoder)
        {
            _weakEncoder = new WeakReference<AudioEncoderWrapper>(encoder);
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
                    encoder.HandleStartRecording((AudioEncoderConfig)obj);
                    break;
                case EncoderMessages.StopRecording:
                    encoder.HandleStopRecording();
                    break;
                case EncoderMessages.FrameAvailable:
                    var timestamp = ((long)inputMessage.Arg1 << 32) |
                                    (inputMessage.Arg2 & 0xffffffffL);
                    encoder.HandleFrameAvailable((byte[])obj, timestamp);
                    break;
                case EncoderMessages.Quit:
                    Looper.MyLooper().Quit();
                    break;
                default:
                    throw new RuntimeException("Unhandled msg what=" + what);
            }
        }
    }
}