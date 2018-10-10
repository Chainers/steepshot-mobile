using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Java.Lang;
using Steepshot.CameraGL.Encoder;

namespace Steepshot.CameraGL.Audio
{
    public class AudioRecorderWrapper
    {
        private AudioRecorderConfig Config { get; set; }
        private AudioRecord _audioRecord;
        private AudioEncoderWrapper _encoderWrapper;
        private CancellationTokenSource _cts;
        private byte[] _buffer;
        private int _samplesPerFrame;
        private bool _isRecording;

        public void Configure(AudioRecorderConfig config)
        {
            Config = config;
        }

        public void ChangeRecordingState(bool record)
        {
            if (_isRecording == record)
                return;

            _isRecording = record;
            if (_isRecording)
            {
                _cts = new CancellationTokenSource();
                StartRecording(_cts.Token);
            }
            else
            {
                _cts.Cancel();
            }
        }

        private async void StartRecording(CancellationToken ct)
        {
            _encoderWrapper = Config.AudioEncoderWrapper;
            _samplesPerFrame = Config.SamplesPerFrame;
            var bufferSize = AudioRecord.GetMinBufferSize(Config.SampleRate, Config.ChanelConfig, Config.AudioFormat);
            if (bufferSize < 0)
            {
                bufferSize = Config.SampleRate * 2;
            }

            _buffer = new byte[_samplesPerFrame];
            _audioRecord = new AudioRecord(AudioSource.Mic, Config.SampleRate, ChannelIn.Mono, Config.AudioFormat, bufferSize);

            await Task.Run(() =>
            {
                _encoderWrapper.StartRecording();

                if (_audioRecord.State != State.Initialized)
                    return;

                _audioRecord.StartRecording();

                while (!ct.IsCancellationRequested)
                {
                    _audioRecord.Read(_buffer, 0, _samplesPerFrame);
                    _encoderWrapper.Poll(_buffer, JavaSystem.NanoTime() / 1000L);
                }
            }, ct);

            StopRecording();
        }

        private void StopRecording()
        {
            _encoderWrapper.StopRecording();
            _audioRecord.Stop();
            _audioRecord.Release();
            _audioRecord = null;
        }
    }
}