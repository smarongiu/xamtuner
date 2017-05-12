using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using XLabs;

namespace XamTuner {
	public class XamTunerViewModel : ViewModelBase {

		RealTimePitchDetectionService _service;

		public OxyPlot.PlotModel PlotModel { get; private set; }

		public XamTunerViewModel() {
			_service = new RealTimePitchDetectionService();
			_service.PitchDetected += OnPitchDetected;
			_service.FragmentReceived += OnFragmentReceived;
			PlotModel = new OxyPlot.PlotModel() {
				Title = "Plot",
				Background = OxyColors.White,
				PlotAreaBorderColor = OxyColors.Gray
			};
			PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = 2000 });
			PlotModel.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Left, Minimum = 1e-100, Maximum = 1e-10 });
		}

		void OnFragmentReceived(double[] psd) {
			PlotModel.Series.Clear();
			var series = new LineSeries();
			for(int i = 0; i < psd.Length; i++) {
				var fq = (double)i / psd.Length * SampleRate;
				series.Points.Add(new OxyPlot.DataPoint(fq, psd[i]));
			}
			PlotModel.Series.Add(series);
			RaisePropertyChanged(nameof(PlotModel));
		}

		void OnPitchDetected(XamTuner.DetectedPitchInfo pi) {
			DetectedPitch = pi.Frequency.ToString("F2");
			vMax = pi.Power.ToString("E2");
			RaisePropertyChanged(nameof(DetectedPitch));
			RaisePropertyChanged(nameof(vMax));
		}

		public int SampleRate => _service.SampleRate;

		public bool IsStarted => _service.IsStarted;

		public bool IsNotStarted => !IsStarted;

		public string DetectedPitch { get; private set; }

		public IEnumerable<int> SupportedSampleRates {
			get {
				return AudioInputService.AudioStream.SupportedSampleRates;
			}
		}

		public RelayCommand StartCmd {
			get {
				return new RelayCommand(async () => {
					try {
						var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Microphone);
						if(status != PermissionStatus.Granted) {
							if(await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Microphone)) {
								//await DisplayAlert("Need microphone", "please please", "OK");
							}
							var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Microphone });
							status = results[Permission.Microphone];
						}

						if(status == PermissionStatus.Granted) {

							await _service.Start();
							RaisePropertyChanged(nameof(IsStarted));
							RaisePropertyChanged(nameof(IsNotStarted));

						} else if(status != PermissionStatus.Unknown) {
							//await DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
						}
					} catch(Exception ex) {
						System.Diagnostics.Debug.WriteLine($"error: {ex}");
					}
				});
			}
		}

		public RelayCommand StopCmd {
			get {
				return new RelayCommand(async () => {
					await _service.Stop();
					RaisePropertyChanged(nameof(IsStarted));
					RaisePropertyChanged(nameof(IsNotStarted));
				});
			}
		}


		//void InitBuffers() {
		//	FFTBuffer = new Complex32[AudioInputService.SampleBufferSize];
		//	int n = (int)Math.Ceiling((FFTBuffer.Length + 1) / 2.0);
		//	PowerSpectrumBuffer = new double[n];
		//}

		//async Task Start() {
		//	if(!IsStarted) {
		//		InitBuffers();
		//		AudioInputService.AudioStream.OnBroadcast += AudioStream_OnBroadcast;
		//		IsStarted = await AudioInputService.AudioStream.Start(SampleRate);
		//		RaisePropertyChanged(nameof(IsStarted));
		//		RaisePropertyChanged(nameof(IsNotStarted));
		//	}
		//}

		//async Task Stop() {
		//	if(IsStarted) {
		//		await AudioInputService.AudioStream.Stop();
		//		IsStarted = false;
		//		AudioInputService.AudioStream.OnBroadcast -= AudioStream_OnBroadcast;
		//		RaisePropertyChanged(nameof(IsStarted));
		//		RaisePropertyChanged(nameof(IsNotStarted));
		//	}
		//}


		//Complex32[] FFTBuffer { get; set; }
		//double[] PowerSpectrumBuffer { get; set; }


		//void AudioStream_OnBroadcast(object sender, EventArgs<byte[]> e) {
		//	var samples = e.Value;
		//	//System.Diagnostics.Debug.WriteLine($"received {samples.Length} samples [bps={AudioInputService.AudioStream.BitsPerSample}]");

		//	var Window = MathNet.Numerics.Window.Hann(FFTBuffer.Length);
		//	for(int i = 0; i < samples.Length; i += 2) {
		//		var v = (short)(samples[i + 1] << 8 | samples[i]);
		//		FFTBuffer[i / 2] = new Complex32((float)((double)v / 32768 * Window[i/2]), 0);
		//	}

		//	MathNet.Numerics.IntegralTransforms.Fourier.Forward(FFTBuffer);

		//	var psd = GetPowerSpectrum(FFTBuffer);
		//	double max = psd.Maximum();
		//	double mean = psd.Mean();
		//	double std = psd.StandardDeviation();

		//	vMax = max.ToString("E2");
		//	vMean = mean.ToString("E2");
		//	vStd = std.ToString("E2");
		//	RaisePropertyChanged(nameof(vMax));
		//	RaisePropertyChanged(nameof(vMean));
		//	RaisePropertyChanged(nameof(vStd));
		//	//System.Diagnostics.Debug.WriteLine($"max={max} mean={mean} std={std}");


		//	double threshold = mean;
		//	for(int i = 0; i < psd.Length; i++) {
		//		if(psd[i] > threshold) {
		//			var fq = (double)i / FFTBuffer.Length * SampleRate;
		//			//System.Diagnostics.Debug.WriteLine($"{fq} : [{i}] -> {psd[i]}");
		//			if(psd[i] > max * 0.95) {
		//				DetectedPitch = fq.ToString("F1");
		//				RaisePropertyChanged(nameof(DetectedPitch));
		//			}
		//		}
		//	}
		//}


		//public double[] GetPowerSpectrum(Complex32[] fft) {
		//	if(fft == null)
		//		throw new ArgumentNullException("fft");

		//	//int n = (int)System.Math.Ceiling((fft.Length + 1) / 2.0);

		//	//double[] mx = new double[n];

		//	PowerSpectrumBuffer[0] = fft[0].MagnitudeSquared() / fft.Length;

		//	for(int i = 1; i < PowerSpectrumBuffer.Length; i++) {
		//		PowerSpectrumBuffer[i] = fft[i].MagnitudeSquared() * 2.0 / fft.Length;
		//	}
		//	return PowerSpectrumBuffer;
		//}

		public string vMax { get; set; }
		public string vMean { get; set; }
		public string vStd { get; set; }
	}
}