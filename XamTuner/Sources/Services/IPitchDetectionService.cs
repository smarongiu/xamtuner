using System;
using System.Threading.Tasks;
using XamTuner.Sources.Processing;

namespace XamTuner.Sources.Services {
    public interface IPitchDetectionService {
        event Action<DetectedPitchInfo> PitchDetected;
        bool IsStarted { get; }
        Task Start();
        Task Stop();
    }
}