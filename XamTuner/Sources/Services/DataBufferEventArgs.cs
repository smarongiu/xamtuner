using System;

namespace XamTuner.Sources.Services {
    public class DataBufferEventArgs : EventArgs {
        public readonly byte[] Data;

        public DataBufferEventArgs(byte[] data) {
            Data = data;
        }
    }
}