using Autofac;
namespace XamTuner {
	public static class AppContext {

		static IContainer _container;

		public static IContainer Container { 
			get {
				if(_container == null) Init();
				return _container;
			} 
		}

		public static void Init() {
			var b = new ContainerBuilder();
			b.RegisterType<XamTunerViewModel>().AsSelf().SingleInstance();
			//b.RegisterInstance(c => new XamTunerViewModel()).As<XamTunerViewModel>().SingleInstance();

			_container = b.Build();
		}

	}
}
