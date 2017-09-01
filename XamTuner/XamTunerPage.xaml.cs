using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace XamTuner {
	public partial class XamTunerPage : ContentPage {
		
        public XamTunerPage(XamTunerViewModel vm) {
            Vm = vm;
			BindingContext = Vm;
			InitializeComponent();

            Vm.StartCmd.Execute(null);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            Vm.PropertyChanged += Vm_PropertyChanged;
        }

        protected override void OnDisappearing() {
            Vm.PropertyChanged -= Vm_PropertyChanged;
            base.OnDisappearing();
        }

        XamTunerViewModel Vm { get; set; }

        void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Vm.DetectedPitch)) {
                Device.BeginInvokeOnMainThread(() => {
                    var err = Vm.DetectedPitch.NoteError;
                    DetectedNoteLabel.Text = Vm.DetectedPitch.Note.ToString();
                    if (Math.Abs(err) < 0.1) {
                        DetectedNoteLabel.TextColor = Color.Green;
                        DetectedNoteLabel.FontSize = 70;
                        LeftLabel.Text = "";
                        RightLabel.Text = "";
                    } else {
                        DetectedNoteLabel.FontSize = 60;
                        DetectedNoteLabel.TextColor = Color.Silver;
                        if (err < 0) {
                            LeftLabel.Text = MakeDotLabel(err);
                            RightLabel.Text = "";
                        } else {
                            LeftLabel.Text = "";
                            RightLabel.Text = MakeDotLabel(err);
                        }
                    }
                    PitchLabel.Text = $"{Vm.DetectedPitch.PitchResult.Pitch:F2} Hz";
                });
            }
        }

        string MakeDotLabel(double error) {
            var count = (int)Math.Round(Math.Abs(error) * 100 / 12.5, 0);
            return new string('•', count);
        }

    }
}
