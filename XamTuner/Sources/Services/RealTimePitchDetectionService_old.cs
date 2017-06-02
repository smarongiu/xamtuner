using System;
using System.Threading.Tasks;
using MathNet.Numerics;
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

		public event Action<DetectedPitchInfo> PitchDetected = delegate { };
		public event Action<double[]> PowerSpectrumAvailable = delegate { };
		public event Action<double[]> HPSAvailable = delegate { };
		public event Action<double[]> PeaksFound = delegate { };

		public int HPSHarmonicsCount { get; set; } = 3;
		public double DetectionThreshold { get; set; } = -75;

		//public double NoiseFilterCutPowerDb { get; set; } = -90;
		//public bool IsNoiseFilterEnabled { get; set; } = false;

		public bool IsStarted { get; private set; }
		public int SampleRate { get; private set; } = AudioInputService.DefaultSampleRate;


		Complex32[] _fftBuf;
		double[] _powerSpecrumBuf;
		int _sampleBufferSize;
		bool _busy;

		public RealTimePitchDetectionService() {
		}

		void InitBuffers() {
			_fftBuf = new Complex32[AudioInputService.SampleBufferSize];
			_sampleBufferSize = (int)Math.Ceiling((_fftBuf.Length + 1) / 2.0);
			_powerSpecrumBuf = new double[_sampleBufferSize];
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
			if(_busy) return;
			_busy = true;

			//convert samples to complex and apply a Hann function window
			AudioUtils.Convert16BitsSamplesWindowed(samples, MathNet.Numerics.Window.Hann(_fftBuf.Length), _fftBuf);

			var acf = AudioUtils.FindPitchByAutocorrelation(_fftBuf, SampleRate, _powerSpecrumBuf);
			System.Diagnostics.Debug.WriteLine($"ACF = {acf:F2}");

			//calculate the fft
			//MathNet.Numerics.IntegralTransforms.Fourier.Forward(_fftBuf);

			//AudioUtils.GetPowerSpectrum(_fftBuf, _powerSpecrumBuf);

			//if(IsNoiseFilterEnabled) {
			//	var minPw = Math.Pow(10, NoiseFilterCutPowerDb / 10);
			//	for(int i = 0; i < _sampleBufferSize; i++) {
			//		if(_powerSpecrumBuf[i] < minPw) _powerSpecrumBuf[i] = 0;
			//	}
			//}

			PowerSpectrumAvailable(_powerSpecrumBuf);

			//get the power spectrum and the HSP
			//AudioUtils.ComputeHPS(_powerSpecrumBuf, HPSHarmonicsCount);
			//HPSAvailable(_powerSpecrumBuf);

			var peaks = AudioUtils.FindPeaks(_powerSpecrumBuf, 3, FrequencyToIndex(100.0), FrequencyToIndex(4000.0));
			PeaksFound(IndicesToFrequencies(peaks));

			var pw = _powerSpecrumBuf[peaks[0]];
			if(pw > DetectionThreshold / HPSHarmonicsCount) {
				PitchDetected(new DetectedPitchInfo(IndexToFrequency(peaks[0]), pw));
			}

			_busy = false;
		}

	}
}
