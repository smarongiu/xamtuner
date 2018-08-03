using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AudioToolbox;
using XamTuner.Sources.Services;

namespace XamTuner.iOS.Sources.Services {
    public class Microphone : IAudioStream {
        private InputAudioQueue _audioQueue;
        private readonly int _bufferSize;

        public Microphone(int bufferSize = 4098) {
            _bufferSize = bufferSize;
        }

        void StartRecording(int rate) {
            if (IsActive) {
                Clear();
            }

            SampleRate = rate;

            var audioFormat = new AudioStreamBasicDescription {
                SampleRate = SampleRate,
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.LinearPCMIsSignedInteger | AudioFormatFlags.LinearPCMIsPacked,
                FramesPerPacket = 1,
                ChannelsPerFrame = 1,
                BitsPerChannel = BitsPerSample,
                BytesPerPacket = 2,
                BytesPerFrame = 2,
                Reserved = 0
            };

            _audioQueue = new InputAudioQueue(audioFormat);
            _audioQueue.InputCompleted += QueueInputCompleted;

            var bufferByteSize = _bufferSize * audioFormat.BytesPerPacket;

            IntPtr bufferPtr;
            for (var index = 0; index < 3; index++) {
                _audioQueue.AllocateBufferWithPacketDescriptors(bufferByteSize, _bufferSize, out bufferPtr);
                _audioQueue.EnqueueBuffer(bufferPtr, bufferByteSize, null);
            }

            _audioQueue.Start();
        }

        void Clear() {
            if (_audioQueue != null) {
                _audioQueue.Stop(true);
                _audioQueue.InputCompleted -= QueueInputCompleted;
                _audioQueue.Dispose();
                _audioQueue = null;
            }
        }

        void QueueInputCompleted(object sender, InputCompletedEventArgs e) {
            // return if we aren't actively monitoring audio packets
            if (!IsActive) {
                return;
            }

            var buffer = (AudioQueueBuffer)Marshal.PtrToStructure(e.IntPtrBuffer, typeof(AudioQueueBuffer));
            if (DataBufferReached != null) {
                var data = new byte[buffer.AudioDataByteSize];
                Marshal.Copy(buffer.AudioData, data, 0, (int)buffer.AudioDataByteSize);

                DataBufferReached(this, new DataBufferEventArgs(data));
            }

            var status = _audioQueue.EnqueueBuffer(e.IntPtrBuffer, _bufferSize, e.PacketDescriptions);

            if (status != AudioQueueStatus.Ok) {
                // todo: 
            }
        }

        #region IAudioStream implementation

        public event EventHandler<DataBufferEventArgs> DataBufferReached;

        public int SampleRate { get; private set; }

        public int ChannelCount => 1;

        public int BitsPerSample => 16;

        public bool IsActive => _audioQueue != null && _audioQueue.IsRunning;

        public IEnumerable<int> SupportedSampleRates { get; private set; } = new[] { 8000, 16000, 22050, 41000, 44100 };

        public Task<bool> Start(int sampleRate) {
            return Task.Run(
                () => {
                    if (!SupportedSampleRates.Contains(sampleRate)) {
                        return false;
                    }

                    StartRecording(sampleRate);

                    return IsActive;
                });
        }

        public Task Stop() {
            return Task.Run(() => Clear());
        }

        #endregion
    }
}