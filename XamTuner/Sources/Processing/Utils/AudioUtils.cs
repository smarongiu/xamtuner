using System;
using System.Linq;
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
			for(int i = 0; i < samples.Length; i += 2) {
				var v = (short)(samples[i + 1] << 8 | samples[i]);
				buf[i / 2] = new Complex32((float)((double)v / 32768 * window[i / 2]), 0);
			}
			for(int i = samples.Length; i < buf.Length; i += 2) {
				buf[i / 2] = new Complex32(0, 0);
			}
			return buf;
		}


		//https://stackoverflow.com/questions/27117391/android-finding-fundamental-frequency-of-audio-input/27123449#27123449
		public static double FindPitchByAutocorrelation(Complex32[] samples,  int sampleRate, double[] acfBuf = null) {
			int low = sampleRate / 4500;
			int hi = sampleRate / 40;

			if (acfBuf == null) acfBuf = new double[samples.Length];
			Array.Clear(acfBuf, 0, acfBuf.Length);

			for(int period = low; period < hi; period++) {
				double sum = 0;
				for(int i = 0; i < samples.Length - period; i++) {
					sum += samples[i].Real * samples[i + period].Real;
				}
				double mean = sum / samples.Length;
				//[period - low] = mean;
				acfBuf[period] = mean;
			}

			double bestValue = double.MinValue;
			int bestIndex = -1;
			for(int i = low; i < hi; i++) {
				if(acfBuf[i] > bestValue) {
					bestIndex = i;
					bestValue = acfBuf[i];
				}
			}

			double res = (double)sampleRate / (bestIndex + low);
			return res;
		}


		public static void ComputeHPS(double[] ps, int harmonicsCount, double factor = 1d) {
			for(int i = 2; i <= harmonicsCount; i++) {
				for(int j = 0; j < ps.Length; j++) {
					ps[j] *= ps.GetDownsampledValueAtIndex(j, i);
				}
			}
		}

		public static double[] ComputeHPS(Complex32[] fft, int harmonicsCount, double factor = 1d, double[] buf = null) {
			buf = GetPowerSpectrum(fft, buf);
			ComputeHPS(buf, harmonicsCount, factor);
			return buf;
		}


		public static int[] FindPeaks(double[] values, int peaksCount, int startIndex, int endIndex) {
			double[] peakValues = new double[peaksCount];
			int[] peakIndices = new int[peaksCount];

			for(int i = 0; i < peaksCount; i++) {
				peakValues[i] = values[peakIndices[i] = i + startIndex];
			}

			// find min peaked value
			double minStoredPeak = peakValues[0];
			int minIndex = 0;
			for(int i = 1; i < peaksCount; i++) {
				if(minStoredPeak > peakValues[i]) minStoredPeak = peakValues[minIndex = i];
			}

			for(int i = peaksCount + startIndex; i < endIndex; i++) {
				if(minStoredPeak < values[i]) {
					// replace the min peaked value with bigger one
					peakValues[minIndex] = values[peakIndices[minIndex] = i];

					// and find min peaked value again
					minStoredPeak = peakValues[minIndex = 0];
					for(int j = 1; j < peaksCount; j++) {
						if(minStoredPeak > peakValues[j]) minStoredPeak = peakValues[minIndex = j];
					}
				}
			}

			var idx = peakIndices.OrderByDescending(i => values[i]).ToArray();
			//for(int i = 0; i < idx.Length; i++) {
			//	System.Diagnostics.Debug.WriteLine($"[{i}] : {((double)idx[i] / values.Length * 11025):F2} -> {10 * System.Math.Log10(values[idx[i]]):F1} ");
			//}
			return idx;
		}


	}
}
