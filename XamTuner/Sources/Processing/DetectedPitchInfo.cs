using System;
using XamTuner.Sources.Processing.Tarsos;

namespace XamTuner.Sources.Processing {
    
    public class DetectedPitchInfo {
        public readonly Note Note;
        public readonly double NoteError;
        public readonly PitchDetectionResult PitchResult;

        public DetectedPitchInfo(Note note, double noteError, PitchDetectionResult pitch) {
            Note = note;
            NoteError = noteError;
            PitchResult = pitch;
        }
    }

}
