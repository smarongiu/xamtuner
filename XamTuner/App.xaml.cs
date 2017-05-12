using Xamarin.Forms;

namespace XamTuner {
	public partial class App : Application {

		public App() {
			AppContext.Init();

			InitializeComponent();

			MainPage = new XamTunerPage();
		}

		protected override void OnStart() {
			// Handle when your app starts
		}

		protected override void OnSleep() {
			// Handle when your app sleeps
		}

		protected override void OnResume() {
			// Handle when your app resumes
		}
	}
}
