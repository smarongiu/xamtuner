using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XamTuner.Sources.Services {
    public interface IAudioStream {
        event EventHandler<DataBufferEventArgs> DataBufferReached;

        int SampleRate { get; }
        int ChannelCount { get; }
        int BitsPerSample { get; }
        IEnumerable<int> SupportedSampleRates { get; }
        bool IsActive { get; }

        Task<bool> Start(int sampleRate);
        Task Stop();
    }
}
