using System;
using Android.Graphics;
using Android.Media;
using Android.Opengl;
using Android.Views;
using Steepshot.CameraGL.Encoder;
using Steepshot.CameraGL.Gles;

namespace Steepshot.CameraGL
{
    public class VideoEditor
    {
        private readonly MediaExtractor _extractor;
        private readonly MuxerWrapper _muxer;
        private EglCore _eglCore;
        private Texture2DProgram _texture2DProgram;
        private FullFrameRect _fullFrame;
        private WindowSurface _windowSurface;
        private MediaCodec _decoder;
        private VideoEditorEncoder _encoder;

        public VideoEditor()
        {
            _extractor = new MediaExtractor();
            _extractor.SetDataSource("/storage/emulated/0/DCIM/Camera/20181031_121127.mp4");
            _muxer = new MuxerWrapper();
            _muxer.Reset($"/storage/emulated/0/DCIM/{Guid.NewGuid()}.mp4", MuxerOutputType.Mpeg4);
        }

        public void PerformEdit()
        {
            string mimeType = null;
            MediaFormat format = null;
            for (int i = 0; i < _extractor.TrackCount; i++)
            {
                var iformat = _extractor.GetTrackFormat(i);
                mimeType = iformat.GetString(MediaFormat.KeyMime);
                if (mimeType.Contains("video"))
                {
                    format = iformat;
                    _extractor.SelectTrack(i);
                    break;
                }
            }

            if (format == null)
                return;

            _encoder = new VideoEditorEncoder();
            var config = new VideoEncoderConfig(_muxer, 720, 720, mimeType, 30, 2, 3000000, 20);
            _encoder.Configure(config);
            _encoder.Start();

            _eglCore = new EglCore(null, EglCore.FlagRecordable);
            _windowSurface = new WindowSurface(_eglCore, _encoder.InputSurface, true);
            _windowSurface.MakeCurrent();

            _texture2DProgram = new Texture2DProgram();
            _fullFrame = new FullFrameRect(_texture2DProgram);
            var textureId = _fullFrame.CreateTextureObject();
            var texture = new SurfaceTexture(textureId);
            var outputSurface = new Surface(texture);

            _decoder = MediaCodec.CreateDecoderByType(mimeType);
            _decoder.Configure(format, outputSurface, null, 0);
            _decoder.Start();

            var outputDone = false;
            var inputDone = false;
            var info = new MediaCodec.BufferInfo();
            var transform = new float[16];

            while (!outputDone)
            {
                if (!inputDone)
                {
                    var inputBufIndex = _decoder.DequeueInputBuffer(0);
                    if (inputBufIndex >= 0)
                    {
                        var inputBuf = _decoder.GetInputBuffer(inputBufIndex);
                        var chunkSize = _extractor.ReadSampleData(inputBuf, 0);
                        if (chunkSize < 0)
                        {
                            _decoder.QueueInputBuffer(inputBufIndex, 0, 0, 0L, MediaCodecBufferFlags.EndOfStream);
                            inputDone = true;
                        }
                        else
                        {
                            var presentationTimeUs = _extractor.SampleTime;
                            _decoder.QueueInputBuffer(inputBufIndex, 0, chunkSize, presentationTimeUs, 0);
                            _extractor.Advance();
                        }
                    }
                }

                var decoderStatus = _decoder.DequeueOutputBuffer(info, 0);
                var endOfStream = (info.Flags & MediaCodecBufferFlags.EndOfStream) != 0;
                if (endOfStream)
                {
                    outputDone = true;
                }
                if (decoderStatus > 0)
                {
                    var doRender = info.Size != 0;

                    if (doRender)
                    {
                        texture.UpdateTexImage();
                        texture.GetTransformMatrix(transform);
                        _texture2DProgram.AspectRatio = 1920 / 1080f;
                        _fullFrame.DrawFrame(textureId, transform);
                        _encoder.DrainEncoder(endOfStream);
                        _windowSurface.SetPresentationTime(info.PresentationTimeUs * 1000);
                        _windowSurface.SwapBuffers();
                    }
                    _decoder.ReleaseOutputBuffer(decoderStatus, doRender);
                }
            }
            _encoder.Stop(true);
        }
    }
}