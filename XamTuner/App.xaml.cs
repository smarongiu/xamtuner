﻿using System;
using Autofac;
using Xamarin.Forms;
using XamTuner.Sources.Services;

namespace XamTuner {
	public partial class App : Application {

        public App() {
            AppContext.Init();            
            InitializeComponent();
            MainPage = new XamTunerPage(AppContext.Container.Resolve<XamTunerViewModel>());
		}

		protected override void OnStart() {
		}

		protected override async void OnSleep() {
            IPitchDetectionService srv = AppContext.Container.Resolve<IPitchDetectionService>();
            if (srv.IsStarted) {
                await srv.Stop();
            }
		}

		protected override void OnResume() {
		}
	}
}
