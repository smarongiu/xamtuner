﻿using XLabs.Platform.Services.Media;
namespace XamTuner {

	public interface IAudioCaptureService {
		IAudioStream GetStream();
		int SampleBufferMs { get; set; }
		int SampleRate { get; set; }

	}

	public static class AudioInputService {

        public const int SampleBufferMs = 333;
		public const int DefaultSampleRate = 22050;

		public const int SampleBufferSize = DefaultSampleRate * SampleBufferMs / 1000;

		public static IAudioStream AudioStream { get; private set; }

		public static void Init(IAudioStream audioStream) {
			AudioStream = audioStream;
		}

	}

}
