using XamTuner.Droid.Sources.Services;
using XamTuner.Sources.Services;

[assembly: Xamarin.Forms.Dependency(typeof(AudioCaptureServiceAndroid))]

namespace XamTuner.Droid.Sources.Services {
    public class AudioCaptureServiceAndroid : IAudioCaptureService {

        public AudioCaptureServiceAndroid() {
            SampleBufferSize = SampleRate * SampleBufferMs / 1000;
            AudioStream = new Microphone(SampleBufferSize);
        }

        public IAudioStream AudioStream { get; private set; }

        public int SampleBufferSize { get; private set; }

        public int SampleRate { get; private set; } = 22050;

        public int SampleBufferMs { get; private set; } = 250;
    }
}
