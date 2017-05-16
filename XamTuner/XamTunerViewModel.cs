using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using XLabs;

namespace XamTuner {
    public enum ViewMode {
        PowerSpectrum,
        HarmonicPowerSpectrum
    };

    public class XamTunerViewModel : ViewModelBase {

        RealTimePitchDetectionService _service;

        public OxyPlot.PlotModel PlotModel { get; private set; }

        public event Action PlotDataChanged = delegate { };

        LineSeries _series;
        ViewMode _viewMode;

        public XamTunerViewModel() {
            _service = new RealTimePitchDetectionService();
            _service.PitchDetected += OnPitchDetected;
            _service.PeaksFound += OnPeaksFound;
            PlotModel = new OxyPlot.PlotModel() {
                Title = "Harmonic Power Spectrum",
                Background = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Gray
            };
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = 1000 });
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = -300, Maximum = 3 });
            _series = new LineSeries();
            PlotModel.Series.Add(_series);
            ViewMode = ViewMode.PowerSpectrum;
        }


        public ViewMode ViewMode {
            get { return _viewMode; }
            set {
                _viewMode = value;
                if (_viewMode == ViewMode.PowerSpectrum) {
                    _service.PowerSpectrumAvailable += OnPSReceived;
                    _service.HPSAvailable -= OnHPSReceived;
                } else {
                    _service.PowerSpectrumAvailable -= OnPSReceived;
                    _service.HPSAvailable += OnHPSReceived;
                }
            }
        }

        void OnPSReceived(double[] psd) {
            _series.Points.Clear();
            var dbs = psd.ConvertToDb();
            double k = (double)dbs.Length / SampleRate * 2;
            for (int i = 0; i < dbs.Length; i++) {
                var fq = (double)i / k;
                _series.Points.Add(new OxyPlot.DataPoint(fq, dbs[i]));
            }
            PlotDataChanged();
        }

        void OnHPSReceived(double[] psd) {
            _series.Points.Clear();
            var dbs = psd.ConvertToDb();
            var k = dbs.Length * SampleRate / 2;
            for (int i = 0; i < dbs.Length; i++) {
                var fq = (double)i / k;
                _series.Points.Add(new OxyPlot.DataPoint(fq, dbs[i]));
            }
            PlotDataChanged();
        }


        void OnPitchDetected(XamTuner.DetectedPitchInfo pi) {
            if (pi != null) {
                DetectedPitch = pi.Frequency.ToString("F2");
                Description = pi.Power.ToString("E2");
            } else {
                DetectedPitch = "--";
            }
            RaisePropertyChanged(nameof(DetectedPitch));
            RaisePropertyChanged(nameof(Description));
        }

        private void OnPeaksFound(double[] peaks) {
            var s = "";
            foreach (var p in peaks) {
                s += $"{p:F2} ";
            }
            Description = s;
            RaisePropertyChanged(Description);
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
                        if (status != PermissionStatus.Granted) {
                            if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Microphone)) {
                                //await DisplayAlert("Need microphone", "please please", "OK");
                            }
                            var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Microphone });
                            status = results[Permission.Microphone];
                        }

                        if (status == PermissionStatus.Granted) {

                            await _service.Start();
                            RaisePropertyChanged(nameof(IsStarted));
                            RaisePropertyChanged(nameof(IsNotStarted));

                        } else if (status != PermissionStatus.Unknown) {
                            //await DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
                        }
                    } catch (Exception ex) {
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