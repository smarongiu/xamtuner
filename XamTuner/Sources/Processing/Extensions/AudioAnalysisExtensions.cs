namespace XamTuner {
	public static class AudioAnalysisExtensions {

		public static double GetDownsampledValueAtIndex(this double[] samples, int index, int factor) {
			return (index * factor < samples.Length) ? samples[index * factor] : 0;
		}


	}
}
