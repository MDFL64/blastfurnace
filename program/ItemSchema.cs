using System;

namespace program {
	class ItemSchema {
		string source;
		int cursor = 0;

		System.Collections.Hashtable prefabs;
		System.Collections.Hashtable weapons;
		System.Collections.Hashtable attrs;
		
		public ItemSchema(string source) {
			prefabs = new System.Collections.Hashtable();
			weapons = new System.Collections.Hashtable();
			attrs = new System.Collections.Hashtable();
			
			this.source = source;

			string title = readString();
			if (title != "items_game")
				throw new Exception("Item schema title incorrect");

			startObj();
			while (!endObj()) {
				string key = readString();
				
				if (key == "items") {
					startObj();
					while (!endObj()) {
						string item_key = readString();
						int item_id = -1;
						if (item_key!="default")
							item_id=int.Parse(item_key);

						SchemaItem item = readItem();
						applyPrefabs(item, item.prefabs);

						if (item.craft_type == "weapon" && !item.limited)
							weapons[item_id] = item;
					}
				} else if (key=="prefabs") {
					startObj();
					while (!endObj()) {
						string prefab_key = readString();

						SchemaItem prefab = readItem();
						prefabs[prefab_key] = prefab;
					}
				}
				else if (key == "attributes") {
					startObj();
					while (!endObj()) {
						int attr_id = int.Parse(readString());

						startObj();
						while (!endObj()) {
							string attr_prop_key = readString();
							string attr_prop_value = readString();
							if (attr_prop_key=="name")
								attrs[attr_id] = attr_prop_value;
						}
					}
				}
				else
					skipAny();
			}
		}

		public SchemaItem get(int id) {
			return (SchemaItem)weapons[id];
		}

		public string getAttr(int id) {
			return (string)attrs[id];
		}

		private string readString() {
			while (true) {//todo catch out of bounds
				char c = source[cursor];
				if (Char.IsWhiteSpace(c))
					cursor++;
				else if (c == '"') {
					cursor++;
					int start = cursor;
					int length = 0;
					c = source[cursor];
					while (c != '"') {
						length++;
						cursor++;
						c = source[cursor];
					}
					cursor++;
					return source.Substring(start, length);
				}
				else {
					throw new Exception("Don't know what to do with char: "+c);
				}
			}
		}

		private void startObj() {
			while (true) {//todo catch out of bounds?
				char c = source[cursor];
				cursor++;
				if (c == '{')
					return;
				else if (!Char.IsWhiteSpace(c))
					throw new Exception("Don't know what to do with char: " + c);
			}
		}

		private bool endObj() {
			while (true) {//todo catch out of bounds?
				char c = source[cursor];
				if (c == '}') {
					cursor++;
					return true;
				}
				else if (!Char.IsWhiteSpace(c))
					return false;
				cursor++;
			}
		}

		private void skipAny() {
			while (true) {//todo catch out of bounds
				char c = source[cursor];
				if (c == '"') {
					readString();
					return;
				}
				else if (c == '{') {
					startObj();

					while (!endObj()) {
						readString(); //key
						skipAny(); //value
					}

					return;
				}
				else if (!Char.IsWhiteSpace(c))
					throw new Exception("Don't know what to do with char: " + c);
				cursor++;
			}
		}

		SchemaItem readItem() {
			SchemaItem item = new SchemaItem();

			startObj();
			while (!endObj()) {
				string key_property = readString();

				if (key_property == "name") {
					item.name = readString();
				}
				else if (key_property == "craft_class") {
					item.craft_type = readString();
				}
				else if (key_property == "used_by_classes") {
					startObj();
					while (!endObj()) {
						item.addClass(readString());
						readString();
					}
				}
				else if (key_property == "static_attrs") {
					startObj();
					while (!endObj()) {
						if (readString() == "limited quantity item")
							item.limited = true;
						readString();
					}
				}
				else if (key_property == "prefab") {
					item.prefabs = readString().Split(' ');
				}
				else
					skipAny();
			}

			return item;
		}

		void applyPrefabs(SchemaItem item, string[] prefabs_keys) {
			if (prefabs_keys != null) {
				foreach (var prefab_key in prefabs_keys) {
					var prefab = (SchemaItem)prefabs[prefab_key];
					applyPrefabs(item,item.apply(prefab));
				}
			}
		}

		public System.Collections.ArrayList[] getAllCraftables() {
			var lists = new System.Collections.ArrayList[10];

			for (int i = 0; i < 10; i++)
				lists[i] = new System.Collections.ArrayList();

				foreach (SchemaItem item in weapons.Values) {
					if (item.finalCraftCheck()) {
						lists[item.getClassId()].Add(item);
					}
				}

			return lists;
		}
	}
}
