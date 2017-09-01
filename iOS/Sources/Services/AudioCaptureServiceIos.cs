using System;
using XamTuner.iOS.Sources.Services;
using XamTuner.Sources.Services;
using XLabs.Platform.Services.Media;

[assembly: Xamarin.Forms.Dependency(typeof(AudioCaptureServiceIos))]

namespace XamTuner.iOS.Sources.Services {
    public class AudioCaptureServiceIos : IAudioCaptureService {

        public AudioCaptureServiceIos() {
            SampleBufferSize = SampleRate * SampleBufferMs / 1000;
            AudioStream = new Microphone(SampleBufferSize);
        }

        public IAudioStream AudioStream { get; private set; }

        public int SampleBufferSize { get; private set; }

        public int SampleRate { get; private set; } = 22050;

        public int SampleBufferMs { get; private set; } = 250;
    }
}
