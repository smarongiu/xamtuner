using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using XamTuner.Services;
using XamTuner.Sources.Processing;
using XLabs;

namespace XamTuner {
	public class XamTunerViewModel : ViewModelBase {

		RealTimePitchDetectionService _service;

		public XamTunerViewModel() {
			_service = new RealTimePitchDetectionService();
			_service.PitchDetected += OnPitchDetected;
		}

		void OnPitchDetected(DetectedPitchInfo pi) {
			if(pi != null) {
				DetectedPitch = pi.Frequency.ToString("F2");
				RaisePropertyChanged(nameof(DetectedPitch));
			}
		}

		public int SampleRate => _service.SampleRate;

		public bool IsStarted => _service.IsStarted;

		public bool IsNotStarted => !IsStarted;

		public string DetectedPitch { get; private set; }

		public string Description { get; private set; }


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


	}
}