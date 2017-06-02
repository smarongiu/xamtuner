using System;
namespace XamTuner.Sources.Processing {
    
    public class DetectedPitchInfo {
        public readonly double Frequency;
        public readonly double Power;

        public DetectedPitchInfo(double fq, double pw) {
            Frequency = fq;
            Power = pw;
        }
    }

}
