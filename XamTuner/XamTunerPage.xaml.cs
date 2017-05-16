using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace XamTuner {
	public partial class XamTunerPage : ContentPage {
		public XamTunerPage() {
			Vm = new XamTunerViewModel();
			BindingContext = Vm;
			InitializeComponent();
            Vm.PlotDataChanged += OnPlotDataChanged;
            Vm.PropertyChanged += OnPropertyChanged;
		}

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            //if (e.PropertyName.Equals(nameof(Vm.Description))) {
                //Device.BeginInvokeOnMainThread(() => Info.Text = );                
            //}
        }

        void OnPlotDataChanged() {
			Device.BeginInvokeOnMainThread(() => Vm.PlotModel.InvalidatePlot(true));
		}

        XamTunerViewModel Vm { get; set; }
	}
}
