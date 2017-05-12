using Xamarin.Forms;

namespace XamTuner {
	public partial class XamTunerPage : ContentPage {
		public XamTunerPage() {
			Vm = new XamTunerViewModel();
			BindingContext = Vm;
			InitializeComponent();
		}

		XamTunerViewModel Vm { get; set; }

	}
}
