using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Java.Lang;
using Steepshot.Base;
using Steepshot.CameraGL.Encoder;
using Exception = Java.Lang.Exception;
using File = Java.IO.File;

namespace Steepshot.CameraGL
{
    public class MuxerWrapper
    {
        public enum TrackMode
        {
            Video,
            VideoAudio
        }

        public TrackMode Mode { get; set; } = TrackMode.VideoAudio;
        public event Action<string> MuxingFinished;
        private MediaMuxer Muxer { get; set; }
        private string _path;
        private File _outPutFile;
        private MuxerOutputType _muxerOutputType;
        private readonly Dictionary<MuxerTrack, int> _encoders;
        private CancellationTokenSource _muxerCts;

        private event Action WriteOutput;

        public MuxerWrapper()
        {
            _encoders = new Dictionary<MuxerTrack, int>();
            WriteOutput += OnWriteOutput;
        }

        public void Reset(string path, MuxerOutputType outputType)
        {
            Release();
            _path = path;
            _muxerOutputType = outputType;
        }

        public void Interrupt()
        {
            _muxerCts?.Cancel();
        }

        public void AddTrack(MuxerTrack encoder)
        {
            lock (_encoders)
            {
                if (Muxer == null)
                {
                    _outPutFile = new File(_path);
                    Muxer = new MediaMuxer(_outPutFile.ToString(), _muxerOutputType);
                }

                if (_encoders.Any(x => x.Key.Type == encoder.Type))
                    throw new IllegalArgumentException("You already added track of this type");

                if (Mode == TrackMode.Video && encoder.Type != EncoderType.Video)
                    throw new IllegalArgumentException("You can add only video track");


                var trackIndex = Muxer.AddTrack(encoder.OutputFormat);
                _encoders.Add(encoder, trackIndex);

                if (_encoders.Count == (Mode == TrackMode.Video ? 1 : 2))
                {
                    try
                    {
                        _muxerCts = new CancellationTokenSource();
                        WriteOutput?.Invoke();
                    }
                    catch (Exception e)
                    {
                        App.Logger.WarningAsync(e);
                    }
                }
            }
        }

        private void OnWriteOutput()
        {
            Task.Run(() =>
            {
                Muxer.Start();
                try
                {
                    Parallel.ForEach(_encoders.Keys, encoder =>
                    {
                        var index = encoder.CircularBuffer.GetFirstIndex();
                        if (index < 0)
                            return;

                        var info = new MediaCodec.BufferInfo();

                        var lastPts = 0L;
                        do
                        {
                            if (_muxerCts.IsCancellationRequested)
                            {
                                OnErrorOrInterrupt();
                                return;
                            }

                            var buf = encoder.CircularBuffer.GetChunk(index, info);
                            if (info.PresentationTimeUs > +lastPts)
                            {
                                lastPts = info.PresentationTimeUs;
                                Muxer.WriteSampleData(_encoders[encoder], buf, info);
                            }

                            index = encoder.CircularBuffer.GetNextIndex(index);
                        } while (index >= 0);
                    });

                    MuxingFinished?.Invoke(_path);
                }
                catch
                {
                    OnErrorOrInterrupt();
                    MuxingFinished?.Invoke(null);
                }
                finally
                {
                    Muxer.Stop();
                    Release();
                }
            }, _muxerCts.Token).ConfigureAwait(false);
        }

        private void OnErrorOrInterrupt()
        {
            _outPutFile.Delete();
            Release();
        }

        private void Release()
        {
            _encoders.Clear();

            if (Muxer != null)
            {
                Muxer.Release();
                Muxer = null;
            }
        }
    }
}