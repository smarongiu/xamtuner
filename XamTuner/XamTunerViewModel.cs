using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using XamTuner.Sources.Processing;
using XamTuner.Sources.Services;

namespace XamTuner {
	public class XamTunerViewModel : ViewModelBase {

        IPitchDetectionService _pitchDetectionService;

        public XamTunerViewModel(IPitchDetectionService pitchDetectionService) : base() {
            _pitchDetectionService = pitchDetectionService;
			_pitchDetectionService.PitchDetected += OnPitchDetected;
		}

		void OnPitchDetected(DetectedPitchInfo pi) {
			if(pi != null) {
                DetectedPitch = pi;
                RaisePropertyChanged(nameof(DetectedPitch));
			}
		}

		public bool IsStarted => _pitchDetectionService.IsStarted;

		public bool IsNotStarted => !IsStarted;

		public DetectedPitchInfo DetectedPitch { get; private set; }


		public RelayCommand StartCmd {
			get {
				return new RelayCommand(async () => {
					try {
						var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Microphone);
						if(status != PermissionStatus.Granted) {
							if(await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Microphone)) {
								//present a custom message to user here
							}
							var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Microphone });
							status = results[Permission.Microphone];
						}

						if(status == PermissionStatus.Granted) {

                            await Task.Run(_pitchDetectionService.Start);

							RaisePropertyChanged(nameof(IsStarted));
							RaisePropertyChanged(nameof(IsNotStarted));

						} else if(status != PermissionStatus.Unknown) {
                            //present a custom error message to user here
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
					await _pitchDetectionService.Stop();
					RaisePropertyChanged(nameof(IsStarted));
					RaisePropertyChanged(nameof(IsNotStarted));
				});
			}
		}


	}
}