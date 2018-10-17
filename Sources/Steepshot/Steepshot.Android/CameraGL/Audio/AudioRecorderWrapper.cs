using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Java.Lang;
using AudioEncoder = Steepshot.CameraGL.Encoder.AudioEncoder;
using ThreadPriority = Android.OS.ThreadPriority;

namespace Steepshot.CameraGL.Audio
{
    public class AudioRecorderWrapper
    {
        private AudioRecorderConfig Config { get; set; }
        private AudioRecord _audioRecord;
        private AudioEncoder _audioEncoder;
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
            _samplesPerFrame = Config.SamplesPerFrame;
            var bufferSize = AudioRecord.GetMinBufferSize(Config.SampleRate, Config.ChanelConfig, Config.AudioFormat);
            if (bufferSize < 0)
            {
                bufferSize = Config.SampleRate * 2;
            }

            _buffer = new byte[_samplesPerFrame];
            _audioRecord = new AudioRecord(AudioSource.Camcorder, Config.SampleRate, Config.ChanelConfig, Config.AudioFormat, bufferSize);

            _audioEncoder = new AudioEncoder();
            _audioEncoder.Configure(Config.AudioEncoderConfig);
            _audioEncoder.Start();

            if (_audioRecord.State != State.Initialized)
                return;

            _audioRecord.StartRecording();

            await Task.Run(() =>
            {
                Android.OS.Process.SetThreadPriority(ThreadPriority.UrgentAudio);

                while (!ct.IsCancellationRequested)
                {
                    _audioRecord.Read(_buffer, 0, _samplesPerFrame);
                    _audioEncoder.Poll(_buffer, JavaSystem.NanoTime() / 1000L);
                    _audioEncoder.DrainEncoder(false);
                }
            }, ct);

            StopRecording();
        }

        private void StopRecording()
        {
            _audioEncoder.Stop(true);
            _audioRecord.Stop();
            _audioRecord.Release();
            _audioRecord = null;
        }
    }
}