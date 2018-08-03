using Autofac;
using Xamarin.Forms;
using XamTuner.Sources.Services;

namespace XamTuner {
	public static class AppContext {

        public static IContainer Container { get; private set; }

		public static void Init() {
			var b = new ContainerBuilder();

            b.Register(c => DependencyService.Get<IAudioCaptureService>()).As<IAudioCaptureService>().SingleInstance();
            b.RegisterType<RealTimePitchDetectionService>().As<IPitchDetectionService>().SingleInstance();
            b.RegisterType<XamTunerViewModel>().AsSelf();

			Container = b.Build();
		}
	}
}
