using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace DKPparser
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

					#region killstodo
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

					#region TODO
					case "Join":

						break;

					case "Leave":

						break;

					case "Loot":


						break;
					#endregion

				}
			}
		}
	}

	static class Parsing
	{
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

			return new DateTime(dateArrStr[2], dateArrStr[0], dateArrStr[1], timeArrStr[0], timeArrStr[1], timeArrStr[2]);

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
		public List<DateTime> joins;
		public List<DateTime> leaves;
		//public List<Kill> events;//doubled???
	}

	class Item
	{
		public string itemName;
		public string itemID;
		public string icon;
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
