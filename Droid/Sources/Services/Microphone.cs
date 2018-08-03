using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Media;
using XamTuner.Sources.Services;

namespace XamTuner.Droid.Sources.Services {
    public class Microphone : IAudioStream {
        AudioRecord _audioSource;
        int _bufferSize;

        public Microphone(int bufferSize = -1) {
            SupportedSampleRates = (new[] { 8000, 11025, 16000, 22050, 44100 })
                .Where(rate => AudioRecord.GetMinBufferSize(rate, ChannelIn.Mono, Encoding.Pcm16bit) > 0)
                .ToList();
            _bufferSize = bufferSize;
        }

        public static bool IsEnabled {
            get {
                return Application.Context.PackageManager.HasSystemFeature(PackageManager.FeatureMicrophone)
                       && Application.Context.PackageManager.CheckPermission(
                           "android.permission.RECORD_AUDIO",
                           Application.Context.PackageName) == Permission.Granted;
            }
        }

        public int SampleRate => _audioSource == null ? -1 : _audioSource.SampleRate;

        public int BitsPerSample => _audioSource == null ? -1 : (_audioSource.AudioFormat == Encoding.Pcm16bit) ? 16 : 8;

        public int ChannelCount => _audioSource == null ? -1 : _audioSource.ChannelCount;

        public int AverageBytesPerSecond => _audioSource == null ? -1 : SampleRate * BitsPerSample / 8 * ChannelCount;

        public bool IsActive => (_audioSource != null && _audioSource.RecordingState == RecordState.Recording);

        public IEnumerable<int> SupportedSampleRates { get; private set; }

        public event EventHandler<DataBufferEventArgs> DataBufferReached;

        public Task<bool> Start(int sampleRate) {
            return Task.Run(
                () => {
                    if (!SupportedSampleRates.Contains(sampleRate)) {
                        return false;
                    }

                    var minBufSize = AudioRecord.GetMinBufferSize(sampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
                    if (_bufferSize < minBufSize) _bufferSize = minBufSize;

                    _audioSource = new AudioRecord(AudioSource.Mic, sampleRate, ChannelIn.Mono, Encoding.Pcm16bit, _bufferSize);

                    StartRecording();

                    return true;
                });
        }

        public Task Stop() {
            return Task.Run(
                () => {
                    _audioSource.Stop();
                    _audioSource = null;
                });
        }

        void StartRecording() {
            _audioSource.StartRecording();

            Task.Run(
                async () => {
                    do {
                        await Record();
                    }
                    while (IsActive);
                });
        }

        async Task Record() {
            var buffer = new byte[_bufferSize];

            var readCount = await _audioSource.ReadAsync(buffer, 0, _bufferSize);

            DataBufferReached?.Invoke(this, new DataBufferEventArgs(buffer));
        }
    }
}