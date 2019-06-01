using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenRA.Mods.Common.FileFormats;

namespace OpenRAMissionGenConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var ini = new Ra95MissionIni();
            if (!ini.LoadFile("scu44ea.ini"))
            {
                Console.WriteLine("Ini Paser Error!");
            }
            ini.Gen();
            Console.ReadKey();
        }
    }


    class Ra95MissionIni
    {
        public static int ParseInt(String str)
        {
            if (String.IsNullOrWhiteSpace(str)) throw new Exception("Null Parse String");
            int ret = 0;
            try
            {
                ret = int.Parse(str);
            }
            catch(Exception)
            {
                if (str.Trim().ToLower() == "false") ret = 0;
                else if (str.Trim().ToLower() == "none") ret = 0;
                else if (str.Trim().ToLower() == "no") ret = 0;
                else if (str.Trim().ToLower() == "not") ret = 0;
                else if (str.Trim().ToLower() == "true") ret = 1;
                else if (str.Trim().ToLower() == "yes") ret = 1;
                else if (str.Trim().ToLower() == "ok") ret = 1;
            }
            

            return ret;
        }

        private String[] DefaultCountries =
        {
            "Spain",
            "Greece",
            "USSR",
            "England",
            "Ukraine",
            "Germany",
            "France",
            "Turkey",
            "GoodGuy",
            "BadGuy",
            "Neutral",
            "Special",
            "Multi1",
            "Multi2",
            "Multi3",
            "Multi4",
            "Multi5",
            "Multi6",
            "Multi7",
            "Multi8"
        };

        private String FileName = String.Empty;
        private List<IniBlock> iniBlocks = new List<IniBlock>();

        public List<Ra95MissionIni_CellTrigger> CellTriggers = new List<Ra95MissionIni_CellTrigger>();
        public List<Ra95MissionIni_Trigger> Triggers = new List<Ra95MissionIni_Trigger>();
        public List<Ra95MissionIni_TeamType> TeamTypes = new List<Ra95MissionIni_TeamType>();
        public List<Ra95MissionIni_Country> Countries = new List<Ra95MissionIni_Country>();
        public List<Ra95MissionIni_Waypoint> Waypoints = new List<Ra95MissionIni_Waypoint>();

        public List<Ra95MissionIni_Structure> Structures = new List<Ra95MissionIni_Structure>();
        public List<Ra95MissionIni_Unit> Units = new List<Ra95MissionIni_Unit>();
        public List<Ra95MissionIni_Ship> Ships = new List<Ra95MissionIni_Ship>();
        public List<Ra95MissionIni_Infantry> Infantries = new List<Ra95MissionIni_Infantry>();
        public List<Ra95MissionIni_Overlay> Overlays = new List<Ra95MissionIni_Overlay>();
        public List<Ra95MissionIni_Terrain> Terrains = new List<Ra95MissionIni_Terrain>();

        public Ra95MissionIni_Map Map = new Ra95MissionIni_Map();

        public Ra95MissionIni_Basic Basic = new Ra95MissionIni_Basic();

        public String Briefing = String.Empty;

        private bool LoadIniFile(String filename)
        {
            bool ret = false;
            try
            {
                String[] ContentArray = File.ReadAllLines(filename);
                List<String> ContentList = new List<string>();
                foreach (var line in ContentArray)
                {
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        ContentList.Add(line.Trim());
                        //Console.WriteLine(line);
                    }
                }
                ContentArray = ContentList.ToArray();
                IniBlock iniBlock = null;
                for (int lineindex = 0; lineindex < ContentArray.Length; lineindex++)
                {
                    if (String.IsNullOrWhiteSpace(ContentArray[lineindex])) continue;
                    var matchsection = Regex.Match(ContentArray[lineindex], @"(?<=^\[)([^\[^\]]+)(?=\]$)", RegexOptions.Multiline);
                    if (matchsection.Success)
                    {
                        if (iniBlock != null)
                        {
                            iniBlocks.Add(iniBlock);
                        }
                        iniBlock = new IniBlock();
                        iniBlock.section = matchsection.Value.Trim();
                        //Console.WriteLine(matchsection.Value.Trim());
                        continue;
                    }
                    var matchpair = Regex.Match(ContentArray[lineindex], @"^([^=]+)=(.+)$");
                    if (matchpair.Success)
                    {
                        string str = matchpair.Value.Trim();
                        int pos = str.IndexOf('=');
                        if (pos >= 0 && pos < str.Length)
                        {
                            String key = str.Substring(0, pos).Trim();
                            String val = str.Substring(pos + 1).Trim();
                            //Console.WriteLine("key:{0}  val:{1}",key, val);
                            var iniPair = new IniPair();
                            iniPair.key = key;
                            iniPair.value = val;
                            if (iniBlock != null) iniBlock.iniPairs.Add(iniPair);
                            continue;
                        }
                    }

                }
                if (iniBlock != null)
                {
                    iniBlocks.Add(iniBlock);
                }

                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }

        private bool ParseIniFile()
        {
            CellTriggers = new List<Ra95MissionIni_CellTrigger>();
            Triggers = new List<Ra95MissionIni_Trigger>();
            TeamTypes = new List<Ra95MissionIni_TeamType>();
            Countries = new List<Ra95MissionIni_Country>();
            Waypoints = new List<Ra95MissionIni_Waypoint>();

            Structures = new List<Ra95MissionIni_Structure>();
            Units = new List<Ra95MissionIni_Unit>();
            Ships = new List<Ra95MissionIni_Ship>();
            Infantries = new List<Ra95MissionIni_Infantry>();

            Terrains = new List<Ra95MissionIni_Terrain>();

            Map = new Ra95MissionIni_Map();

            Overlays = new List<Ra95MissionIni_Overlay>();

            Basic = new Ra95MissionIni_Basic();

            if (iniBlocks == null) return false;
            if (iniBlocks.Count == 0) return false;

            //scan countries
            foreach (var iniBlock in iniBlocks)
            {
                String section = iniBlock.section;
                if ((section == "England") ||
                    (section == "Germany") ||
                    (section == "France") ||
                    (section == "Greece") ||
                    (section == "Turkey") ||
                    (section == "Spain") ||
                    (section == "Ukraine") ||
                    (section == "USSR") ||
                    (section == "GoodGuy") ||
                    (section == "BadGuy") ||
                    (section == "Neutral") ||
                    (section == "Special") ||
                    (section == "Multi1") ||
                    (section == "Multi2") ||
                    (section == "Multi3") ||
                    (section == "Multi4") ||
                    (section == "Multi5") ||
                    (section == "Multi6") ||
                    (section == "Multi7") ||
                    (section == "Multi8"))
                {
                    var Country = new Ra95MissionIni_Country();
                    Country.Name = section;
                    foreach (var iniPair in iniBlock.iniPairs)
                    {
                        if (iniPair.key == "PlayerControl" && (iniPair.value == "yes" || iniPair.value == "Yes" || iniPair.value == "true" || iniPair.value == "True" || iniPair.value == "1")) Country.PlayerControl = true;
                        else if (iniPair.key == "Allies") Country.Allies = iniPair.value;
                    }
                    Countries.Add(Country);
                }
            }

            //scan triggers and teamtypes
            foreach (var iniBlock in iniBlocks)
            {
                if (iniBlock.section == "CellTriggers")
                {
                    var keyCellTriggerPairs = new Dictionary<string, Ra95MissionIni_CellTrigger>();
                    foreach (var CellTrigger in CellTriggers)
                    {
                        keyCellTriggerPairs[CellTrigger.Name] = CellTrigger;
                    }


                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        if (keyCellTriggerPairs.ContainsKey(inipair.value))
                        {
                            var cellTrigger = new Ra95MissionIni_CellTrigger();
                            if (keyCellTriggerPairs.TryGetValue(inipair.value, out cellTrigger))
                            {
                                var triggerpos = new Ra95MissionIni_Pos();
                                if (triggerpos.Parse(inipair.key))
                                {
                                    cellTrigger.TriggerPos.Add(triggerpos);
                                    keyCellTriggerPairs[inipair.value] = cellTrigger;
                                }
                            }
                        }
                        else
                        {
                            var cellTrigger = new Ra95MissionIni_CellTrigger();
                            var triggerpos = new Ra95MissionIni_Pos();
                            if (triggerpos.Parse(inipair.key))
                            {
                                cellTrigger.Name = inipair.value;
                                cellTrigger.TriggerPos.Add(triggerpos);

                            }
                            keyCellTriggerPairs.Add(inipair.value, cellTrigger);
                        }

                    }

                    CellTriggers = keyCellTriggerPairs.Values.ToList();

                }
                else if (iniBlock.section == "Trigs")
                {
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        var trigger = new Ra95MissionIni_Trigger();
                        if (trigger.Parse(inipair.key, inipair.value))
                        {
                            Triggers.Add(trigger);
                        }
                    }

                }
                else if (iniBlock.section == "TeamTypes")
                {
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        var teantype = new Ra95MissionIni_TeamType();
                        if (teantype.Parse(inipair.key, inipair.value))
                        {
                            TeamTypes.Add(teantype);
                        }
                    }

                }
                else if (iniBlock.section == "Waypoints")
                {
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        var waypoint = new Ra95MissionIni_Waypoint();
                        waypoint.Num = int.Parse(inipair.key);
                        if (waypoint.Pos.Parse(inipair.value))
                        {
                            Waypoints.Add(waypoint);
                        }
                    }
                }
                else if (iniBlock.section == "Briefing")
                {
                    Briefing = String.Empty;
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        Briefing += inipair.value + " ";
                    }
                }
                else if (iniBlock.section == "STRUCTURES")
                {
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        var Structure = new Ra95MissionIni_Structure();
                        if(Structure.Parse(inipair.key, inipair.value))
                        {
                            Structures.Add(Structure);
                        }
                    }
                }
                else if (iniBlock.section == "SHIPS")
                {
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        var Ship = new Ra95MissionIni_Ship();
                        if (Ship.Parse(inipair.key, inipair.value))
                        {
                            Ships.Add(Ship);
                        }
                    }
                }
                else if (iniBlock.section == "UNITS")
                {
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        var Unit = new Ra95MissionIni_Unit();
                        if (Unit.Parse(inipair.key, inipair.value))
                        {
                            Units.Add(Unit);
                        }
                    }
                }
                else if (iniBlock.section == "INFANTRY")
                {
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        var Infantry = new Ra95MissionIni_Infantry();
                        if (Infantry.Parse(inipair.key, inipair.value))
                        {
                            Infantries.Add(Infantry);
                        }
                    }
                }
                else if (iniBlock.section == "Basic")
                {
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        /*
                        Name=Shock Therapy
                        Intro=<none>
                        Brief=<none>
                        Win=SOVTSTAR
                        Lose=SOVCEMET
                        Action=<none>
                        Player=USSR
                        Theme=No theme
                        CarryOverMoney=0
                        ToCarryOver=no
                        ToInherit=no
                        TimerInherit=no
                        CivEvac=no
                        NewINIFormat=3
                        CarryOverCap=0
                        EndOfGame=no
                        NoSpyPlane=yes
                        SkipScore=no
                        OneTimeOnly=yes
                        SkipMapSelect=no
                        Official=yes
                        FillSilos=yes
                        TruckCrate=no
                        Percent=0
                         */
                        if (inipair.key == "Name") Basic.Name = inipair.value;
                        else if (inipair.key == "Intro") Basic.Intro = inipair.value;
                        else if (inipair.key == "Brief") Basic.Brief = inipair.value;
                        else if (inipair.key == "Win") Basic.Win = inipair.value;
                        else if (inipair.key == "Lose") Basic.Lose = inipair.value;
                        else if (inipair.key == "Action") Basic.Action = inipair.value;
                        else if (inipair.key == "Player") Basic.Player = inipair.value;
                        else if (inipair.key == "Theme") Basic.Theme = inipair.value;
                        else if (inipair.key == "CarryOverMoney") Basic.CarryOverMoney = inipair.value;
                        else if (inipair.key == "ToCarryOver") Basic.ToCarryOver = inipair.value;
                        else if (inipair.key == "ToInherit") Basic.ToInherit = inipair.value;
                        else if (inipair.key == "TimerInherit") Basic.TimerInherit = inipair.value;
                        else if (inipair.key == "CivEvac") Basic.CivEvac = inipair.value;
                        else if (inipair.key == "NewINIFormat") Basic.NewINIFormat = inipair.value;
                        else if (inipair.key == "CarryOverCap") Basic.CarryOverCap = inipair.value;
                        else if (inipair.key == "EndOfGame") Basic.EndOfGame = inipair.value;
                        else if (inipair.key == "NoSpyPlane") Basic.NoSpyPlane = inipair.value;
                        else if (inipair.key == "SkipScore") Basic.SkipScore = inipair.value;
                        else if (inipair.key == "OneTimeOnly") Basic.OneTimeOnly = inipair.value;
                        else if (inipair.key == "SkipMapSelect") Basic.SkipMapSelect = inipair.value;
                        else if (inipair.key == "Official") Basic.Official = inipair.value;
                        else if (inipair.key == "FillSilos") Basic.FillSilos = inipair.value;
                        else if (inipair.key == "TruckCrate") Basic.TruckCrate = inipair.value;
                        else if (inipair.key == "Percent") Basic.Percent = inipair.value;
                    }
                }
                else if (iniBlock.section == "Map")
                {
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        if (inipair.key == "Theater")
                        {
                            if(inipair.value.ToUpper()== "TEMPERATE") Map.Theater = "TEMPERAT";
                            else Map.Theater = inipair.value;
                        }
                        else if (inipair.key == "X") Map.X = int.Parse(inipair.value);
                        else if (inipair.key == "Y") Map.Y = int.Parse(inipair.value);
                        else if (inipair.key == "Width") Map.Width = int.Parse(inipair.value);
                        else if (inipair.key == "Height") Map.Height = int.Parse(inipair.value);
                    }
                }
                else if (iniBlock.section == "TERRAIN")
                {
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        var Terrain = new Ra95MissionIni_Terrain();
                        Terrain.Terrain = inipair.value;
                        Terrain.Pos.Parse(inipair.key);
                        Terrains.Add(Terrain);

                        //Console.WriteLine("Terrain:{0} X:{1} Y:{2}", Terrain.Terrain, Terrain.Pos.x, Terrain.Pos.y);
                    }
                }
                else if (iniBlock.section == "OverlayPack")
                {
                    String Base64Str = String.Empty;
                    foreach (var inipair in iniBlock.iniPairs)
                    {
                        Base64Str += inipair.value;
                    }

                    var data = Convert.FromBase64String(Base64Str);
                    var chunks = new List<byte[]>();
                    var reader = new BinaryReader(new MemoryStream(data));

                    try
                    {
                        while (true)
                        {
                            var length = reader.ReadUInt32() & 0xdfffffff;
                            var dest = new byte[8192];
                            var src = reader.ReadBytes((int)length);

                            LCWCompression.DecodeInto(src, dest);

                            chunks.Add(dest);
                        }
                    }
                    catch (EndOfStreamException) { }

                    var ms = new MemoryStream();
                    foreach (var chunk in chunks)
                        ms.Write(chunk, 0, chunk.Length);
                    ms.Position = 0;

                    const int mapsize = 128;
                    for (int y = 0; y < mapsize; y++)
                    {
                        for (int x = 0; x < mapsize; x++)
                        {
                            var o = ms.ReadByte();
                            if (Ra95MissionIni_Overlay.IsOverlay(o))
                            {
                                var Overlay = new Ra95MissionIni_Overlay();
                                Overlay.Overlay = Ra95MissionIni_Overlay.GetOverlayName(o);
                                Overlay.Pos.x = x;
                                Overlay.Pos.y = y;

                                if(!Overlay.IsOre()) Overlays.Add(Overlay);
                                //Console.WriteLine("Overlay:{0} X:{1} Y:{2}", Ra95MissionIni_Overlay.GetOverlayName(o), x, y);
                            }
                        }
                    }


                }

            }
            
            /*
                Console.WriteLine("Countries Cnt:{0}", Countries.Count);
                Console.WriteLine("CellTriggers Cnt:{0}", CellTriggers.Count);
                Console.WriteLine("Triggers Cnt:{0}", Triggers.Count);
                Console.WriteLine("TeamTypes Cnt:{0}", TeamTypes.Count);
                Console.WriteLine("Waypoints Cnt:{0}", Waypoints.Count);
                Console.WriteLine("Briefing:{0}", Briefing);
                Console.WriteLine("Structures Cnt:{0}", Structures.Count);
                Console.WriteLine("Ships Cnt:{0}", Ships.Count);
                Console.WriteLine("Units Cnt:{0}", Units.Count);
                Console.WriteLine("Infantries Cnt:{0}", Infantries.Count);
                */
            return true;
        }

        private bool IsCelltriggerExist(String CellTriggerName)
        {
            bool ret = false;
            foreach (var CellTrigger in CellTriggers)
            {
                if (CellTrigger.Name == CellTriggerName)
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        private bool GetCelltrigger(String CellTriggerName,out Ra95MissionIni_CellTrigger CellTrigger)
        {
            CellTrigger = null;
            bool ret = false;
            foreach (var c in CellTriggers)
            {
                if (c.Name == CellTriggerName)
                {
                    CellTrigger = c;
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        private bool GetWayPoint(int Num,out Ra95MissionIni_Waypoint Waypoint)
        {
            bool ret = false;
            Waypoint = null;
            foreach (var w in Waypoints)
            {
                if(w.Num==Num)
                {
                    Waypoint = w;
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        private bool IsWayPointExist(int Num)
        {
            bool ret = false;
            foreach (var w in Waypoints)
            {
                if (w.Num == Num)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        public bool LoadFile(String filename)
        {
            FileName = System.IO.Path.GetFileNameWithoutExtension(filename);
            bool ret = false;
            ret = LoadIniFile(filename);
            if (!ret) return false;
            ret = ParseIniFile();
            return ret;
        }

        public bool Gen()
        {
            return GenMapYaml() && GenRuleYaml() && GenLua();
        }

        public bool GenMapYaml()
        {
            StreamWriter sw_mapyaml = new StreamWriter("map.yaml", false, Encoding.UTF8);

            sw_mapyaml.Write("MapFormat: 11\n");
            sw_mapyaml.Write("RequiresMod: ra\n");
            sw_mapyaml.Write("Title: {0}\n",Basic.Name);
            sw_mapyaml.Write("Author: Westwood Studios\n");
            sw_mapyaml.Write("Tileset: {0}\n", Map.Theater.ToUpper());//Tileset: TEMPERAT
            sw_mapyaml.Write("MapSize: 128,128\n");
            sw_mapyaml.Write("Bounds: {0},{1},{2},{3}\n",Map.X,Map.Y,Map.Width,Map.Height);//Bounds: 26,32,73,67
            sw_mapyaml.Write("Visibility: MissionSelector\n");
            sw_mapyaml.Write("Categories: Campaign\n");
            sw_mapyaml.Write("LockPreview: True\n");

            sw_mapyaml.Write("Players:\n");

            foreach(String DefaultCountry in DefaultCountries)
            {
                bool found = false;
                foreach (var Country in Countries)
                {
                    if(Country.Name.ToLower()== DefaultCountry.ToLower())
                    {
                        sw_mapyaml.Write("\tPlayerReference@{0}:\n", Country.Name);
                        sw_mapyaml.Write("\t\tName: {0}\n", Country.Name);
                        if(Country.Name.ToLower()== "Neutral".ToLower()) sw_mapyaml.Write("\t\tOwnsWorld: True\n");
                        sw_mapyaml.Write("\t\tFaction: {0}\n", Country.GetFaction());
                        sw_mapyaml.Write("\t\tColor: {0}\n", Country.GetColor());
                        if (Country.PlayerControl || Country.Name.ToLower() == Basic.Player.ToLower())
                        {
                            sw_mapyaml.Write("\t\tPlayable: True\n");
                        }

                        if (!String.IsNullOrWhiteSpace(Country.Allies)) sw_mapyaml.Write("\t\tAllies: {0}\n", Country.Allies);
                        if(!String.IsNullOrWhiteSpace(Country.GetEnemies())) sw_mapyaml.Write("\t\tEnemies: {0}\n", Country.GetEnemies());

                        found = true;
                        break;
                    }
                }
                if(!found)
                {
                    sw_mapyaml.Write("\tPlayerReference@{0}:\n", DefaultCountry);
                    sw_mapyaml.Write("\t\tName: {0}\n", DefaultCountry);
                    sw_mapyaml.Write("\t\tFaction: allies\n");
                    sw_mapyaml.Write("\t\tColor: FFFFFF\n");
                }
            }
            
            sw_mapyaml.Write("\nActors:\n");
            for(int i=0;i<Overlays.Count;i++)
            {
                sw_mapyaml.Write("\tActor_Overlay_{0}: {1}\n", i,Overlays[i].Overlay.ToLower());
                sw_mapyaml.Write("\t\tLocation: {0},{1}\n", Overlays[i].Pos.x, Overlays[i].Pos.y);
                sw_mapyaml.Write("\t\tOwner: Neutral\n");
            }
            for (int i = 0; i < Terrains.Count; i++)
            {
                sw_mapyaml.Write("\tActor_Terrain_{0}: {1}\n", i, Terrains[i].Terrain.ToLower());
                sw_mapyaml.Write("\t\tLocation: {0},{1}\n", Terrains[i].Pos.x, Terrains[i].Pos.y);
                sw_mapyaml.Write("\t\tOwner: Neutral\n");
            }
            for (int i = 0; i < Structures.Count; i++)
            {
                sw_mapyaml.Write("\tActor_Structure_{0}: {1}\n", Structures[i].index, Structures[i].Structure.ToLower());
                sw_mapyaml.Write("\t\tLocation: {0},{1}\n", Structures[i].Pos.x, Structures[i].Pos.y);
                if (Structures[i].Structure.ToLower() == "v19") sw_mapyaml.Write("\t\tOwner: Neutral\n");
                else sw_mapyaml.Write("\t\tOwner: {0}\n", Structures[i].Owner);
                sw_mapyaml.Write("\t\tHealth: {0}\n", Structures[i].Health * 100 / 256);
            }


            for (int i = 0; i < Units.Count; i++)
            {
                sw_mapyaml.Write("\tActor_Unit_{0}: {1}\n", Units[i].index, Units[i].Unit.ToLower());
                sw_mapyaml.Write("\t\tLocation: {0},{1}\n", Units[i].Pos.x, Units[i].Pos.y);
                sw_mapyaml.Write("\t\tOwner: {0}\n", Units[i].Owner);
                sw_mapyaml.Write("\t\tFacing: {0}\n", Units[i].Direction);
                sw_mapyaml.Write("\t\tHealth: {0}\n", Units[i].Health*100/256);
            }
            for (int i = 0; i < Ships.Count; i++)
            {
                sw_mapyaml.Write("\tActor_Ship_{0}: {1}\n", Ships[i].index, Ships[i].Ship.ToLower());
                sw_mapyaml.Write("\t\tLocation: {0},{1}\n", Ships[i].Pos.x, Ships[i].Pos.y);
                sw_mapyaml.Write("\t\tOwner: {0}\n", Ships[i].Owner);
                sw_mapyaml.Write("\t\tFacing: {0}\n", Ships[i].Direction);
                sw_mapyaml.Write("\t\tHealth: {0}\n", Ships[i].Health * 100 / 256);
            }
            for (int i = 0; i < Infantries.Count; i++)
            {
                sw_mapyaml.Write("\tActor_Infantry_{0}: {1}\n", Infantries[i].index, Infantries[i].Infantry.ToLower());
                sw_mapyaml.Write("\t\tLocation: {0},{1}\n", Infantries[i].Pos.x, Infantries[i].Pos.y);
                sw_mapyaml.Write("\t\tOwner: {0}\n", Infantries[i].Owner);
                sw_mapyaml.Write("\t\tFacing: {0}\n", Infantries[i].Direction);
                sw_mapyaml.Write("\t\tSubCell: {0}\n", Infantries[i].PositionInCell);
                sw_mapyaml.Write("\t\tHealth: {0}\n", Infantries[i].Health * 100 / 256);
            }
            
            sw_mapyaml.Write("\nRules: ra|rules/campaign-rules.yaml, ra|rules/campaign-tooltips.yaml, ra|rules/campaign-palettes.yaml, rules.yaml\n");
            
            sw_mapyaml.Flush();
            sw_mapyaml.Close();
            return true;
        }

        public bool GenRuleYaml()
        {
            bool ret = true;
            return ret;
        }
        
        public bool GenLua()
        {
            StreamWriter sw_lua = new StreamWriter(FileName+".lua", false, Encoding.UTF8);

            sw_lua.Write("--[[\nThis File Is Generated By The Tool Of Tuo Qiang...\n]]\n\n");


            foreach (var CellTrigger in CellTriggers)
            {
                GenLua_CellTriggerDeclaration(sw_lua, CellTrigger);
            }


            sw_lua.Write("\n\n");

            foreach(var Waypoint in Waypoints)
            {
                sw_lua.Write("Waypoint_"+ Waypoint.Num.ToString()+ " =  {CPos.New(" + Waypoint.Pos.x.ToString() + ", " + Waypoint.Pos.y.ToString() + ")}\n");
            }

            sw_lua.Write("\n\n");

            
            foreach (var TeamType in TeamTypes)
            {
                GenLua_TeamTypeDeclaration(sw_lua, TeamType);
                GenLua_TeamTypeFunctions(sw_lua, TeamType);
            }

            sw_lua.Write("\n\n");

            foreach (var Trigger in Triggers)
            {
                GenLua_TriggerAction(sw_lua, Trigger, 1);
                GenLua_TriggerAction(sw_lua, Trigger, 2);
                GenLua_TriggerLogic(sw_lua, Trigger);
                GenLua_TriggerEventSetup(sw_lua, Trigger, 1);
                if(Trigger.Type== (int)TriggerType.and|| Trigger.Type == (int)TriggerType.or|| Trigger.Type == (int)TriggerType.complex)
                {
                    GenLua_TriggerEventSetup(sw_lua, Trigger, 2);
                }
            }

            sw_lua.Write("\n\n");

            sw_lua.Write("WorldLoaded = function()\n");
            foreach(var Country in DefaultCountries)
            {
                sw_lua.Write("\t{0} = Player.GetPlayer(\"{1}\")\n", Country.ToLower(), Country);
            }


            String PlayerControlCountry = "ussr";
            foreach (var Country in Countries)
            {
                if (Country.PlayerControl)
                {
                    PlayerControlCountry = Country.Name.ToLower();
                    break;
                }
            }

            sw_lua.Write("\n\tTrigger.OnObjectiveAdded({0}, function(p, id)\n", PlayerControlCountry);
            sw_lua.Write("\t\tMedia.DisplayMessage(p.GetObjectiveDescription(id), \"New \" .. string.lower(p.GetObjectiveType(id)) .. \" objective\")\n");
            sw_lua.Write("\tend)\n");

            sw_lua.Write("\n\tTrigger.OnObjectiveCompleted({0}, function(p, id)\n", PlayerControlCountry);
            sw_lua.Write("\t\tMedia.DisplayMessage(p.GetObjectiveDescription(id), \"Objective completed\")\n");
            sw_lua.Write("\tend)\n");

            sw_lua.Write("\n\tTrigger.OnObjectiveFailed({0}, function(p, id)\n", PlayerControlCountry);
            sw_lua.Write("\t\tMedia.DisplayMessage(p.GetObjectiveDescription(id), \"Objective failed\")\n");
            sw_lua.Write("\tend)\n");

            sw_lua.Write("\n\tMissionObjective = {0}.AddPrimaryObjective(\"{1}\")\n\n", PlayerControlCountry, Briefing);

            sw_lua.Write("\n\tTrigger.OnPlayerWon({0}, function()\n", PlayerControlCountry);
            sw_lua.Write("\t\tMedia.PlaySpeechNotification({0}, \"MissionAccomplished\")\n", PlayerControlCountry);
            sw_lua.Write("\tend)\n");

            sw_lua.Write("\n\tTrigger.OnPlayerLost({0}, function()\n", PlayerControlCountry);
            sw_lua.Write("\t\tMedia.PlaySpeechNotification({0}, \"MissionFailed\")\n", PlayerControlCountry);
            sw_lua.Write("\tend)\n\n");


            foreach (var Trigger in Triggers)
            {
                sw_lua.Write("\tTrigger_{0}_Event{1}_Setup()\n", Trigger.Name, 1);
                if (Trigger.Type == (int)TriggerType.and || Trigger.Type == (int)TriggerType.or || Trigger.Type == (int)TriggerType.complex)
                {
                    sw_lua.Write("\tTrigger_{0}_Event{1}_Setup()\n", Trigger.Name, 2);
                }
            }

            sw_lua.Write("end\n\n");


            
            sw_lua.Write("Tick = function()\n");
            foreach (var Trigger in Triggers)
            {
                sw_lua.Write("\tTrigger_{0}_Event1_Tick()\n", Trigger.Name);
                if (Trigger.Type == (int)TriggerType.and || Trigger.Type == (int)TriggerType.or || Trigger.Type == (int)TriggerType.complex)
                {
                    sw_lua.Write("\tTrigger_{0}_Event2_Tick()\n", Trigger.Name);
                }
            }
            sw_lua.Write("end\n\n");


            sw_lua.Flush();
            sw_lua.Close();
            
            return true;
        }

        private void GenLua_CellTriggerDeclaration(StreamWriter sw_lua, Ra95MissionIni_CellTrigger CellTrigger)
        {
            String celltrigerstr = "CellTrigger_" + CellTrigger.Name + "={";

            for (int i = 0; i < CellTrigger.TriggerPos.Count; i++)
            {
                celltrigerstr += "CPos.New(" + CellTrigger.TriggerPos[i].x.ToString() + "," + CellTrigger.TriggerPos[i].y.ToString() + ")";
                if (i < CellTrigger.TriggerPos.Count - 1) celltrigerstr += ",";
            }
            celltrigerstr += "}\n";
            sw_lua.Write(celltrigerstr);
        }
        
        private void GenLua_TeamTypeFunctions(StreamWriter sw_lua, Ra95MissionIni_TeamType TeamType)
        {
            //Create Team Function
            if (TeamType.Opt_Only_Autocreate_AI)
            {
                sw_lua.Write("TeamType_{0}_Produce = function()\n", TeamType.Name);
                sw_lua.Write("\tOrderIndex_{0}=0\n", TeamType.Name);
                sw_lua.Write("\t{0}.Build(TeamType_{1}, function(tTeamInstance_{1})\n", DefaultCountries[TeamType.Owner].ToLower(), TeamType.Name);
                sw_lua.Write("\t\tUtils.Do(tTeamInstance_{0}, function(Act)\n", TeamType.Name);
                sw_lua.Write("\t\t\tTrigger.OnIdle(Act, function()\n");
                if (TeamType.Orders.Count > 0)
                {
                    bool first_order = true;
                    for (int OrderIndex = 0; OrderIndex < TeamType.Orders.Count; OrderIndex++)
                    {
                        var Order = TeamType.Orders[OrderIndex];
                        if (first_order)
                        {
                            first_order = false;
                            sw_lua.Write("\t\t\t\tif OrderIndex_{0} == {1} then\n", TeamType.Name, OrderIndex);
                        }
                        else
                        {
                            sw_lua.Write("\t\t\t\telseif OrderIndex_{0} == {1} then\n", TeamType.Name, OrderIndex);
                        }
                        sw_lua.Write("\t\t\t\t\t--Order:{0}\n", ((TeamOrderType)Order.Order).ToString());

                        if (Order.Order == TeamOrderType.Move_to_waypoint)
                        {
                            var w = new Ra95MissionIni_Waypoint();
                            GetWayPoint(Order.Para, out w);
                            sw_lua.Write("\t\t\t\t\tAct.Move(CPos.New({0}, {1}))\n", w.Pos.x, w.Pos.y);
                        }
                        else if (Order.Order == TeamOrderType.Attack_Waypoint)
                        {
                            var w = new Ra95MissionIni_Waypoint();
                            GetWayPoint(Order.Para, out w);
                            sw_lua.Write("\t\t\t\t\tAct.AttackMove(CPos.New({0}, {1}))\n", w.Pos.x, w.Pos.y);
                        }
                        else
                        {
                            sw_lua.Write("\t\t\t\t\tTrigger.ClearAll(Act)\n");
                        }
                    }
                    sw_lua.Write("\t\t\t\telse\n");
                    sw_lua.Write("\t\t\t\t\t--OrderCnt:{0}\n", TeamType.Orders.Count);
                    sw_lua.Write("\t\t\t\t\tTrigger.ClearAll(Act)\n");
                    sw_lua.Write("\t\t\t\tend\n");
                }
                else
                {
                    sw_lua.Write("\t\t\t\tTrigger.ClearAll(Act)\n");
                }
                sw_lua.Write("\t\t\t\tOrderIndex_{0}=OrderIndex_{0}+1\n", TeamType.Name);
                sw_lua.Write("\t\t\tend)\n");
                sw_lua.Write("\t\tend)\n");
                sw_lua.Write("\tend)\n");
                sw_lua.Write("end\n\n");
            }

            else
            //ReinForce function
            {
                sw_lua.Write("TeamType_{0}_Reinforce = function()\n", TeamType.Name);
                sw_lua.Write("\tOrderIndex_{0}=0\n", TeamType.Name);
                
                if (TeamType.HasUnit("badr")&& TeamType.Teams.Count>=2)//paratroop
                {
                    var Teams = TeamType.GetTeamsWithExceptionFilter("badr");
                    String teamstr = "\tlocal Local_TeamType_" + TeamType.Name + "={";
                    for (int teamindex = 0; teamindex < Teams.Count; teamindex++)
                    {
                        for (int numi = 0; numi < Teams[teamindex].Num; numi++)
                        {
                            teamstr += "\"" + Teams[teamindex].Type.ToLower() + "\"";
                            if (teamindex >= Teams.Count - 1 && numi >= Teams[teamindex].Num - 1) ;
                            else teamstr += ",";
                        }
                    }
                    teamstr += "}\n";
                    sw_lua.Write(teamstr);

                    if (TeamType.Orders.Count >= 1)
                    {
                        if (TeamType.Orders[0].Order == TeamOrderType.Attack_Waypoint)
                        {
                            var WaypointEntry = new Ra95MissionIni_Waypoint();
                            var WaypointUnload = new Ra95MissionIni_Waypoint();

                            GetWayPoint(TeamType.WayPoint, out WaypointEntry);
                            GetWayPoint(TeamType.Orders[0].Para, out WaypointUnload);

                            if (WaypointUnload == null && WaypointEntry == null)
                            {
                                WaypointEntry = new Ra95MissionIni_Waypoint();
                                WaypointUnload = new Ra95MissionIni_Waypoint();
                            }
                            else if (WaypointEntry == null)
                            {
                                WaypointEntry = new Ra95MissionIni_Waypoint();
                                WaypointEntry.Pos.x = 0;
                                WaypointEntry.Pos.y = 0;
                            }
                            else if (WaypointUnload == null)
                            {
                                WaypointUnload = WaypointEntry.Clone();
                                WaypointEntry = new Ra95MissionIni_Waypoint();
                                WaypointEntry.Pos.x = 0;
                                WaypointEntry.Pos.y = 0;
                            }

                            sw_lua.Write("\tlocal Local_EntryPoint=CPos.New({0}, {1})\n", WaypointEntry.Pos.x, WaypointEntry.Pos.y);
                            sw_lua.Write("\tlocal Local_UnloadPoint=CPos.New({0}, {1})\n", WaypointUnload.Pos.x, WaypointUnload.Pos.y);
                            sw_lua.Write("\tlocal Local_ExitPoint=CPos.New(0, 0)\n");

                            
                            sw_lua.Write("\tTeamInstance_{0}=Reinforcements.ReinforceWithTransport({1},\"badr\", Local_TeamType_{0},{{Local_EntryPoint,Local_UnloadPoint}}, {{Local_ExitPoint}})\n",
                                TeamType.Name,
                                DefaultCountries[TeamType.Owner].ToLower());
                                

                            //sw_lua.Write("\tUtils.Do(TeamInstance_{0}, function(Act)\n", TeamType.Name);
                            //sw_lua.Write("\t\tTrigger.OnIdle(Act, function()\n");

                        }
                    }
                }
                else if (TeamType.HasUnit("tran") && TeamType.Teams.Count >= 2)//unload from heli
                {
                    var Teams = TeamType.GetTeamsWithExceptionFilter("tran");
                    String teamstr = "\tlocal Local_TeamType_" + TeamType.Name + "={";
                    for (int teamindex = 0; teamindex < Teams.Count; teamindex++)
                    {
                        for (int numi = 0; numi < Teams[teamindex].Num; numi++)
                        {
                            teamstr += "\"" + Teams[teamindex].Type.ToLower() + "\"";
                            if (teamindex >= Teams.Count - 1 && numi >= Teams[teamindex].Num - 1) ;
                            else teamstr += ",";
                        }
                    }
                    teamstr += "}\n";
                    sw_lua.Write(teamstr);

                    if (TeamType.Orders.Count >= 2)
                    {
                        if (TeamType.Orders[0].Order == TeamOrderType.Move_to_waypoint && TeamType.Orders[1].Order == TeamOrderType.Unload)
                        {
                            var WaypointEntry = new Ra95MissionIni_Waypoint();
                            var WaypointUnload = new Ra95MissionIni_Waypoint();


                            GetWayPoint(TeamType.WayPoint, out WaypointEntry);
                            GetWayPoint(TeamType.Orders[0].Para, out WaypointUnload);

                            if (WaypointUnload == null && WaypointEntry == null)
                            {
                                WaypointEntry = new Ra95MissionIni_Waypoint();
                                WaypointUnload = new Ra95MissionIni_Waypoint();
                            }
                            else if (WaypointEntry == null)
                            {
                                WaypointEntry = WaypointUnload.Clone();
                            }
                            else if (WaypointUnload == null)
                            {
                                WaypointUnload = WaypointEntry.Clone();
                            }

                            sw_lua.Write("\tlocal Local_EntryPoint=CPos.New({0}, {1})\n", WaypointEntry.Pos.x, WaypointEntry.Pos.y);
                            sw_lua.Write("\tlocal Local_UnloadPoint=CPos.New({0}, {1})\n", WaypointUnload.Pos.x, WaypointUnload.Pos.y);
                            sw_lua.Write("\tlocal Local_ExitPoint=CPos.New(0, 0)\n");


                            sw_lua.Write("\tTeamInstance_{0}=Reinforcements.ReinforceWithTransport({1},\"tran\", Local_TeamType_{0},{{Local_EntryPoint,Local_UnloadPoint}}, {{Local_ExitPoint}})\n",
                                TeamType.Name,
                                DefaultCountries[TeamType.Owner].ToLower());

                            //sw_lua.Write("\tUtils.Do(TeamInstance_{0}, function(Act)\n", TeamType.Name);
                            //sw_lua.Write("\t\tTrigger.OnIdle(Act, function()\n");

                        }
                    }
                }
                else if (TeamType.HasUnit("lst") && TeamType.Teams.Count >= 2)//unload
                {
                    var Teams = TeamType.GetTeamsWithExceptionFilter("lst");
                    String teamstr = "\tlocal Local_TeamType_" + TeamType.Name + "={";
                    for (int teamindex = 0; teamindex < Teams.Count; teamindex++)
                    {
                        for (int numi = 0; numi < Teams[teamindex].Num; numi++)
                        {
                            teamstr += "\"" + Teams[teamindex].Type.ToLower() + "\"";
                            if(! (teamindex >= Teams.Count - 1 && numi >= Teams[teamindex].Num - 1)) teamstr += ",";
                        }
                    }
                    teamstr += "}\n";
                    sw_lua.Write(teamstr);

                    if (TeamType.Orders.Count >= 2)
                    {
                        if (TeamType.Orders[0].Order == TeamOrderType.Move_to_waypoint && TeamType.Orders[1].Order == TeamOrderType.Unload)
                        {
                            var WaypointEntry = new Ra95MissionIni_Waypoint();
                            var WaypointUnload = new Ra95MissionIni_Waypoint();


                            GetWayPoint(TeamType.WayPoint, out WaypointEntry);
                            GetWayPoint(TeamType.Orders[0].Para, out WaypointUnload);

                            if (WaypointUnload == null && WaypointEntry == null)
                            {
                                WaypointEntry = new Ra95MissionIni_Waypoint();
                                WaypointUnload = new Ra95MissionIni_Waypoint();
                            }
                            else if (WaypointEntry == null)
                            {
                                WaypointEntry = WaypointUnload.Clone();
                            }
                            else if (WaypointUnload == null)
                            {
                                WaypointUnload = WaypointEntry.Clone();
                            }

                            sw_lua.Write("\tlocal Local_EntryPoint=CPos.New({0}, {1})\n",WaypointEntry.Pos.x, WaypointEntry.Pos.y);
                            sw_lua.Write("\tlocal Local_UnloadPoint=CPos.New({0}, {1})\n", WaypointUnload.Pos.x, WaypointUnload.Pos.y);
                            sw_lua.Write("\tlocal Local_ExitPoint=CPos.New(0,0)\n");

                            
                            sw_lua.Write("\tTeamInstance_{0}=Reinforcements.ReinforceWithTransport({1},\"lst\", Local_TeamType_{0},{{Local_EntryPoint,WaypointUnload}}, {{Local_ExitPoint}})\n", 
                                TeamType.Name, 
                                DefaultCountries[TeamType.Owner].ToLower());
                                
                            //sw_lua.Write("\tUtils.Do(TeamInstance_{0}, function(Act)\n", TeamType.Name);
                            //sw_lua.Write("\t\tTrigger.OnIdle(Act, function()\n");

                        }
                    }
                }
                else if (TeamType.HasUnit("badr") && TeamType.Teams.Count == 1)//para bomb
                {

                }
                else
                {
                    var Waypoint = new Ra95MissionIni_Waypoint();
                    if (GetWayPoint(TeamType.WayPoint, out Waypoint))
                    {
                        sw_lua.Write("\tlocal TeamInstance_{0}=Reinforcements.Reinforce({1}, TeamType_{0},{{CPos.New({2}, {3})}}, DateTime.Seconds(0))\n", 
                            TeamType.Name, 
                            DefaultCountries[TeamType.Owner].ToLower(), 
                            Waypoint.Pos.x, Waypoint.Pos.y);
                        sw_lua.Write("\tUtils.Do(TeamInstance_{0}, function(Act)\n", TeamType.Name);
                        sw_lua.Write("\t\tTrigger.OnIdle(Act, function()\n");
                        if (TeamType.Orders.Count > 0)
                        {
                            bool first_order = true;
                            for (int OrderIndex = 0; OrderIndex < TeamType.Orders.Count; OrderIndex++)
                            {
                                var Order = TeamType.Orders[OrderIndex];
                                if (first_order)
                                {
                                    first_order = false;
                                    sw_lua.Write("\t\t\tif OrderIndex_{0} == {1} then\n", TeamType.Name, OrderIndex);
                                }
                                else
                                {
                                    sw_lua.Write("\t\t\telseif OrderIndex_{0} == {1} then\n", TeamType.Name, OrderIndex);
                                }
                                sw_lua.Write("\t\t\t\t--Order:{0}\n", ((TeamOrderType)Order.Order).ToString());

                                if (Order.Order == TeamOrderType.Move_to_waypoint)
                                {
                                    var w = new Ra95MissionIni_Waypoint();
                                    GetWayPoint(Order.Para, out w);
                                    if(w!=null) sw_lua.Write("\t\t\t\tAct.Move(CPos.New({0}, {1}))\n", w.Pos.x, w.Pos.y);
                                }
                                else if (Order.Order == TeamOrderType.Attack_Waypoint)
                                {
                                    var w = new Ra95MissionIni_Waypoint();
                                    GetWayPoint(Order.Para, out w);
                                    if (w != null) sw_lua.Write("\t\t\t\tAct.AttackMove(CPos.New({0}, {1}))\n", w.Pos.x, w.Pos.y);
                                }
                                else
                                {
                                    sw_lua.Write("\t\t\t\tTrigger.ClearAll(Act)\n");
                                }
                            }
                            sw_lua.Write("\t\t\telse\n");
                            sw_lua.Write("\t\t\t\t--OrderCnt:{0}\n", TeamType.Orders.Count);
                            sw_lua.Write("\t\t\t\tTrigger.ClearAll(Act)\n");
                            sw_lua.Write("\t\t\tend\n");
                        }
                        else
                        {
                            sw_lua.Write("\t\t\tTrigger.ClearAll(Act)\n");
                        }
                        sw_lua.Write("\t\t\tOrderIndex_{0}=OrderIndex_{0}+1\n", TeamType.Name);
                        sw_lua.Write("\t\tend)\n");
                        sw_lua.Write("\tend)\n");
                    }
                }
                sw_lua.Write("end\n\n");
            }
        }

        private void GenLua_TeamTypeDeclaration(StreamWriter sw_lua,Ra95MissionIni_TeamType TeamType)
        {
            String teamstr = "TeamType_" + TeamType.Name + "={";
            for (int teamindex = 0; teamindex < TeamType.Teams.Count; teamindex++)
            {
                for (int numi = 0; numi < TeamType.Teams[teamindex].Num; numi++)
                {
                    teamstr += "\"" + TeamType.Teams[teamindex].Type.ToLower() + "\"";
                    if (teamindex >= TeamType.Teams.Count - 1 && numi >= TeamType.Teams[teamindex].Num - 1) ;
                    else teamstr += ",";
                }
            }
            teamstr += "}\n";
            sw_lua.Write(teamstr);
        }

        private List<String> GetTriggerTeam(Ra95MissionIni_Trigger Trigger)
        {
            Ra95MissionIni_CellTrigger CellTrigger = null;
            GetCelltrigger(Trigger.Name, out CellTrigger);

            List<String> triggerunits = new List<string>();
            foreach (var s in Structures)
            {
                if (s.Trigger == Trigger.Name)
                    triggerunits.Add(String.Format("Actor_Structure_{0}", s.index));
            }
            foreach (var u in Units)
            {
                if (u.Trigger == Trigger.Name)
                    triggerunits.Add(String.Format("Actor_Unit_{0}", u.index));
            }
            foreach (var s in Ships)
            {
                if (s.Trigger == Trigger.Name)
                    triggerunits.Add(String.Format("Actor_Ship_{0}", s.index));
            }
            foreach (var i in Infantries)
            {
                if (i.Trigger == Trigger.Name)
                    triggerunits.Add(String.Format("Actor_Infantry_{0}", i.index));
            }
            return triggerunits;
        }
        private void GenLua_TriggerTeamDeclaration(StreamWriter sw_lua, Ra95MissionIni_Trigger Trigger)
        {
            Ra95MissionIni_CellTrigger CellTrigger = null;
            GetCelltrigger(Trigger.Name, out CellTrigger);

            List<String> triggerunits = new List<string>();
            foreach (var s in Structures)
            {
                if (s.Trigger == Trigger.Name)
                    triggerunits.Add(String.Format("Actor_Structure_{0}", s.index));
            }
            foreach (var u in Units)
            {
                if (u.Trigger == Trigger.Name)
                    triggerunits.Add(String.Format("Actor_Unit_{0}", u.index));
            }
            foreach (var s in Ships)
            {
                if (s.Trigger == Trigger.Name)
                    triggerunits.Add(String.Format("Actor_Ship_{0}", s.index));
            }
            foreach (var i in Infantries)
            {
                if (i.Trigger == Trigger.Name)
                    triggerunits.Add(String.Format("Actor_Infantry_{0}", i.index));
            }

            String teamstr = "\tlocal Local_TriggerTeam_" + Trigger.Name + "={";
            for (int index = 0; index < triggerunits.Count; index++)
            {
                teamstr += triggerunits[index];
                if (index < triggerunits.Count - 1) teamstr += ",";

            }
            teamstr += "}\n";
            sw_lua.Write(teamstr);
        }
        private void GenLua_TriggerAction(StreamWriter sw_lua,Ra95MissionIni_Trigger Trigger,int ActionNum)
        {
            TriggerActionType Action = TriggerActionType.No_Action;
            int ActionParaA = -1;
            int ActionParaB = -1;
            int ActionParaC = -1;
            if(ActionNum<=1)
            {
                Action = (TriggerActionType)Trigger.Action1;
                ActionParaA = Trigger.Action1ParaA;
                ActionParaB = Trigger.Action1ParaB;
                ActionParaC = Trigger.Action1ParaC;
            }
            else
            {
                Action = (TriggerActionType)Trigger.Action2;
                ActionParaA = Trigger.Action2ParaA;
                ActionParaB = Trigger.Action2ParaB;
                ActionParaC = Trigger.Action2ParaC;
            }

            sw_lua.Write("Trigger_{0}_Action{1}=function()\n",Trigger.Name, ActionNum);


            if (Action == TriggerActionType.Reinforcement_team)
            {
                var team = TeamTypes[ActionParaA];
                sw_lua.Write("\tTeamType_{0}_Reinforce()\n", team.Name);
            }
            else if(Action == TriggerActionType.Global_Set)
            {
                sw_lua.Write("\tGlobal_{0}=1\n", ActionParaC);
            }
            else if(Action == TriggerActionType.Global_Clear)
            {
                sw_lua.Write("\tGlobal_{0}=0\n", ActionParaC);
            }
            else if(Action == TriggerActionType.Force_Trigger)
            {
                sw_lua.Write("\tTrigger_{0}_Event1_Flag=1\n", Triggers[ActionParaB].Name);
                sw_lua.Write("\tTrigger_{0}_Event1_Flag=2\n", Triggers[ActionParaB].Name);
                sw_lua.Write("\tTrigger_{0}_Logic()\n", Triggers[ActionParaB].Name);
                
            }

            sw_lua.Write("end\n\n");
        }
        private void GenLua_TriggerLogic(StreamWriter sw_lua, Ra95MissionIni_Trigger Trigger)
        {
            sw_lua.Write("Trigger_{0}_Action=function()\n", Trigger.Name);
            if (Trigger.Action1 >= 0 && Trigger.Action1 <= 36)
                sw_lua.Write("\tTrigger_{0}_Action1()\n", Trigger.Name);
            if (Trigger.Action2 >= 0 && Trigger.Action2 <= 36)
                sw_lua.Write("\tTrigger_{0}_Action2()\n", Trigger.Name);
            sw_lua.Write("end\n");

            sw_lua.Write("\n");

            if (Trigger.Type == (int)TriggerType.simple) //simple
            {
                sw_lua.Write("Trigger_{0}_Logic=function()\n", Trigger.Name);
                sw_lua.Write("\tif Trigger_{0}_Event1_Flag then\n", Trigger.Name);


                sw_lua.Write("\t\tif not Trigger_{0}_Triggered then\n", Trigger.Name);
                sw_lua.Write("\t\t\tTrigger_{0}_Triggered = true\n", Trigger.Name);

                sw_lua.Write("\t\t\tTrigger_{0}_Action()\n", Trigger.Name);

                sw_lua.Write("\t\tend\n");

                sw_lua.Write("\tend\n");

                sw_lua.Write("end\n\n");
            }
            else if (Trigger.Type == (int)TriggerType.and)
            {
                sw_lua.Write("Trigger_{0}_Logic=function()\n", Trigger.Name);
                sw_lua.Write("\tif Trigger_{0}_Event1_Flag and Trigger_{0}_Event2_Flag then\n", Trigger.Name);


                sw_lua.Write("\t\tif not Trigger_{0}_Triggered then\n", Trigger.Name);
                sw_lua.Write("\t\t\tTrigger_{0}_Triggered = true\n", Trigger.Name);

                
                sw_lua.Write("\t\t\tTrigger_{0}_Action()\n", Trigger.Name);

                sw_lua.Write("\t\tend\n");

                sw_lua.Write("\tend\n");

                sw_lua.Write("end\n\n");
            }
            else if (Trigger.Type == (int)TriggerType.or)
            {
                sw_lua.Write("Trigger_{0}_Logic=function()\n", Trigger.Name);
                sw_lua.Write("\tif Trigger_{0}_Event1_Flag or Trigger_{0}_Event2_Flag then\n", Trigger.Name);


                sw_lua.Write("\t\tif not Trigger_{0}_Triggered then\n", Trigger.Name);
                sw_lua.Write("\t\t\tTrigger_{0}_Triggered = true\n", Trigger.Name);

                sw_lua.Write("\t\t\tTrigger_{0}_Action()\n", Trigger.Name);

                sw_lua.Write("\t\tend\n");

                sw_lua.Write("\tend\n");

                sw_lua.Write("end\n\n");
            }
            else if (Trigger.Type == (int)TriggerType.complex)
            {
                sw_lua.Write("Trigger_{0}_Logic=function()\n", Trigger.Name);
                sw_lua.Write("\tif not Trigger_{0}_Triggered then\n", Trigger.Name);


                sw_lua.Write("\t\tif Trigger_{0}_Event1_Flag then\n", Trigger.Name);
                if (Trigger.Action1 >= 0 && Trigger.Action1 <= 36)
                    sw_lua.Write("\t\t\tTrigger_{0}_Action1()\n", Trigger.Name);
                sw_lua.Write("\t\t\tTrigger_{0}_Triggered = true\n", Trigger.Name);

                sw_lua.Write("\t\telseif Trigger_{0}_Event2_Flag then\n", Trigger.Name);
                if (Trigger.Action2 >= 0 && Trigger.Action2 <= 36)
                    sw_lua.Write("\t\t\tTrigger_{0}_Action2()\n", Trigger.Name);
                sw_lua.Write("\t\t\tTrigger_{0}_Triggered = true\n", Trigger.Name);

                sw_lua.Write("\t\tend\n");


                sw_lua.Write("\tend\n");
                
                sw_lua.Write("end\n\n");
            }
        }
        private void GenLua_TriggerEventSetup(StreamWriter sw_lua, Ra95MissionIni_Trigger Trigger, int EventNum)
        {
            TriggerEventType Event = TriggerEventType.No_Event;
            int EventParaA = -1;
            int EventParaB = -1;
            if(EventNum<=1)
            {
                Event = (TriggerEventType)Trigger.Event1;
                EventParaA =Trigger.Event1ParaA;
                EventParaB = Trigger.Event1ParaB;
            }
            else
            {
                Event = (TriggerEventType)Trigger.Event2;
                EventParaA = Trigger.Event2ParaA;
                EventParaB = Trigger.Event2ParaB;
            }

            //Setup Function
            sw_lua.Write("Trigger_{0}_Event{1}_Setup=function()\n", Trigger.Name, EventNum);
            if (Event == TriggerEventType.No_Event)
            {

            }
            else if (Event == TriggerEventType.Entered_by)
            {
                if (IsCelltriggerExist(Trigger.Name))
                {
                    sw_lua.Write("\tTrigger.OnEnteredFootprint(CellTrigger_{0}, function(a, id)\n", Trigger.Name);
                    sw_lua.Write("\t\tif a.Owner == {0} then\n", DefaultCountries[EventParaB].ToLower(), Trigger.Name);

                    sw_lua.Write("\t\t\tTrigger_{0}_Event{1}_Flag = 1\n", Trigger.Name, EventNum);
                    sw_lua.Write("\t\t\tTrigger_{0}_Logic()\n", Trigger.Name);

                    sw_lua.Write("\t\tend\n");
                    sw_lua.Write("\tend)\n", Trigger.Name);
                }
                else
                {
                    List<String> TriggerTeam = GetTriggerTeam(Trigger);
                    if(TriggerTeam.Count>0)
                    {
                        sw_lua.Write("\tTrigger.OnCapture({0},function(self, captor, oldOwner, newOwner)\n", TriggerTeam[0]);
                        sw_lua.Write("\t\tif newOwner == {0} then\n", DefaultCountries[Trigger.Owner].ToLower());
                        sw_lua.Write("\t\t\tTrigger_{0}_Event{1}_Flag = 1\n", Trigger.Name, EventNum);
                        sw_lua.Write("\t\t\tTrigger_{0}_Logic()\n", Trigger.Name);
                        sw_lua.Write("\t\tend\n");
                        sw_lua.Write("\tend)\n");
                    }
                    else
                    {
                        sw_lua.Write("\t--Trigger Error,Name:{0},Type:{1},Wrong CellTrigger\n", Trigger.Name, Trigger.Type);
                    }
                    
                }
            }
            else if (Event == TriggerEventType.Elapsed_Time)
            {
                sw_lua.Write("\tTrigger.AfterDelay(DateTime.Seconds({0}), function()\n", (EventParaB * 6).ToString());
                sw_lua.Write("\t\tTrigger_{0}_Event{1}_Flag = 1\n", Trigger.Name, EventNum);
                sw_lua.Write("\t\tTrigger_{0}_Logic()\n", Trigger.Name);
                sw_lua.Write("\tend)\n", Trigger.Name);
            }
            else if (Event == TriggerEventType.Destroyed_by_anybody)
            {
                GenLua_TriggerTeamDeclaration(sw_lua, Trigger);

                sw_lua.Write("\tTrigger.OnAnyKilled(Local_TriggerTeam_{0},function()\n", Trigger.Name);
                sw_lua.Write("\t\tTrigger_{0}_Event{1}_Flag = 1\n", Trigger.Name, EventNum);
                sw_lua.Write("\t\tTrigger_{0}_Logic()\n", Trigger.Name);
                sw_lua.Write("\tend)\n");
            }
            else if (Event == TriggerEventType.Low_Power)
            {
                sw_lua.Write("\tTrigger.OnAnyKilled(Local_TriggerTeam_{0},function()\n", Trigger.Name);
                sw_lua.Write("\t\tTrigger_{0}_Event{1}_Flag = 1\n", Trigger.Name, EventNum);
                sw_lua.Write("\t\tTrigger_{0}_Logic()\n", Trigger.Name);
                sw_lua.Write("\tend)\n");
            }

            
            sw_lua.Write("end\n\n");


            //Tick Function
            sw_lua.Write("Trigger_{0}_Event{1}_Tick=function()\n", Trigger.Name, EventNum);
            if (Event == TriggerEventType.Global_is_set)
            {
                sw_lua.Write("\tif Global_{0} then\n", EventParaB);
                sw_lua.Write("\t\tTrigger_{0}_Event{1}_Flag=1\n", Trigger.Name, EventNum);
                sw_lua.Write("\t\tTrigger_{0}_Logic()\n", Trigger.Name);
                sw_lua.Write("\tend\n");
            }
            else if (Event == TriggerEventType.Global_is_clear)
            {
                sw_lua.Write("\tif not Global_{0} then\n", EventParaB);
                sw_lua.Write("\t\tTrigger_{0}_Event{1}_Flag=1\n", Trigger.Name, EventNum);
                sw_lua.Write("\t\tTrigger_{0}_Logic()\n", Trigger.Name);
                sw_lua.Write("\tend\n");
            }
            sw_lua.Write("end\n\n");
        }
    }
    public class IniPair
    {
        public String key = String.Empty;
        public String value = String.Empty;
    }
    class IniBlock
    {
        public String section = String.Empty;

        public List<IniPair> iniPairs = new List<IniPair>();
    }
    class Ra95MissionIni_Overlay
    {
        private static string[] redAlertOverlayNames =
        {
            "sbag", "cycl", "brik", "fenc", "wood",
            "gold01", "gold02", "gold03", "gold04",
            "gem01", "gem02", "gem03", "gem04",
            "v12", "v13", "v14", "v15", "v16", "v17", "v18",
            "fpls", "wcrate", "scrate", "barb", "sbag",
        };
        public static bool IsOverlay(int type)
        {
            if (type < 0) return false;
            else if (type >= redAlertOverlayNames.Length) return false;
            return true;
        }
        public static String GetOverlayName(int type)
        {
            if (type < 0) return String.Empty;
            else if (type >= redAlertOverlayNames.Length) return String.Empty;
            return redAlertOverlayNames[type];
        }

        public String Overlay = String.Empty;
        public Ra95MissionIni_Pos Pos=new Ra95MissionIni_Pos();

        public bool IsOre()
        {
            bool ret = false;
            if (Overlay == "gold01") ret = true;
            else if (Overlay == "gold02") ret = true;
            else if (Overlay == "gold03") ret = true;
            else if (Overlay == "gold04") ret = true;
            else if (Overlay == "gem01") ret = true;
            else if (Overlay == "gem02") ret = true;
            else if (Overlay == "gem03") ret = true;
            else if (Overlay == "gem04") ret = true;
            else if (Overlay == "wcrate") ret = true;
            else if (Overlay == "scrate") ret = true;
            return ret;
        }
    }

    class Ra95MissionIni_Terrain
    {
        public String Terrain = String.Empty;
        public Ra95MissionIni_Pos Pos = new Ra95MissionIni_Pos();
    }

    class Ra95MissionIni_Country
    {
        /*
        England
        Germany
        France
        Greece
        Turkey
        Spain
        Ukraine
        USSR
        GoodGuy
        BadGuy
        Neutral
        Special
        Multi1
        Multi2
        Multi3
        Multi4
        Multi5
        Multi6
        Multi7
        Multi8
        */
        public String Name = String.Empty;
        public bool PlayerControl = false;
        public String Allies = String.Empty;

        public String GetColor()
        {
            String ret = "FFFFFF";

            if (Name== "England") ret = "008080";
            else if (Name == "Germany") ret = "7090ff";
            else if (Name == "France") ret = "7070ff";
            else if (Name == "Greece") ret = "8080ff";
            else if (Name == "GoodGuy") ret = "9090ff";

            else if (Name == "Spain") ret = "FFFF80";

            else if (Name == "Turkey") ret = "c08040";
            else if (Name == "Ukraine") ret = "808000";//brown
            else if (Name == "USSR") ret = "800000";//red
            else if (Name == "BadGuy") ret = "900000";

            else if (Name == "Neutral") ret = "FFFFFF";
            else if (Name == "Special") ret = "FFFFFF";

            else if (Name == "Multi1") ret = "800000";
            else if (Name == "Multi2") ret = "008000";
            else if (Name == "Multi3") ret = "000080";
            else if (Name == "Multi4") ret = "808000";
            else if (Name == "Multi5") ret = "008080";
            else if (Name == "Multi6") ret = "800080";
            else if (Name == "Multi7") ret = "40C000";
            else if (Name == "Multi8") ret = "4000C0";
            
            return ret;
        }
        public String GetFaction()
        {
            String ret = "allies";

            if (Name == "England") ret = "england";
            else if (Name == "Germany") ret = "allies";
            else if (Name == "France") ret = "allies";
            else if (Name == "Greece") ret = "allies";
            else if (Name == "GoodGuy") ret = "allies";

            else if (Name == "Spain") ret = "allies";

            else if (Name == "Turkey") ret = "soviet";
            else if (Name == "Ukraine") ret = "soviet";
            else if (Name == "USSR") ret = "soviet";
            else if (Name == "BadGuy") ret = "soviet";

            return ret;
        }
        public String GetEnemies()
        {
            String ret = String.Empty;

            if (Name == "England") ret = "Ukraine,USSR,BadGuy";
            else if (Name == "Germany") ret = "Ukraine,USSR,BadGuy";
            else if (Name == "France") ret = "Ukraine,USSR,BadGuy";
            else if (Name == "Greece") ret = "Ukraine,USSR,BadGuy";
            else if (Name == "GoodGuy") ret = "Ukraine,USSR,BadGuy";

            else if (Name == "Spain") ret = "Ukraine,USSR,BadGuy";

            else if (Name == "Turkey") ret = "Ukraine,USSR,BadGuy";

            else if (Name == "Ukraine") ret = "England,Germany,France,Greece,GoodGuy,Spain,Turkey";
            else if (Name == "USSR") ret = "England,Germany,France,Greece,GoodGuy,Spain,Turkey";
            else if (Name == "BadGuy") ret = "England,Germany,France,Greece,GoodGuy,Spain,Turkey";

            return ret;
        }
    }
    class Ra95MissionIni_Map
    {
        public String Theater = String.Empty;
        public int X = 32;
        public int Y = 32;
        public int Width = 64;
        public int Height = 64;
    }
    class Ra95MissionIni_Pos
    {
        public int x = 0;
        public int y = 0;
        public bool Parse(String str)
        {
            bool ret = false;
            try
            {
                Parse(int.Parse(str));
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        public bool Parse(int val)
        {
            x = val & 0x7f;
            y = (val >> 7) & 0x7f;
            return true;
        }

    }
    class Ra95MissionIni_CellTrigger
    {
        public String Name = String.Empty;
        public List<Ra95MissionIni_Pos> TriggerPos = new List<Ra95MissionIni_Pos>();

        public bool IsPosInCellTrigger(Ra95MissionIni_Pos Pos)
        {
            bool ret = false;
            foreach(var TPos in TriggerPos)
            {
                if(TPos.x==Pos.x&&TPos.y==Pos.y)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
    }
    class Ra95MissionIni_Waypoint
    {
        public int Num=-1;
        public Ra95MissionIni_Pos Pos=new Ra95MissionIni_Pos();

        public Ra95MissionIni_Waypoint Clone()
        {
            Ra95MissionIni_Waypoint ret = new Ra95MissionIni_Waypoint();
            ret.Num = Num;
            ret.Pos.x = Pos.x;
            ret.Pos.y = Pos.y;
            return ret;
        }
    }
    enum TriggerType
    {
        simple = 0,
        and = 1,
        or = 2,
        complex = 3
    };
    enum TriggerEventType
    {
        No_Event = 0,
        Entered_by,
        Spied_by,
        Thieved_by,
        Discovered_by_player,
        House_Discovered,
        Attacked_by_anybody,
        Destroyed_by_anybody,
        Any_Event,
        Destroyed_Units_All,
        Destroyed_Buildings_All,
        Destroyed_All,
        Credits_exceed,
        Elapsed_Time,
        Mission_Timer_Expired,
        Destroyed_Buildings,
        Destroyed_Units,
        No_Factories_left,
        Civilians_Evacuated,
        Build_Building_Type,
        Build_Unit_Type,
        Build_Infantry_Type,
        Build_Aircraft_Type,
        Leaves_map_team,
        Zone_Entry_by,
        Crosses_Horizontal_Line,
        Crosses_Vertical_Line,
        Global_is_set,
        Global_is_clear,
        Destroyed_Fakes_All,
        Low_Power,
        All_bridges_destroyed,
        Building_exists
    };
    enum TriggerActionType
    {
        No_Action = 0,
        Winner_is,
        Loser_is,
        Production_Begins,
        Create_Team,
        Destroy_All_Teams,
        All_to_Hunt,
        Reinforcement_team,
        Drop_Zone_Flare_waypoint,
        Fire_Sale,
        Play_Movie,
        Text_Trigger_ID,
        Destroy_Trigger,
        Autocreate_Begins,
        don_t_use,
        Allow_Win,
        Reveal_all_map,
        Reveal_around_waypoint,
        Reveal_zone_of_waypoint,
        Play_sound_effect,
        Play_music_theme,
        Play_speech,
        Force_Trigger,
        Timer_Start,
        Timer_Stop,
        Timer_Extend_1_10th_min,
        Timer_Shorten_1_10th_min,
        Timer_Set_1_10th_min,
        Global_Set,
        Global_Clear,
        Auto_Base_Building,
        Grow_shroud_one,
        Destroy_attached_building,
        Add_1_time_special_weapon,
        Add_repeating_special_weapon,
        Preferred_target,
        Launch_Nukes
    };

    class Ra95MissionIni_Trigger
    {
        /*
        Example :
        strt=0,2,0,1,13,-1,0,0,-1,0,7,0,-1,-1,7,0,-1,-1
        Name:Strt
        0-Existense:0    temporary,semi-constant,constant
        1-Owner:2   Greece,depend on the order
        2-Type:0    simple,and,or,complex(switch)
        3-Reserved
        4-Event0
        5-Reserved
        6-Event0Para
        */
        
        /*
        Event:0~32
            No Event
            Entered by
            Spied by
            Thieved by
            Discovered by player
            House Discovered
            Attacked by anybody
            Destroyed by anybody
            Any Event
            Destroyed, Units, All
            Destroyed, Buildings, All
            Destroyed, All
            Credits exceed
            Elapsed Time
            Mission Timer Expired
            Destroyed, Buildings, #
            Destroyed, Units, #
            No Factories left
            Civilians Evacuated
            Build Building Type
            Build Unit Type
            Build Infantry Type
            Build Aircraft Type
            Leaves map (team)
            Zone Entry by
            Crosses Horizontal Line
            Crosses Vertical Line
            Global is set
            Global is clear
            Destroyed, Fakes, All
            Low Power
            All bridges destroyed
            Building exists
        */
        

        /*
        Action:0~36
            No Action
            Winner is
            Loser is
            Production Begins
            Create Team
            Destroy All Teams
            All to Hunt
            Reinforcement (team)
            Drop Zone Flare (waypoint)
            Fire Sale
            Play Movie
            Text Trigger (ID num)
            Destroy Trigger
            Autocreate Begins
            ~don't use~
            Allow Win
            Reveal all map
            Reveal around waypoint
            Reveal zone of waypoint
            Play sound effect
            Play music theme
            Play speech
            Force Trigger
            Timer Start
            Timer Stop
            Timer Extend (1/10th min)
            Timer Shorten (1/10th min)
            Timer Set (1/10th min)
            Global Set
            Global Clear
            Auto Base Building
            Grow shroud one
            Destroy attached building
            Add 1-time special weapon
            Add repeating special weapon
            Preferred target
            Launch Nukes
        */
        


        public String Name = String.Empty;//名称

        public int Existense = -1;//
        public int Owner = -1;//根据INI中Player顺序而来
        public int Type = -1;
        public int Reserved=0;

        public int Event1 = -1;
        public int Event1ParaA = -1;
        public int Event1ParaB = -1;

        public int Event2 = -1;
        public int Event2ParaA = -1;
        public int Event2ParaB = -1;

        public int Action1 = -1;
        public int Action1ParaA = -1;
        public int Action1ParaB = -1;
        public int Action1ParaC = -1;

        public int Action2 = -1;
        public int Action2ParaA = -1;
        public int Action2ParaB = -1;
        public int Action2ParaC = -1;

        public bool Parse(String key, String val)
        {
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(val)) return false;

            bool ret = false;
            try
            {
                Name = key;
                var vals = val.Split(',');
                Existense = int.Parse(vals[0].Trim());
                Owner = int.Parse(vals[1].Trim());
                Type = int.Parse(vals[2].Trim());
                Reserved = int.Parse(vals[3].Trim());

                Event1 = int.Parse(vals[4].Trim());
                Event1ParaA = int.Parse(vals[5].Trim());
                Event1ParaB = int.Parse(vals[6].Trim());

                Event2 = int.Parse(vals[7].Trim());
                Event2ParaA = int.Parse(vals[8].Trim());
                Event2ParaB = int.Parse(vals[9].Trim());

                Action1 = int.Parse(vals[10].Trim());
                Action1ParaA = int.Parse(vals[11].Trim());
                Action1ParaB = int.Parse(vals[12].Trim());
                Action1ParaC = int.Parse(vals[13].Trim());

                Action2 = int.Parse(vals[14].Trim());
                Action2ParaA = int.Parse(vals[15].Trim());
                Action2ParaB = int.Parse(vals[16].Trim());
                Action2ParaC = int.Parse(vals[17].Trim());

                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
    }
    class Ra95MissionIni_TeamType_Team
    {
        public string Type = String.Empty;
        public int Num = -1;
    }
    enum TeamOrderType
    {
        Invalid=-1,
        Attack=0,
        Attack_Waypoint,
        Change_Formation_to,
        Move_to_waypoint,
        Move_to_Cell,
        Guard_area_1_10th_min,
        Jump_to_line,
        Attack_Tarcom,
        Unload,
        Deploy,
        Follow_friendlies,
        Do_this,
        Set_global,
        Invulnerable,
        Load_onto_Transport,
        Spy_on_bldg_waypt,
        Patrol_to_waypoint
    };
    class Ra95MissionIni_TeamType_Order
    {
        public TeamOrderType Order = TeamOrderType.Invalid;
        public int Para = -1;
    }
    class Ra95MissionIni_TeamType
    {
        /*
        Team1=1,31,7,0,0,16,-1,3,E1:1,E2:1,E4:1,6,9:0,10:0,11:0,11:0,11:0,11:0
        Name=Owner,Option,Priority,Max,Num,WayPoint,Trigger,
            TeamCnt,Type:Num...
            OrderCnt,Order:Para...
        */
        public String Name = String.Empty;

        public int Owner = -1;
        /*
        Use safest, possibly longer, route to target.
        Charge toward target ignoring distractions.
        Only autocreate AI uses this team type.
        Prebuild team members before team is created.
        Automatically reinforce team whenever possible.
        */
        public bool Opt_SafestToTarget = false;
        public bool Opt_Ignoring_Distraction = false;
        public bool Opt_Only_Autocreate_AI = false;
        public bool Opt_Prebuild_Team_Members = false;
        public bool Opt_Automatically_Reinforce_Team = false;

        public int Priority = -1;
        public int Max = -1;
        public int Num = -1;
        public int WayPoint = -1;
        public int Trigger = -1;

        public List<Ra95MissionIni_TeamType_Team> Teams = new List<Ra95MissionIni_TeamType_Team>();
        public List<Ra95MissionIni_TeamType_Order> Orders = new List<Ra95MissionIni_TeamType_Order>();
        
        public bool Parse(String key, String val)
        {
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(val)) return false;

            bool ret = false;
            try
            {
                Name = key;
                var vals = val.Split(',');

                Owner = int.Parse(vals[0].Trim());
                int Option = int.Parse(vals[1].Trim());

                Opt_SafestToTarget = (Option & 1)!=0;
                Opt_Ignoring_Distraction = (Option & 2)!=0;
                Opt_Only_Autocreate_AI = (Option & 4)!=0;
                Opt_Prebuild_Team_Members = (Option & 8)!=0;
                Opt_Automatically_Reinforce_Team = (Option & 16)!=0;

                Priority = int.Parse(vals[2].Trim());
                Max = int.Parse(vals[3].Trim());
                Num = int.Parse(vals[4].Trim());
                WayPoint = int.Parse(vals[5].Trim());
                Trigger = int.Parse(vals[6].Trim());


                int TeamCnt= int.Parse(vals[7].Trim());
                for(int teamindex=0; teamindex<TeamCnt; teamindex++)
                {
                    var teamstrs = vals[7 + 1 + teamindex].Trim().Split(':');
                    var Team = new Ra95MissionIni_TeamType_Team();
                    Team.Type= teamstrs[0].Trim();
                    Team.Num= int.Parse(teamstrs[1].Trim());
                    Teams.Add(Team);
                }

                int OrderCnt = int.Parse(vals[7+ 1+TeamCnt].Trim());

                for (int orderndex = 0; orderndex < OrderCnt; orderndex++)
                {
                    var orderstrs = vals[7 + 1 + TeamCnt + 1 + orderndex].Trim().Split(':');
                    var Order = new Ra95MissionIni_TeamType_Order();
                    Order.Order = (TeamOrderType)int.Parse(orderstrs[0].Trim());
                    Order.Para = int.Parse(orderstrs[1].Trim());
                    Orders.Add(Order);
                }

                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }

        public bool HasUnit(String unittype)
        {
            foreach (var Team in Teams)
            {
                if (Team.Type.ToLower() == unittype)
                {
                    return true;
                }
            }
            return false;
        }
        public List<Ra95MissionIni_TeamType_Team> GetTeamsWithExceptionFilter(String unittype)
        {
            List<Ra95MissionIni_TeamType_Team> ret = new List<Ra95MissionIni_TeamType_Team>();
            foreach (var Team in Teams)
            {
                if (Team.Type.ToLower() != unittype)
                {
                    ret.Add(Team);
                }
            }
            return ret;
        }

    }
    class Ra95MissionIni_Basic
    {
        public String Name = String.Empty;
        public String Intro = String.Empty;
        public String Brief = String.Empty;
        public String Win = String.Empty;
        public String Lose = String.Empty;
        public String Action = String.Empty;
        public String Player = String.Empty;
        public String Theme = String.Empty;
        public String CarryOverMoney = String.Empty;
        public String ToCarryOver = String.Empty;
        public String ToInherit = String.Empty;
        public String TimerInherit = String.Empty;
        public String CivEvac = String.Empty;
        public String NewINIFormat = String.Empty;
        public String CarryOverCap = String.Empty;
        public String EndOfGame = String.Empty;
        public String NoSpyPlane = String.Empty;
        public String SkipScore = String.Empty;
        public String OneTimeOnly = String.Empty;
        public String SkipMapSelect = String.Empty;
        public String Official = String.Empty;
        public String FillSilos = String.Empty;
        public String TruckCrate = String.Empty;
        public String Percent = String.Empty;
    }

    class Ra95MissionIni_Structure
    {
        //6=Spain,V10,256,11294,0,hel2,1,0
        public String Owner = String.Empty;
        public String Structure = String.Empty;
        public int Health = 255;
        public Ra95MissionIni_Pos Pos = new Ra95MissionIni_Pos();
        public int Direction = 255;
        public String Trigger = String.Empty;
        public int Sellable = 0;
        public int Rebuild = 0;

        public int index = -1;

        public bool Parse(String key, String val)
        {
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(val)) return false;

            bool ret = false;
            try
            {
                index= int.Parse(key.Trim());

                var vals = val.Split(',');

                Owner = vals[0].Trim();
                Structure = vals[1].Trim();
                Health = int.Parse(vals[2].Trim());
                Pos = new Ra95MissionIni_Pos();
                Pos.Parse(vals[3].Trim());

                Direction = int.Parse(vals[4].Trim());
                Trigger = vals[5].Trim();
                Sellable = int.Parse(vals[6].Trim());
                Rebuild = int.Parse(vals[7].Trim());

                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
    }
    class Ra95MissionIni_Infantry
    {
        //0=Greece,E1,230,5798,1,Area Guard,64,opd
        public String Owner = String.Empty;
        public String Infantry = String.Empty;
        public int Health = 255;
        public Ra95MissionIni_Pos Pos = new Ra95MissionIni_Pos();
        
        public int PositionInCell = 0;
        public String Mission = String.Empty;
        public int Direction = 255;
        public String Trigger = String.Empty;

        public int index = -1;

        public bool Parse(String key, String val)
        {
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(val)) return false;

            bool ret = false;
            try
            {
                index = int.Parse(key.Trim());

                var vals = val.Split(',');

                Owner = vals[0].Trim();
                Infantry = vals[1].Trim();
                Health = int.Parse(vals[2].Trim());
                Pos = new Ra95MissionIni_Pos();
                Pos.Parse(vals[3].Trim());

                PositionInCell = int.Parse(vals[4].Trim());
                Mission = vals[5].Trim();
                Direction = int.Parse(vals[6].Trim());
                Trigger = vals[7].Trim();
                
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
    }
    class Ra95MissionIni_Unit
    {
        //0=Greece,HARV,256,11594,192,Harvest,bdef
        public String Owner = String.Empty;
        public String Unit = String.Empty;
        public int Health = 255;
        public Ra95MissionIni_Pos Pos = new Ra95MissionIni_Pos();

        public int Direction = 255;
        public String Mission = String.Empty;
        public String Trigger = String.Empty;

        public int index = -1;

        public bool Parse(String key, String val)
        {
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(val)) return false;

            bool ret = false;
            try
            {
                index = int.Parse(key.Trim());

                var vals = val.Split(',');

                Owner = vals[0].Trim();
                Unit = vals[1].Trim();
                Health = int.Parse(vals[2].Trim());
                Pos = new Ra95MissionIni_Pos();
                Pos.Parse(vals[3].Trim());

                Direction = int.Parse(vals[4].Trim());
                Mission = vals[5].Trim();
                Trigger = vals[6].Trim();
                
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
    }
    class Ra95MissionIni_Ship
    {
        //0=USSR,SS,256,652,0,Sleep,None
        public String Owner = String.Empty;
        public String Ship = String.Empty;
        public int Health = 255;
        public Ra95MissionIni_Pos Pos = new Ra95MissionIni_Pos();

        public int Direction = 255;
        public String Mission = String.Empty;
        public String Trigger = String.Empty;

        public int index = -1;

        public bool Parse(String key, String val)
        {
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(val)) return false;

            bool ret = false;
            try
            {
                index = int.Parse(key.Trim());

                var vals = val.Split(',');

                Owner = vals[0].Trim();
                Ship = vals[1].Trim();
                Health = int.Parse(vals[2].Trim());
                Pos = new Ra95MissionIni_Pos();
                Pos.Parse(vals[3].Trim());

                Direction = int.Parse(vals[4].Trim());
                Mission = vals[5].Trim();
                Trigger = vals[6].Trim();
                
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
    }

}
