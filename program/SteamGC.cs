using System;
using System.Runtime.InteropServices;

using SteamKit2.GC;

namespace program {
	class SteamGC {
		private bool connected = false;
		private IntPtr interfacePtr;

		public SteamGC() {
			interfacePtr = _init();
			connected = (interfacePtr != IntPtr.Zero);
		}

		public bool isConnected() {
			return connected;
		}

		public PacketClientGCMsgProtobuf readMsg() {
			if (!connected) return null;

			//Check if a message is ready.
			uint msg_size = 0;
			if (!_check(interfacePtr, ref msg_size)) return null;

			uint msg_type = 0;
			byte[] buffer = new byte[msg_size];
			if (!_recv(interfacePtr, msg_size, ref msg_type, buffer)) return null;

			return new PacketClientGCMsgProtobuf(msg_type & 0x7FFFFFFF, buffer);
		}

		public bool sendMsg(IClientGCMsg msg, bool fixit = true) {
			if (!connected) return false;

			byte[] buffer = msg.Serialize();

			uint mask = 0;
			if (fixit) {
				mask = 0x80000000;
			}

			return _send(interfacePtr, msg.MsgType | mask, buffer, (uint)buffer.Length);
		}

		[DllImport("gclink.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr _init();

		[DllImport("gclink.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool _check(IntPtr ifptr, ref uint size);

		[DllImport("gclink.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool _recv(IntPtr ifptr, uint size, ref uint id, byte[] buffer);

		[DllImport("gclink.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool _send(IntPtr ifptr, uint id, byte[] buffer, uint size);
	}
}
