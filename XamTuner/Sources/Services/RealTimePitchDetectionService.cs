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

		public const int HPS_FACTOR = 3;

		public event Action<DetectedPitchInfo> PitchDetected = delegate {};
		public event Action<double[]> FragmentReceived = delegate {};

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


			//get the power spectrum and the HSP
			AudioUtils.ComputeHPS(_fftBuf, HPS_FACTOR, _powerSpecrumBuf);

			FragmentReceived(_powerSpecrumBuf);
				
			//find max
			int index = -1;
			double max = double.MinValue;
			for(int i = 0; i < _powerSpecrumBuf.Length / HPS_FACTOR; i++) {
				if(_powerSpecrumBuf[i] > max) {
					max = _powerSpecrumBuf[i];
					index = i;
				}
			}

			if(index != -1) {
				var fq = (double)index / _powerSpecrumBuf.Length * SampleRate;
				System.Diagnostics.Debug.WriteLine($"max: fq={fq} -> {max}");
				PitchDetected(new DetectedPitchInfo(fq, max)); 
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
