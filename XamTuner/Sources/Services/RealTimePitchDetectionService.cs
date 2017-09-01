using System;
using System.Threading.Tasks;
using XamTuner.Sources.Processing;
using XamTuner.Sources.Processing.Tarsos;
using XLabs;

namespace XamTuner.Sources.Services {

	public class RealTimePitchDetectionService : IPitchDetectionService {

		public event Action<DetectedPitchInfo> PitchDetected = delegate { };

		public bool IsStarted { get; private set; }

        readonly IAudioCaptureService _audioCapture;
        float[] _buffer;
		int _sampleBufferSize;
        YinPitchDetector _yin;
        bool _busy;

        public RealTimePitchDetectionService(IAudioCaptureService audioCapture) {
            _audioCapture = audioCapture;
            _yin = new YinPitchDetector(_audioCapture.SampleRate, _audioCapture.SampleBufferSize);
		}

		void InitBuffers() {
			_buffer = new float[_audioCapture.SampleBufferSize];
			_sampleBufferSize = (int)Math.Ceiling((_buffer.Length + 1) / 2.0);
		}

		public async Task Start() {
			if(!IsStarted) {
				InitBuffers();
                _audioCapture.AudioStream.OnBroadcast += OnNewFragment;
				IsStarted = await _audioCapture.AudioStream.Start(_audioCapture.SampleRate);
			}
		}

		public async Task Stop() {
			if(IsStarted) {
				await _audioCapture.AudioStream.Stop();
				IsStarted = false;
				_audioCapture.AudioStream.OnBroadcast -= OnNewFragment;
			}
		}


		void OnNewFragment(object sender, EventArgs<byte[]> e) {
            if (_busy) return;
            _busy = true;

			var samples = e.Value;
            Convert16BitsSamplesToFloat(samples, _buffer);

            var result = _yin.GetPitch(_buffer);

            if (result != null && result.IsPitched && result.Probability > 0.9) {
                double err;
                var note = Note.FromFrequency(result.Pitch, out err);
                PitchDetected(new DetectedPitchInfo(note, err, result));
            }
            _busy = false;
		}


        static void Convert16BitsSamplesToFloat(byte[] samples, float[] buf) {
            for (int i = 0; i < samples.Length; i += 2) {
                var v = (short)(samples[i + 1] << 8 | samples[i]);
                buf[i / 2] = (float)((double)v / 32768);
            }
            for (int i = samples.Length; i < buf.Length; i += 2) {
                buf[i / 2] = 0;
            }
        }

    }
}
