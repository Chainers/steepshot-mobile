using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Media;
using Java.Lang;
using Java.Nio;
using Steepshot.CameraGL.Encoder;
using File = Java.IO.File;

namespace Steepshot.CameraGL
{
    public class MuxerWrapper : IDisposable
    {
        public string Path { get; }
        public bool IsMuxerStarted { get; private set; }
        private MediaMuxer Muxer { get; set; }
        private readonly List<BaseMediaEncoder> _encoders;

        public MuxerWrapper(string path, MuxerOutputType outputType)
        {
            var fs = new FileStream(path, FileMode.CreateNew);
            var file = new File(fs.Name);
            Path = fs.Name;
            Muxer = new MediaMuxer(file.ToString(), outputType);
            _encoders = new List<BaseMediaEncoder>();
        }

        public void WriteSampleData(int trackIndex, ByteBuffer buffer, MediaCodec.BufferInfo bufferInfo)
        {
            lock (Muxer)
            {
                Muxer.WriteSampleData(trackIndex, buffer, bufferInfo);
            }
        }

        public int AddTrack(MediaFormat format)
        {
            lock (Muxer)
            {
                return Muxer.AddTrack(format);
            }
        }

        public void RegisterEncoder(BaseMediaEncoder encoder)
        {
            lock (_encoders)
            {
                if (_encoders.Any(x => x.Type == encoder.Type))
                    throw new IllegalArgumentException("You haver already registered encoder with the same type.");

                _encoders.Add(encoder);
                if (_encoders.Any(x => x.Type == EncoderType.Video) && _encoders.Any(x => x.Type == EncoderType.Audio))
                {
                    Muxer.Start();
                    IsMuxerStarted = true;
                }
            }
        }

        public void UnRegisterEncoder(BaseMediaEncoder encoder)
        {
            lock (_encoders)
            {
                _encoders.Remove(encoder);
                if (_encoders.Count == 0)
                    Release();
            }
        }

        private void Release()
        {
            if (Muxer != null)
            {
                IsMuxerStarted = false;
                Muxer.Stop();
                Muxer.Release();
                Muxer = null;
            }
        }

        public void Dispose()
        {

        }
    }
}