using System.Linq;

namespace XamTuner {
	public static class AudioAnalysisExtensions {

		public static double GetDownsampledValueAtIndex(this double[] samples, int index, int factor, double fillDefaultValue = 0) {
			return (index * factor < samples.Length) ? samples[index * factor] : fillDefaultValue;
		}

        public static double[] ConvertToDb(this double[] samples) {
            var converted = new double[samples.Length];
            for (int i = 0; i < samples.Length; i++) {
                converted[i] = 10 * System.Math.Log10(samples[i]);
            }
            return converted;
        }

        public static double[] ToFrequencies(this int[] indices, int sampleCount, int sampleRate) {
            double[] fqs = new double[indices.Length];
            for (int i = 0; i < indices.Length; i++) {
                fqs[i] = (double)i / sampleCount * sampleRate / 2;
            }
            return fqs;
        }

	}
}
