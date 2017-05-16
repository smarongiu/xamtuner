using System;
using MathNet.Numerics;

namespace XamTuner {
	public static class AudioUtils {

		public static double[] GetPowerSpectrum(Complex32[] fft, double[] powerSpectrumBuf = null) {
			if(powerSpectrumBuf == null) {
				int n = (int)System.Math.Ceiling((fft.Length + 1) / 2.0);
				powerSpectrumBuf = new double[n];
			}
			powerSpectrumBuf[0] = fft[0].MagnitudeSquared() / fft.Length;
			for(int i = 1; i < powerSpectrumBuf.Length; i++) {
				powerSpectrumBuf[i] = fft[i].MagnitudeSquared() * 2.0 / fft.Length;
			}
			return powerSpectrumBuf;
		}


		public static Complex32[] Convert16BitsSamples(byte[] samples, Complex32[] buf = null) {
			if(buf == null) buf = new Complex32[samples.Length / 2];
			for(int i = 0; i < samples.Length; i += 2) {
				var v = (short)(samples[i + 1] << 8 | samples[i]);
				buf[i / 2] = new Complex32((float)v / 32768, 0);
			}
			return buf;
		}

		public static Complex32[] Convert16BitsSamplesWindowed(byte[] samples, double[] window, Complex32[] buf = null) { 
			if(buf == null) buf = new Complex32[samples.Length / 2];
			for(int i = 0; i<samples.Length; i += 2) {
				var v = (short)(samples[i + 1] << 8 | samples[i]);
				buf[i / 2] = new Complex32((float)((double)v / 32768 * window[i / 2]), 0);
			}
			return buf;
		}

        public static void ComputeHPS(double[] ps, int harmonicsCount, double factor = 1d) {
			for(int i = 2; i <= harmonicsCount; i++) {
				for(int j = 0; j < ps.Length; j++) {
                    ps[j] += ps.GetDownsampledValueAtIndex(j, i) * factor;
				}
			}
		}

        public static double[] ComputeHPS(Complex32[] fft, int harmonicsCount, double factor = 1d, double[] buf = null) {
			buf = GetPowerSpectrum(fft, buf);
            ComputeHPS(buf, harmonicsCount, factor);
			return buf;
		}


        /// <summary>
        /// Finds the first peakCount peaks indices.
        /// </summary>
        /// <returns>The peaks.</returns>
        /// <param name="values">Values.</param>
        /// <param name="peaksCount">Peaks count.</param>
        public static int[] FindPeaks(double[] values, int peaksCount, int startIndex, int endIndex) {
            //double[] peakValues = new double[peaksCount];
            int[] peakIndices = new int[peaksCount];
            for (int i = 0; i < peaksCount; i++) {
                peakIndices[i] = startIndex;
                //peakValues[i] = values[0];
            }
            for (int i = startIndex + 1; i < endIndex; i++) {
                if (values[i] > values[peakIndices[0]]) {
                    for (int j = peaksCount - 1; j > 0; j--) {
                        //peakValues[j] = peakValues[j - 1];
                        peakIndices[j] = peakIndices[j - 1];
                    }
                    //peakValues[0] = values[i];
                    peakIndices[0] = i;

                    System.Diagnostics.Debug.WriteLine($"peaks: [{i}] {peakIndices[0]}, {peakIndices[1]}, {peakIndices[2]}");
                }
            }
            return peakIndices;
        }


        public static int[] FindPeaks_(double[] values, int peaksCount) {
            double[] peakValues = new double[peaksCount];
            int[] peakIndices = new int[peaksCount];

            for (int i = 0; i < peaksCount; i++) {
                peakValues[i] = values[peakIndices[i] = i];
            }

            // find min peaked value
            double minStoredPeak = peakValues[0];
            int minIndex = 0;
            for (int i = 1; i < peaksCount; i++) {
                if (minStoredPeak > peakValues[i]) minStoredPeak = peakValues[minIndex = i];
            }

            for (int i = peaksCount; i < values.Length; i++) {
                if (minStoredPeak < values[i]) {
                    // replace the min peaked value with bigger one
                    peakValues[minIndex] = values[peakIndices[minIndex] = i];

                    // and find min peaked value again
                    minStoredPeak = peakValues[minIndex = 0];
                    for (int j = 1; j < peaksCount; j++) {
                        if (minStoredPeak > peakValues[j]) minStoredPeak = peakValues[minIndex = j];
                    }
                }
            }

            return peakIndices;
        }

    }
}
