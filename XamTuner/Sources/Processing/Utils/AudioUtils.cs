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

		public static void ComputeHPS(double[] ps, int harmonicCount) {
			for(int i = 2; i <= harmonicCount; i++) {
				for(int j = 0; j < ps.Length; j++) {
                    ps[j] += ps.GetDownsampledValueAtIndex(j, i);
				}
			}
		}

		public static double[] ComputeHPS(Complex32[] fft, int harmonicCount, double[] buf = null) {
			buf = GetPowerSpectrum(fft, buf);
			ComputeHPS(buf, harmonicCount);
			return buf;
		}

        public static int[] FindPeaks(double[] values, int peaksCount) {
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
