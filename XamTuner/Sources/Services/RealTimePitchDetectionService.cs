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

        public const int HPS_FACTOR = 5;

		public event Action<DetectedPitchInfo> PitchDetected = delegate {};
		public event Action<double[]> FragmentReceived = delegate {};

        public double DetectionThreshold { get; set; } = -60;

		public RealTimePitchDetectionService() {
		}

		Complex32[] _fftBuf;
		double[] _powerSpecrumBuf;

		void InitBuffers() {
			_fftBuf = new Complex32[AudioInputService.SampleBufferSize];
			int n = (int)Math.Ceiling((_fftBuf.Length + 1) / 2.0);
			_powerSpecrumBuf = new double[n];
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


		void OnNewFragment(object sender, EventArgs<byte[]> e) {
			var samples = e.Value;

			//convert samples to complex and apply a Hann function window
			AudioUtils.Convert16BitsSamplesWindowed(samples, MathNet.Numerics.Window.Hann(_fftBuf.Length), _fftBuf);

			//calculate the fft
			MathNet.Numerics.IntegralTransforms.Fourier.Forward(_fftBuf);

            AudioUtils.GetPowerSpectrum(_fftBuf, _powerSpecrumBuf);

			//get the power spectrum and the HSP
			//AudioUtils.ComputeHPS(_powerSpecrumBuf, HPS_FACTOR);

            _powerSpecrumBuf.ConvertToDb();
            FragmentReceived(_powerSpecrumBuf);

            double mean = _powerSpecrumBuf.Mean();
            double std = _powerSpecrumBuf.StandardDeviation();
            double threshold = -50;

			//find max
			int index = -1;
            double max = DetectionThreshold;
			for(int i = 0; i < _powerSpecrumBuf.Length; i++) {
				if(_powerSpecrumBuf[i] > max) {
					max = _powerSpecrumBuf[i];
					index = i;
				}
			}
            System.Diagnostics.Debug.WriteLine($"mean={mean} max={max} @[{index}] std={std}");

			if(index != -1) {
				var fq = (double)index / _powerSpecrumBuf.Length * SampleRate;
				//System.Diagnostics.Debug.WriteLine($"max: fq={fq} -> {max}");
				PitchDetected(new DetectedPitchInfo(fq, max));
            } else {
                PitchDetected(null);
            }

			//double max = hsp.Maximum();
			//double mean = hsp.Mean();
			//double std = hsp.StandardDeviation();

			//double threshold = mean;
			//for(int i = 0; i < hsp.Length; i++) {
			//	if(hsp[i] > threshold) {
			//		var fq = (double)i / _fftBuf.Length * SampleRate;
			//		//System.Diagnostics.Debug.WriteLine($"{fq} : [{i}] -> {psd[i]}");
			//		if(hsp[i] > max * 0.95) {
			//			DetectedPitch = fq.ToString("F1");
			//			RaisePropertyChanged(nameof(DetectedPitch));
			//		}
			//	}
			//}
		}

	}
}
