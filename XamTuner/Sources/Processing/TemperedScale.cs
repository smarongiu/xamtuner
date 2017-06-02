using System;
namespace XamTuner.Sources.Processing {
    public static class TemperedScale {

        public static readonly string[] KEYS = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        public static readonly float[] KEY_FREQUENCIES_BY_PICTH;
        public const double REFERENCE_KEY_FREQUENCY = 440.0d; //A4

        public const int MIN_PITCH = -57; //note C0
        public const int MAX_PITCH = 50; //note B8

        static TemperedScale() {
            for (int i = MIN_PITCH; i < MAX_PITCH; i++) {
                KEY_FREQUENCIES_BY_PICTH[i] = (float)Math.Round(REFERENCE_KEY_FREQUENCY * Math.Pow(2, i / 12), 2);
            }
        }

        public static int FrequencyToPitchIndex(double freq, out double error) {
            var pitch = 12 * Math.Log(freq / REFERENCE_KEY_FREQUENCY, 2) - MIN_PITCH;
            error = pitch - (int)pitch;
            return (int)pitch;
        }

        public static string FrequencyToKey(double freq, out int octave, out double error) {
            var pitch = FrequencyToPitchIndex(freq, out error);
            var idx = pitch % 12;
            octave = pitch / 12;
            return KEYS[idx];
        }

    }
}
