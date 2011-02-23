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
                     //TEMP: možna udělat víc objektově čistě, hlavne je to natvrdo
                     Player plr = new Player(raid);

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
                     Kill kill = new Kill(raid);

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
                     Item loot = new Item(raid);

                     foreach (XmlNode lootPar in lootXml)
                     {
                        #region switch
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
                              loot.playerStr = lootPar.InnerText;
                              break;

                           case "Cost":
                              loot.cost = Convert.ToInt32(lootPar.InnerText);
                              break;

                           case "Time":
                              loot.timeString = lootPar.InnerText;
                              loot.time = Parsing.TimeToObj(oldVer, lootPar.InnerText);
                              break;

                           case "Zone":
                              loot.zone = lootPar.InnerText;
                              break;

                           case "Boss":
                              loot.boss = lootPar.InnerText;
                              break;

                           case "Note":
                              loot.note = lootPar.InnerText;
                              break;
                        }
                        #endregion
                     }

                     //string to object
                     foreach (Player plr in raid.players)
                     {
                        if (plr.name == loot.playerStr) loot.player = plr;
                     }

                     raid.items.Add(loot);
                  }
                  break;
               #endregion
            }
         }         

         foreach (Player plr in raid.players) plr.GetPointsRelative(raid.start);
         foreach (Player plr in raid.players) plr.CheckEvents();

         //foreach (Player plr in raid.players)
         //{
         //   Console.WriteLine(plr.name);
         //   StringWriter sw = new StringWriter();
         //   Kill.PrintKills(plr.CheckEvents(), ref sw);
         //   sw.Flush();
         //   Console.Write(sw.ToString());
         //   sw.Dispose();
         //}

         //foreach (Kill kl in raid.kills)
         //{
         //   Console.WriteLine(kl.name);
         //   foreach(Player plr in kl.GetAttendees()) Console.WriteLine(plr.name);
         //}

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
         if (!old)
         {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dt.AddSeconds(Convert.ToDouble(timeStr));
            return dt;
         }

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
      private Raid defRaid;
      public string name;
      public string race;
      public string guild;
      public string cls;
      public string level;
      public string timeString;
      public DateTime time;
      //original
      public List<DateTime> joins = new List<DateTime>();
      public List<DateTime> leaves = new List<DateTime>();

      #region constructors
      public Player(Raid defRaid)
      {
         this.defRaid = defRaid;
      }
      #endregion

      public List<Kill> CheckEvents()
      {
         if (defRaid == null) return null;
         List<TimePoint> pointsAbsolute = GetPointsAbsolute();

         List<Kill> events = new List<Kill>();

         foreach (Kill kl in defRaid.kills)
         {
            for (int i = 0; i < pointsAbsolute.Count - 1; i++) //vzhledem ke stylu podmímky, se poslední čas netestuje, protože je uzavírací
            {
               if (kl.time >= pointsAbsolute[i].dt &&
                   kl.time <= pointsAbsolute[i + 1].dt &&
                   pointsAbsolute[i].join)
               {
                  events.Add(kl);
                  break;
               }
            }
         }

         return events;
      }

      public bool CheckEvent(Kill kl)
      {
         List<TimePoint> pointsAbsolute = GetPointsAbsolute();

         for (int i = 0; i < pointsAbsolute.Count - 1; i++) //vzhledem ke stylu podmímky, se poslední čas netestuje, protože je uzavírací
         {
            if (kl.time >= pointsAbsolute[i].dt &&
                kl.time <= pointsAbsolute[i + 1].dt &&
                pointsAbsolute[i].join)
            {
               return true;
            }
         }
         return false;
      }

      public List<TimePoint> GetPointsAbsolute()
      {
         List<TimePoint> sortedPoints = new List<TimePoint>();

         //joiny
         if (this.joins.Count + this.leaves.Count == 0) return null;
         foreach (DateTime dt in this.joins)
         {
            sortedPoints.Add(new TimePoint(dt, true));
         }
         //leavey
         foreach (DateTime dt in this.leaves)
         {
            sortedPoints.Add(new TimePoint(dt, false));
         }

         sortedPoints.Sort();

         return sortedPoints;
      }

      public List<TimePoint> GetPointsNull(DateTime dt)
      {
         List<TimePoint> sortedPoints = this.GetPointsAbsolute();

         for (int i = 0; i < sortedPoints.Count; i++)
         {
            sortedPoints[i] = new TimePoint(sortedPoints[i].dt.AddTicks(-dt.Ticks), sortedPoints[i].join);
         }

         return sortedPoints;
      }

      public List<TimePoint> GetPointsRelative(DateTime dt)
      {
         List<TimePoint> sortedPoints = this.GetPointsAbsolute();
         List<TimePoint> relativePoints = new List<TimePoint>(sortedPoints.Count);

         //spočítat 1. vztah
         relativePoints.Add(new TimePoint(
             sortedPoints[0].dt.AddTicks(-dt.Ticks),
             sortedPoints[0].join
             ));

         //spočítat vztahy
         for (int i = 1; i < sortedPoints.Count; i++)
         {
            relativePoints.Add(new TimePoint(
                sortedPoints[i].dt.AddTicks(-sortedPoints[i - 1].dt.Ticks),
                sortedPoints[i].join
                ));
         }
         return relativePoints;
      }

      public List<Item> GetItems()
      {
         List<Item> items = new List<Item>();
         foreach (Item item in defRaid.items)
         {
            if (item.player == this) items.Add(item);
         }
         return items;
      }

   }

   class Item
   {
      private Raid defRaid;
      public string itemName;
      public string itemID;
      public string icon;
      public string itemCls;
      public string itemSubCls;
      public string color;
      public string count;
      public string playerStr;
      public Player player;
      public int cost;
      public string timeString;
      public DateTime time;
      public string zone;
      public string boss;
      public string note;

      public Item(Raid raid)
      {
         this.defRaid = raid;
      }
   }

   class Kill
   {
      private Raid defRaid;
      public string name;
      public DateTime time;

      public Kill(Raid raid)
      {
         this.defRaid = raid;
      }

      public static void PrintKills(List<Kill> kills, ref StringWriter sw/* * DOPLNIT: REF NA VÝSTUP; FORMAT ZPRACOVANI * */)
      {
         for (int i = 0; i < kills.Count; i++)
         {
            Kill kl = kills[i];
            sw.WriteLine(/*i + 1 + "/" + kills.Count + " " + */kl.Print());
         }
      }

      public string Print(/*, FORMAT ZPRACOVANI*/)
      {
         return time.ToShortDateString() + " " + time.ToShortTimeString() + " " + name;
      }

      public List<Player> GetAttendees()
      {
         List<Player> att = new List<Player>();
         foreach (Player plr in defRaid.players)
         {
            if (plr.CheckEvent(this)) att.Add(plr);
         }
         return att;
      }
   }

   class Raid
   {
      private List<Raid> defRaidList;
      public List<Player> players = new List<Player>();
      public List<Item> items = new List<Item>();
      public List<Kill> kills = new List<Kill>();
      public string zone;
      public string note;
      public DateTime key;
      public DateTime start;
      public DateTime end;

      public TimeSpan GetLengthTS()
      {
         return end - start;
      }

      public DateTime GetLengthDT()
      {
         return end.AddTicks(-start.Ticks);
      }
   }

   struct TimePoint : IComparable<TimePoint>
   {
      public DateTime dt;
      public bool join;
      public TimePoint(DateTime dt, bool join)
      {
         this.dt = dt;
         this.join = join;
      }

      public int CompareTo(TimePoint atp)
      {
         return DateTime.Compare(this.dt, atp.dt);
      }
   }

}

//Changelog:
/*
 * Version 0 most of existing functions works now
 */

//TODO:
/*
 * Timepointy dát ven, celý system víc zobjekotvat.
 * Zbavit se lokálních stínu jako jako jsou v players a vše udělat jako return metod s jedním centrálním data skladem, pro snadnou updatabilitu.

 * každý objekt musí mít povině defRaid (=> nelze mít nulový konstruktor)
 * =>(2) zavést error hlásky při null parametru na defRaid
 * DONE

 * 
*/