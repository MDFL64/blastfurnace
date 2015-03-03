namespace program {
	class SchemaItem {
		public string name;
		public string craft_type;
		public int class_flags = 0;
		public bool limited;
		public string[] prefabs;

		private System.Collections.ArrayList instances;
		private bool hasUncraftable;

		public SchemaItem() {
			instances = new System.Collections.ArrayList();
		}

		public void addClass(string player_class) {
			switch (player_class) {
				case "scout":
					class_flags |= 1;
					break;
				case "soldier":
					class_flags |= 2;
					break;
				case "pyro":
					class_flags |= 4;
					break;
				case "demoman":
					class_flags |= 8;
					break;
				case "heavy":
					class_flags |= 16;
					break;
				case "engineer":
					class_flags |= 32;
					break;
				case "medic":
					class_flags |= 64;
					break;
				case "sniper":
					class_flags |= 128;
					break;
				case "spy":
					class_flags |= 256;
					break;
			}
		}

		public int getClassId() {
			switch (class_flags) {
				case 1:
					return 1;
				case 2:
					return 2;
				case 4:
					return 3;
				case 8:
					return 4;
				case 16:
					return 5;
				case 32:
					return 6;
				case 64:
					return 7;
				case 128:
					return 8;
				case 256:
					return 9;
				default:
					return 0;
			}
		}

		public string[] apply(SchemaItem prefab) {
			if (prefab.craft_type != null)
				craft_type = prefab.craft_type;

			class_flags |= prefab.class_flags;

			return prefab.prefabs;
		}

		public void addUncraftable() {
			hasUncraftable = true;
		}

		public void addCraftable(ulong id) {
			instances.Add(id);
		}

		public bool finalCraftCheck() {
			if (!hasUncraftable && instances.Count > 0) {
				instances.RemoveAt(instances.Count - 1);
			}
			if (instances.Count > 0)
				return true;
			return false;
		}

		public ulong popCraftable() {
			ulong i = (ulong)instances[instances.Count - 1];
			instances.RemoveAt(instances.Count - 1);
			return i;
		}

		public int getCraftableCount() {
			return instances.Count;
		}
	}
}