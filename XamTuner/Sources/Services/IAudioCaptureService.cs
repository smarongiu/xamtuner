using System;
using XLabs.Platform.Services.Media;

namespace XamTuner.Sources.Services {
    public interface IAudioCaptureService {
        IAudioStream AudioStream { get; }
        int SampleBufferSize { get; }
        int SampleRate { get; }
    }
}
