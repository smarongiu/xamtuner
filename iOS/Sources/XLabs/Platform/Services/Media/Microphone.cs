/*
 * based on XLabs frameworks : 
 * https://github.com/XLabs/Xamarin-Forms-Labs/blob/master/src/Platform/XLabs.Platform.iOS/Services/Media/Microphone.cs
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AudioToolbox;

namespace XLabs.Platform.Services.Media {	
	public class Microphone {
		private InputAudioQueue _audioQueue;

		private readonly int _bufferSize;

		public Microphone(int bufferSize = 4098) {
			_bufferSize = bufferSize;
		}

		private void StartRecording(int rate) {
			if(Active) {
				Clear();
			}

			SampleRate = rate;

			var audioFormat = new AudioStreamBasicDescription {
				SampleRate = SampleRate,
				Format = AudioFormatType.LinearPCM,
				FormatFlags = AudioFormatFlags.LinearPCMIsFloat,
				FramesPerPacket = 1,
				ChannelsPerFrame = 1,
				BitsPerChannel = sizeof(float) * 8,
				BytesPerPacket = sizeof(float),
				BytesPerFrame = sizeof(float),
				Reserved = 0
			};

			_audioQueue = new InputAudioQueue(audioFormat);
			_audioQueue.InputCompleted += QueueInputCompleted;

			var bufferByteSize = _bufferSize * audioFormat.BytesPerPacket;

			IntPtr bufferPtr;
			for(var index = 0; index < 3; index++) {
				_audioQueue.AllocateBufferWithPacketDescriptors(bufferByteSize, _bufferSize, out bufferPtr);
				_audioQueue.EnqueueBuffer(bufferPtr, bufferByteSize, null);
			}

			_audioQueue.Start();
		}

		private void Clear() {
			if(_audioQueue != null) {
				_audioQueue.Stop(true);
				_audioQueue.InputCompleted -= QueueInputCompleted;
				_audioQueue.Dispose();
				_audioQueue = null;
			}
		}

		private void QueueInputCompleted(object sender, InputCompletedEventArgs e) {
			// return if we aren't actively monitoring audio packets
			if(!Active) {
				return;
			}

			//var buffer = (AudioQueueBuffer)Marshal.PtrToStructure(e.IntPtrBuffer, typeof(AudioQueueBuffer));
			if(OnBroadcast != null) {
				var buf = new float[e.Buffer.AudioDataByteSize / sizeof(float)];
				e.Buffer.AudioData

				//var send = new byte[buffer.AudioDataByteSize];
				Marshal.Copy(buffer.AudioData, send, 0, (int)buffer.AudioDataByteSize);

				OnBroadcast(this, new EventArgs<byte[]>(send));
			}

			var status = _audioQueue.EnqueueBuffer(e.IntPtrBuffer, _bufferSize, e.PacketDescriptions);

			if(status != AudioQueueStatus.Ok) {
				// todo: 
			}
		}

		#region IAudioStream implementation

		public event EventHandler<EventArgs<byte[]>> OnBroadcast;

		public int SampleRate { get; private set; }

		public int ChannelCount {
			get {
				return 1;
			}
		}

		public int BitsPerSample {
			get {
				return 16;
			}
		}

		public bool Active {
			get {
				return _audioQueue != null && _audioQueue.IsRunning;
			}
		}

		public IEnumerable<int> SupportedSampleRates {
			get {
				return new[] { 8000, 16000, 22050, 41000, 44100 };
			}
		}

		public Task<bool> Start(int sampleRate) {
			return Task.Run(
				() => {
					if(!SupportedSampleRates.Contains(sampleRate)) {
						return false;
					}

					StartRecording(sampleRate);

					return Active;
				});
		}

		public Task Stop() {
			return Task.Run(() => Clear());
		}

		#endregion
	}
}