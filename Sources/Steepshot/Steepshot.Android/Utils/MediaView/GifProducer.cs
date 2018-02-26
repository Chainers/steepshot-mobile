using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Utils.GifDecoder;

namespace Steepshot.Utils.MediaView
{
    public class GifProducer : IMediaProducer
    {
        private readonly IMediaPerformer _mediaPerformer;
        private GifDecoder.GifDecoder _gifDecoder;
        private CancellationTokenSource _gifAnimationCancellationTokenSource;

        public GifProducer(IMediaPerformer mediaPerformer)
        {
            _mediaPerformer = mediaPerformer;
        }

        public async void Init(MediaModel media)
        {
            _gifAnimationCancellationTokenSource?.Cancel();
            var bytes = await SimpleGifLoader.LoadAsync(media.Url);
            _gifDecoder = new GifDecoder.GifDecoder();
            try
            {
                _gifDecoder.Read(bytes);
                _gifDecoder.SetFrameIndex(0);
                _gifAnimationCancellationTokenSource = new CancellationTokenSource();
                Prepare();
            }
            catch
            {
                _gifDecoder = null;
                return;
            }
        }

        public void Pause()
        {
        }

        public void Play()
        {
        }

        public void Prepare()
        {
            if (_gifAnimationCancellationTokenSource == null) return;
            Task.Run(async () =>
                {
                    if (_gifDecoder == null) return;
                    int frameIndex = 0;
                    while (true)
                    {
                        frameIndex = frameIndex >= _gifDecoder.FrameCount ? 0 : frameIndex;
                        _gifDecoder.SetFrameIndex(frameIndex);
                        frameIndex++;
                        //milliseconds spent on frame decode
                        double frameDecodeTime = 0;
                        try
                        {
                            var before = DateTime.Now;
                            await _mediaPerformer.PrepareBufferAsync(_gifDecoder.GetNextFrame());
                            frameDecodeTime = (DateTime.Now - before).TotalMilliseconds;
                            _mediaPerformer.DrawBuffer();
                        }
                        catch
                        {
                            // suppress exception
                        }
                        try
                        {
                            int delay = _gifDecoder.GetNextDelay();
                            delay -= (int)frameDecodeTime;
                            if (delay > 0)
                            {
                                await Task.Delay(delay);
                            }
                        }
                        catch
                        {
                            // suppress exception
                        }
                    }
                }, _gifAnimationCancellationTokenSource.Token);
        }

        public void Release()
        {
            _gifAnimationCancellationTokenSource?.Cancel();
            _gifDecoder?.Clear();
            _gifDecoder = null;
        }
    }
}