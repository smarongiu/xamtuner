using System;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using XLabs;

namespace XamTuner {
	public class DetectedPitchInfo {
		public readonly double Frequency;
		public readonly double Power;
		public DetectedPitchInfo(double fq, double pw) {
			Frequency = fq;
			Power = pw;
		}
	}

	public class RealTimePitchDetectionService {

        public const int HPS_HARMONICS_COUNT = 4;

		public event Action<DetectedPitchInfo> PitchDetected = delegate {};
		public event Action<double[]> PowerSpectrumAvailable = delegate {};
        public event Action<double[]> HPSAvailable = delegate {};
        public event Action<double[]> PeaksFound = delegate {};

        public double DetectionThreshold { get; set; } = -60;

		public RealTimePitchDetectionService() {
		}

		Complex32[] _fftBuf;
		double[] _powerSpecrumBuf;

        int SampleBufferSize { get; set; }

		void InitBuffers() {
			_fftBuf = new Complex32[AudioInputService.SampleBufferSize];
			SampleBufferSize = (int)Math.Ceiling((_fftBuf.Length + 1) / 2.0);
			_powerSpecrumBuf = new double[SampleBufferSize];
		}

		public bool IsStarted { get; private set; }

		public int SampleRate { get; private set; } = AudioInputService.DefaultSampleRate;

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
            for (int i = 0; i < indices.Length; i++) {
                fqs[i] = (double)i / _powerSpecrumBuf.Length * SampleRate / 2;
            }
            return fqs;
        }


        int FrequencyToIndex(double freq) {
            return (int)(freq * SampleBufferSize / SampleRate * 2);
        }


		void OnNewFragment(object sender, EventArgs<byte[]> e) {
			var samples = e.Value;

			//convert samples to complex and apply a Hann function window
			AudioUtils.Convert16BitsSamplesWindowed(samples, MathNet.Numerics.Window.Hann(_fftBuf.Length), _fftBuf);

			//calculate the fft
			MathNet.Numerics.IntegralTransforms.Fourier.Forward(_fftBuf);

            AudioUtils.GetPowerSpectrum(_fftBuf, _powerSpecrumBuf);
            PowerSpectrumAvailable(_powerSpecrumBuf);

            //get the power spectrum and the HSP
            AudioUtils.ComputeHPS(_powerSpecrumBuf, HPS_HARMONICS_COUNT);
            HPSAvailable(_powerSpecrumBuf);

            var peaks = AudioUtils.FindPeaks(_powerSpecrumBuf, 3, FrequencyToIndex(40.0), FrequencyToIndex(6000.0));
            PeaksFound(IndicesToFrequencies(peaks));
		}

	}
}
