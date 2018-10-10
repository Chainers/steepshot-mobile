using System;
using Android.OS;
using Java.Lang;
using Steepshot.CameraGL.Enums;

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
                    Looper.MyLooper().Quit();
                    break;
                case EncoderMessages.FrameAvailable:
                    encoder.HandleFrameAvailable();
                    break;
                case EncoderMessages.Poll:
                    var timestamp = ((long)inputMessage.Arg1 << 32) |
                                    (inputMessage.Arg2 & 0xffffffffL);
                    encoder.HandlePoll((byte[])obj, timestamp);
                    break;
                default:
                    throw new RuntimeException("Unhandled msg what=" + what);
            }
        }
    }
}