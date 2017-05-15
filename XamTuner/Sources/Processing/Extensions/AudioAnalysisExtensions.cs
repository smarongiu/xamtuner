namespace XamTuner {
	public static class AudioAnalysisExtensions {

		public static double GetDownsampledValueAtIndex(this double[] samples, int index, int factor, double fillDefaultValue = 0) {
			return (index * factor < samples.Length) ? samples[index * factor] : fillDefaultValue;
		}

        public static void ConvertToDb(this double[] samples) {
            for (int i = 0; i < samples.Length; i++) {
                samples[i] = 10 * System.Math.Log10(samples[i]);
            }
        }
	}
}
