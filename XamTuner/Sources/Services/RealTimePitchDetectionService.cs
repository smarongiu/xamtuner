using System;
using System.Threading.Tasks;
using MathNet.Numerics;
using XamTuner.Sources.Processing;
using XamTuner.Sources.Processing.Tarsos;
using XLabs;

namespace XamTuner.Services {

	public class RealTimePitchDetectionService {

		public event Action<DetectedPitchInfo> PitchDetected = delegate { };
		public event Action<double[]> PowerSpectrumAvailable = delegate { };
		public event Action<double[]> HPSAvailable = delegate { };
		public event Action<double[]> PeaksFound = delegate { };

		public bool IsStarted { get; private set; }
		public int SampleRate { get; private set; } = AudioInputService.DefaultSampleRate;

        float[] _buffer;
		int _sampleBufferSize;
        YinPitchDetector _yin;

		public RealTimePitchDetectionService() {
            _yin = new YinPitchDetector(AudioInputService.DefaultSampleRate, AudioInputService.SampleBufferSize);
		}

		void InitBuffers() {
			_buffer = new float[AudioInputService.SampleBufferSize];
			_sampleBufferSize = (int)Math.Ceiling((_buffer.Length + 1) / 2.0);
		}

		public async Task Start() {
			if(!IsStarted) {
				InitBuffers();
				AudioInputService.AudioStream.OnBroadcast += OnNewFragment;
				IsStarted = await AudioInputService.AudioStream.Start(SampleRate);
			}
		}

		public async Task Stop() {
			if(IsStarted) {
				await AudioInputService.AudioStream.Stop();
				IsStarted = false;
				AudioInputService.AudioStream.OnBroadcast -= OnNewFragment;
			}
		}

		public double[] IndicesToFrequencies(int[] indices) {
			double[] fqs = new double[indices.Length];
			for(int i = 0; i < indices.Length; i++) {
				fqs[i] = IndexToFrequency(i);
			}
			return fqs;
		}

		double IndexToFrequency(int index) => (double)index / _sampleBufferSize * SampleRate / 2;

		int FrequencyToIndex(double freq) => (int)(freq * _sampleBufferSize / SampleRate * 2);


		void OnNewFragment(object sender, EventArgs<byte[]> e) {
			var samples = e.Value;
            Convert16BitsSamplesToFloat(samples, _buffer);

            var result = _yin.GetPitch(_buffer);

            System.Diagnostics.Debug.WriteLine(result);
		}



        public static void Convert16BitsSamplesToFloat(byte[] samples, float[] buf) {
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
