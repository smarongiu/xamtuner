using System;
using MathNet.Numerics;

namespace XamTuner {
	public static class AudioUtils {

		public static double[] GetPowerSpectrum(Complex32[] fft, double[] powerSpectrumBuf = null) {
			if(fft == null)
				throw new ArgumentNullException("fft");

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


		public static void ComputeHPS(double[] ps, int harmonicCount = 5) {
			for(int i = 2; i <= harmonicCount; i++) {
				for(int j = 0; j < ps.Length; j++) {
					ps[j] *= ps.GetDownsampledValueAtIndex(j, i);
				}
			}
		}


		public static double[] ComputeHPS(Complex32[] fft, int harmonicCount = 5, double[] buf = null) {
			buf = GetPowerSpectrum(fft, buf);
			ComputeHPS(buf, harmonicCount);
			return buf;
		}
	}
}
