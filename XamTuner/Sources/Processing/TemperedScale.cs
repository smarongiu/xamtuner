using System;
namespace XamTuner.Sources.Processing {
    public static class TemperedScale {

        public static readonly string[] KEYS = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        public static readonly float[] KEY_FREQUENCIES_BY_PICTH;
        public const double REFERENCE_KEY_FREQUENCY = 440.0d; //A4

        public const int MIN_PITCH = -57; //note C0
        public const int MAX_PITCH = 50; //note B8
        const int PITCH_COUNT = MAX_PITCH - MIN_PITCH + 1;

        static TemperedScale() {
            KEY_FREQUENCIES_BY_PICTH = new float[PITCH_COUNT];
            for (int i = 0; i < PITCH_COUNT; i++) {
                KEY_FREQUENCIES_BY_PICTH[i] = (float)Math.Round(REFERENCE_KEY_FREQUENCY * Math.Pow(2, (i + MIN_PITCH) / 12), 2);
            }
        }

        public static int FrequencyToPitchIndex(double freq, out double error) {
            var pitch = 12 * Math.Log(freq / REFERENCE_KEY_FREQUENCY, 2) - MIN_PITCH;
            var rounded = (int)Math.Round(pitch, 0);
            error = pitch - rounded;
            return rounded;
        }

        public static string FrequencyToKey(double freq, out int octave, out double error) {
            var pitch = FrequencyToPitchIndex(freq, out error);
            var idx = pitch % 12;
            octave = pitch / 12;
            return KEYS[idx];
        }

    }
}
