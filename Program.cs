using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace RDDKP
{
	class Program
	{  
		const string fileName = "RaidPrinter.lua";

		static void Main(string[] args)
		{
			Raid raid = new Raid();
			bool oldVer = true;

			XmlDocument doc = new XmlDocument();
			doc.Load("Oldformat.xml");
			XmlElement root = doc.DocumentElement;

			foreach (XmlNode node in root.ChildNodes)
			{
				bool leave = false;
				switch (node.Name)
				{
					#region key
					case "key":
						raid.key = Parsing.TimeToObj(oldVer, node.InnerText);
						break;
					#endregion

					#region start
					case "start":
						raid.start = Parsing.TimeToObj(oldVer, node.InnerText);
						break;
					#endregion

					#region end
					case "end":
						raid.end = Parsing.TimeToObj(oldVer, node.InnerText);
						break;
					#endregion

					#region zone
					case "zone":
						raid.zone = node.InnerText;

						break;
					#endregion

					#region note
					case "note":
						raid.note = node.InnerText;
						//node.Value;

						break;
					#endregion

					#region players
					case "PlayerInfos":
						//project skrz celý seznam klíčů a zařadit je do players listu, který je zatím prázdný
						foreach (XmlNode plrXml in node.ChildNodes)
						{
							Player plr = new Player();

							foreach (XmlNode plrPar in plrXml.ChildNodes)
							{
								switch (plrPar.Name)
								{
									case "name":
										plr.name = plrPar.InnerText;
										break;

									case "race":
										plr.race = plrPar.InnerText;
										break;

									case "guild":
										plr.guild = plrPar.InnerText;
										break;

									case "class":
										plr.cls = plrPar.InnerText;
										break;

									case "level":
										plr.level = plrPar.InnerText;
										break;
								}
							}

							raid.players.Add(plr);
						}
						break;
					#endregion

					#region kills
					case "BossKills":
						//project skrz celý seznam killů a zařadit je do event listu, který je zatím prázdný
						foreach (XmlNode killsXml in node.ChildNodes)
						{
							Kill kill = new Kill();

							foreach (XmlNode killXml in killsXml.ChildNodes)
							{
								switch (killXml.Name)
								{
									case "name":
										kill.name = killXml.InnerText;
										break;

									case "time":
										kill.time = Parsing.TimeToObj(oldVer, killXml.InnerText);
										break;
								}
							}

							raid.kills.Add(kill);
						}
						break;
					#endregion

					#region join
					case "Join":
						foreach (XmlNode joinXml in node.ChildNodes)
						{
							//projedeme parametry a zapísujme je do tempu
							string name = null;
							string timeStr = null;
							foreach (XmlNode joinPar in joinXml)
							{
								if (joinPar.Name == "player")
								{
									name = joinPar.InnerText;
								}
								else if (joinPar.Name == "time")
								{
									timeStr = joinPar.InnerText;
								}
							}
							// non-null check, možno odstranit, key bude vždy neco obsahovat
							if (name != null && timeStr != null)
							{
								//najdeme k jmenu odpovídající záznam v players
								foreach (Player player in raid.players)
								{
									if (name == player.name)
									{
										//pokud sedí
										if (leave) player.leaves.Add(Parsing.TimeToObj(oldVer, timeStr));
										else player.joins.Add(Parsing.TimeToObj(oldVer, timeStr));
										break;
									}
								}
							}
						}
						break;
					#endregion

					#region leave
					case "Leave":
						leave = true;
						goto case "Join";
					#endregion

					#region loot
					case "Loot":
						foreach (XmlNode lootXml in node.ChildNodes)
						{
							Item loot = new Item();

							foreach (XmlNode lootPar in lootXml)
							{
								switch (lootPar.Name)
								{
									case "ItemName":
										loot.itemName = lootPar.InnerText;
										break;

									case "ItemID":
										loot.itemID = lootPar.InnerText;
										break;

									case "Icon":
										loot.icon = lootPar.InnerText;
										break;

									case "Class":
										loot.itemCls = lootPar.InnerText;
										break;

									case "SubClass":
										loot.itemSubCls = lootPar.InnerText;
										break;

									case "Color":
										loot.color = lootPar.InnerText;
										break;

									case "Count":
										loot.count = lootPar.InnerText;
										break;

									case "Player":
										loot.player = lootPar.InnerText;
										break;

									case "Time":
										loot.timeString = lootPar.InnerText;
										loot.time = Parsing.TimeToObj(oldVer, lootPar.InnerText);
										break;

									case "Zone":
										loot.zone = lootPar.InnerText;
										break;

									case "Note":
										loot.note = lootPar.InnerText;
										break;
								}
							}
							raid.items.Add(loot);
						}
						break;
					#endregion
				}
			}
		}
	}

	/// <summary>
	/// Class with CT_RA XML output parsing methods
	/// </summary>
	static class Parsing
	{
		/// <summary>
		/// For translating string form time into timeObj
		/// </summary>
		/// <param name="old">true for oldformat=1; false for oldformat=0</param>
		/// <param name="timeStr">time in string format "MM/DD/YY HH:MM:SS"</param>
		/// <returns></returns>
		public static DateTime TimeToObj(bool old, string timeStr)
		{
			int[] dateArrStr = new int[3];
			int indexCh = 0;
			for (int i = 0; i < 3; i++)
			{
				int tmp = timeStr.IndexOf("/", indexCh + 1);
				if (tmp == -1)
				{
					dateArrStr[i] = Convert.ToInt32(timeStr.Substring(indexCh, 2));
					break;
				}
				dateArrStr[i] = Convert.ToInt32(timeStr.Substring(indexCh, tmp - indexCh));
				indexCh = timeStr.IndexOf("/", indexCh + 1) + 1;
			}
			int[] timeArrStr = new int[3];
			indexCh = timeStr.IndexOf(" ") + 1;
			for (int i = 0; i < 3; i++)
			{
				int tmp = timeStr.IndexOf(":", indexCh + 1);
				if (tmp == -1)
				{
					timeArrStr[i] = Convert.ToInt32(timeStr.Substring(indexCh, 2));
					break;
				}
				timeArrStr[i] = Convert.ToInt32(timeStr.Substring(indexCh, tmp - indexCh));
				indexCh = timeStr.IndexOf(":", indexCh + 1) + 1;
			}

			return new DateTime(2000 + dateArrStr[2], dateArrStr[0], dateArrStr[1], timeArrStr[0], timeArrStr[1], timeArrStr[2]);

		}

	}

	class Player
	{
		public string name;
		public string race;
		public string guild;
		public string cls;
		public string level;
		public string timeString;
		public DateTime time;
		public List<DateTime> joins = new List<DateTime>();
		public List<DateTime> leaves = new List<DateTime>();
		//public List<Kill> events;//doubled???

	}

	class Item
	{
		public string itemName;
		public string itemID;
		public string icon;
		public string itemCls;
		public string itemSubCls;
		public string color;
		public string count;
		public string player;
		public string timeString;
		public DateTime time;
		public string zone;
		public string boss;
		public string note;
	}

	class Kill
	{
		public string name;
		public DateTime time;
	}

	class Raid
	{
		public List<Player> players = new List<Player>();
		public List<Item> items = new List<Item>();
		public List<Kill> kills = new List<Kill>();
		public string zone;
		public string note;
		public DateTime key;
		public DateTime start;
		public DateTime end;
	}

}

//Changelog:
/*
 * Version 0 most of existing functions works now
 */