using System;
namespace XamTuner.Sources.Processing {
    public class Note {

        public readonly string Key;
        public readonly int Octave;

        public Note(string key, int octave) {
            Key = key;
            Octave = octave;
        }

        public static Note FromFrequency(float frequency, out double error) {
            int octave;
            var note = new Note(TemperedScale.FrequencyToKey((float)frequency, out octave, out error), octave);
            return note;
        }

        public override string ToString() {
            return $"{Key}{Octave}";
        }
    }
}
