using System;
using System.IO;
using System.Net;
using System.Threading;

using SteamKit2.GC;
using MsgSys = SteamKit2.GC.Internal;
using MsgTF = SteamKit2.GC.TF2.Internal;

namespace program {
	class Program {
		static readonly uint MSG_REQ_INVENTORY = 1050;
		static readonly uint MSG_REQ_CRAFT = 1002;
		static readonly uint MSG_SCHEMA = 1049;
		static readonly uint MSG_CACHE_SUB = 24;

		static SteamGC gc;
		static ItemSchema schema;
		static System.Collections.ArrayList inventory;

		static void Main(string[] args) {
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("Blastfurnace Bulk Crafting Utility V1.0.1");
			Console.WriteLine("Written by Adam Coggeshall.");
			Console.WriteLine("Thanks to Steamkit2.");
			Console.WriteLine("This software is in no way supported by Valve.");
			Console.WriteLine();
			
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("This program uses the Steam API, pretends to be TF2, and interacts with the");
			Console.WriteLine("TF2 Game Coordinator in ways not intended by Valve. While the likelihood");
			Console.WriteLine("of users being banned or having their items deleted is low, the creator");
			Console.WriteLine("of this software takes no responsibility for bans or lost items.");
			Console.WriteLine();
			Console.WriteLine("This program is NOT intended to give users any significant advantage");
			Console.WriteLine("over others, and will not help players to achieve low craft numbers.");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("I WILL TAKE NO RESPONSIBILITY FOR ANYTHING BAD THAT HAPPENS TO YOU.");
			Console.WriteLine("ARE YOU SURE YOU WANT TO CONTINUE?");

			confirm();

			Console.WriteLine();
			
			//Create the schema folder.
			if (!Directory.Exists("schema"))
				Directory.CreateDirectory("schema");

			//Connect to Steam GC.
			gc = new SteamGC();
			if (!gc.isConnected()) {
				err("Failed to connect to Steam GC.");
			}
			log("Connected to Steam GC.");

			while (true) {
				PacketClientGCMsgProtobuf msg = gc.readMsg();
				if (msg != null) {
					if (msg.MsgType == MSG_SCHEMA) {
						var body = new ClientGCMsgProtobuf<MsgTF.CMsgUpdateItemSchema>(msg).Body;
						log("Item Schema #" + body.item_schema_version + " required...");

						//Get the schema
						string schema_path = "schema/" + body.item_schema_version + ".txt";
						string schema_text;
						if (File.Exists(schema_path)) {
							log("We have the correct schema.");
							schema_text = File.ReadAllText(schema_path);
							log("Schema loaded.");
						}
						else {
							log("Downloading schema...");
							schema_text = new StreamReader(WebRequest.Create(body.items_game_url).GetResponse().GetResponseStream()).ReadToEnd();
							File.WriteAllText(schema_path, schema_text);
							log("Schema saved.");
						}

						schema = new ItemSchema(schema_text);
						log("Schema parsed.");

						//Send inventory request.
						var reqInv = new ClientGCMsgProtobuf<MsgTF.CMsgRequestInventoryRefresh>(MSG_REQ_INVENTORY);
						if (!gc.sendMsg(reqInv)) {
							err("Failed to send inventory request!");
						}
						log("Requesting inventory...");
					}
					else if (msg.MsgType == MSG_CACHE_SUB) {
						var body = new ClientGCMsgProtobuf<MsgSys.CMsgSOCacheSubscribed>(msg).Body;
						log("Inventory received.");
						var obj = body.objects.Find(o => o.type_id == 1);

						inventory = new System.Collections.ArrayList();
						foreach (var item_data in obj.object_data) {
							using (MemoryStream ms = new MemoryStream(item_data)) {
								var item = ProtoBuf.Serializer.Deserialize<MsgTF.CSOEconItem>(ms);
								inventory.Add(item);
							}
						}

						log("Inventory decoded.");

						doCraft();
						break;
					}
					/*else {
						Console.WriteLine("MSG " + msg.MsgType);
					}*/
				}
			}
		}

		static void log(string msg) {
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("[>] "+msg);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void err(string msg) {
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine("[!] Error: " + msg);
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.ReadKey();
			Environment.Exit(1);
		}

		static void confirm() {
			Console.ForegroundColor = ConsoleColor.Green;

			string response;
			while (true) {
				Console.WriteLine();
				Console.WriteLine("Type \"confirm\" to continue.");
				Console.WriteLine("Type \"exit\" or just quit the program to exit.");
				Console.WriteLine();
				response = Console.ReadLine();
				if (response == "confirm" || response == "exit")
					break;
			}

			if (response != "confirm")
				Environment.Exit(0);

			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void doCraft() {
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine();
			log("Excluding special items, none of the following items will be crafted:");
			foreach (MsgTF.CSOEconItem item in inventory) {
				SchemaItem si = schema.get((int)item.def_index);
				if (si != null) {
					bool failed = false;

					if (item.quality != 6) {
						if (!failed) {
							Console.WriteLine("\n#" + item.id + " (" + si.name + "):");
							si.addUncraftable();
							failed = true;
						}
						Console.WriteLine("\tquality(" + getQualityName(item.quality) + ")");
					}

					if (item.def_index == 474) {
						if (!failed) {
							Console.WriteLine("\n#" + item.id + " (" + si.name + "):");
							si.addUncraftable();
							failed = true;
						}
						Console.WriteLine("\tblackslist(sign)");
					}

					foreach (var a in item.attribute) {
						if (!failed) {
							Console.WriteLine("\n#" + item.id + " (" + si.name + "):");
							si.addUncraftable();
							failed = true;
						}
						Console.WriteLine("\tattr(" + schema.getAttr((int)a.def_index) + ")");
					}

					if (item.origin != 0) {
						if (!failed) {
							Console.WriteLine("\n#" + item.id + " (" + si.name + "):");
							si.addUncraftable();
							failed = true;
						}
						Console.WriteLine("\torigin(" + getOriginName(item.origin) + ")");

						Console.ForegroundColor = ConsoleColor.Red;
						if (item.origin == 1) {
							Console.WriteLine("\tNOTE: ITEM NOT TRADABLE.");
						}
						else if (item.origin == 2) {
							Console.WriteLine("\tNOTE: ITEM NOT CRAFTABLE.");
						}
						Console.ForegroundColor = ConsoleColor.Gray;
					}

					if (!failed)
						si.addCraftable(item.id);
				}
			}

			Console.WriteLine();
			log("Listing craftables, the following items will be crafted:");
			Console.WriteLine();
			var craftables = schema.getAllCraftables();
			for (int i = 0; i < 10; i++) {
				Console.WriteLine(getClassName(i) + ":");
				var itemset = craftables[i];
				foreach (SchemaItem item in itemset) {
					Console.WriteLine("\t" + item.getCraftableCount() + " X " + item.name);
				}
				if (itemset.Count==0)
					Console.WriteLine("\tNone");
			}

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine();
			Console.WriteLine("Ready to craft. Please confirm that you want to craft these items. Make sure");
			Console.WriteLine("your valuable items are in the exclude section, or that their types are not");
			Console.WriteLine("listed in the crafting section. I have tried to filter out your valuable items,");
			Console.WriteLine("but I will take no responsibility for lost items.");
			Console.WriteLine();
			Console.WriteLine("Blastfurnace has not been tested against:");
			Console.WriteLine("\t- Australium weapons");
			Console.WriteLine("\t- Killstreak weapons");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("I WILL TAKE NO RESPONSIBILITY FOR LOST ITEMS.");
			Console.WriteLine("ARE YOU SURE YOU WANT TO CRAFT THE ABOVE ITEMS?");

			confirm();

			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine();
			Console.WriteLine("CRAFTING!");
			Console.WriteLine();
			for (int i = 1; i < 10; i++) {
				var itemset = craftables[i];

				while (true) {
					string type1 = null;
					ulong id1 = 0;

					foreach (SchemaItem item in itemset)
						if (item.getCraftableCount() != 0) {
							id1 = item.popCraftable();
							type1 = item.name;
							break;
						}

					if (type1 == null)
						break;

					string type2 = null;
					ulong id2 = 0;

					foreach (SchemaItem item in itemset)
						if (item.getCraftableCount() != 0) {
							id2 = item.popCraftable();
							type2 = item.name;
							break;
						}

					if (type2 == null)
						break;

					Console.WriteLine("CRAFT:");
					Console.WriteLine("\t+" + type1 + "#" + id1);
					Console.WriteLine("\t+" + type2 + "#" + id2);

					ClientGCMsg<GCMsgCraftItem> msg = new ClientGCMsg<GCMsgCraftItem>();
					msg.Body.recipe = 3;
					msg.Body.items.Add(id1);
					msg.Body.items.Add(id2);

					if (!gc.sendMsg(msg, false)) {
						Console.WriteLine("\tFailed to send craft msg!");
					}
					Thread.Sleep(1000);
				}
			}

			Console.WriteLine("Done.");
			Console.ReadKey();
		}

		static string getClassName(int c) {
			switch (c) {
				case 0:
					return "Multi-Class";
				case 1:
					return "Scout";
				case 2:
					return "Soldier";
				case 3:
					return "Pyro";
				case 4:
					return "Demoman";
				case 5:
					return "Heavy";
				case 6:
					return "Engineer";
				case 7:
					return "Medic";
				case 8:
					return "Sniper";
				case 9:
					return "Spy";
			}
			return "?";
		}

		static string getQualityName(uint q) {
			switch (q) {
				case 0:
					return "normal";
				case 1:
					return "genuine";
				case 3:
					return "vintage";
				case 5:
					return "unusual";
				case 6:
					return "unique";
				case 7:
					return "community";
				case 8:
					return "valve";
				case 9:
					return "self-made";
				case 11:
					return "strange";
				case 13:
					return "haunted";
				case 14:
					return "collector's";
				default:
					return "q" + q;
			}
		}

		static string getOriginName(uint o) {
			switch (o) {
				case 0:
					return "drop";
				case 1:
					return "achievement";
				case 3:
					return "traded";
				case 2:
					return "purchase";
				case 4:
					return "craft";
				case 8:
					return "crate";
				case 21:
					return "mvm surplus";
				default:
					return "q" + o;
			}
		}
	}
}