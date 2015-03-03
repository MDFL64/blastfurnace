using System.IO;

// This code was taken from https://github.com/Jessecar96/SteamBot/issues/57#issuecomment-16900889

namespace program {
	class GCMsgCraftItem : SteamKit2.Internal.IGCSerializableMessage {
		public uint GetEMsg() {
			return 1002;
		}
		public short recipe;
		public System.Collections.Generic.List<ulong> items = new System.Collections.Generic.List<ulong>();
		public void Serialize(Stream stream) {
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(recipe);
			writer.Write((short)items.Count);
			for (int i = 0; i < items.Count; i++) {
				writer.Write(items[i]);
			}
		}
		public void Deserialize(Stream stream) {
			//We don't need this.
		}
	}
}
