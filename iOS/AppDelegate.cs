
using Foundation;
using UIKit;
using XLabs.Platform.Services.Media;

namespace XamTuner.iOS {
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate {
		public override bool FinishedLaunching(UIApplication app, NSDictionary options) {
			global::Xamarin.Forms.Forms.Init();
			OxyPlot.Xamarin.Forms.Platform.iOS.PlotViewRenderer.Init();
			AudioInputService.Init(new Microphone(AudioInputService.SampleBufferSize));

			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}
