using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace XamTuner {
	public partial class XamTunerPage : ContentPage {
		public XamTunerPage() {
			Vm = new XamTunerViewModel();
			BindingContext = Vm;
			InitializeComponent();

            Vm.PropertyChanged += Vm_PropertyChanged;
		}

        XamTunerViewModel Vm { get; set; }

        void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Vm.DetectedPitch)) {
                Device.BeginInvokeOnMainThread(() => {
                    DetectedNoteLabel.Text = $"{Vm.DetectedPitch.Note} ({Vm.DetectedPitch.NoteError})";
                    DetectedNoteLabel.TextColor = (Math.Abs(Vm.DetectedPitch.NoteError) < 0.1) ? Color.Green : Color.Yellow;
                    PitchLabel.Text = $"{Vm.DetectedPitch.PitchResult.Pitch} Hz";
                });
            }
        }

    }
}
