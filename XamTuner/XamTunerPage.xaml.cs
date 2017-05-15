using System;
using Xamarin.Forms;

namespace XamTuner {
	public partial class XamTunerPage : ContentPage {
		public XamTunerPage() {
			Vm = new XamTunerViewModel();
			BindingContext = Vm;
			InitializeComponent();
            Vm.PlotDataChanged += OnPlotDataChanged;
		}

        void OnPlotDataChanged() {
			Device.BeginInvokeOnMainThread(() => Vm.PlotModel.InvalidatePlot(true));
		}

        XamTunerViewModel Vm { get; set; }
	}
}
