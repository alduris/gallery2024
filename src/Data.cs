using System;
using System.Collections.Generic;
using System.Linq;

namespace Gallery2024
{
    internal static class Data
    {
        public struct RoomData(string Author, string Prompts)
        {
            public string author = Author;
            public string prompts = Prompts;
        }

        public static readonly Dictionary<string, RoomData> Rooms = new()
        {
            { "GR_0rchid-1a", new("0rchid", "Bridge/Suspended") },
            { "GR_0rchid-2a", new("0rchid", "Crumbling Crypt") },
            { "GR_0rchid-2b", new("0rchid", "Growth") },
            { "GR_0rchid-4b", new("0rchid", "Diseased") },
            { "GR_Addy-1b", new("Addy", "Suspended") },
            { "GR_Addy-2b", new("Addy", "Growth") },
            { "GR_Addy-3b", new("Addy", "Monolith") },
            { "GR_Addy-4b", new("Addy", "Diseased") },
            { "GR_Alduris-1b", new("Alduris", "Suspended") },
            { "GR_Alduris-2b", new("Alduris", "Growth") },
            { "GR_Alduris-3b", new("Alduris", "Monolith") },
            { "GR_Alduris-4b", new("Alduris", "Diseased") },
            { "GR_alekus-1b", new("Alekus", "Suspended") },
            { "GR_alexavailable-1b", new("AlexAvailable", "Bridge/Suspended") },
            { "GR_ATOM-3b", new("Atom", "Monolith") },
            { "GR_Ax3lotI-1a", new("Ax3lotI", "Bridge") },
            { "GR_Ax3lotI-2b", new("Ax3lotI", "Growth") },
            { "GR_Ax3lotI-3b", new("Ax3lotI", "Monolith") },
            { "GR_Bemo-1b", new("Bemo", "Suspended") },
            { "GR_Bemo-3a", new("Bemo", "Shrouded Facility") },
            { "GR_bismuth-2b", new("Bismuth", "Growth") },
            { "GR_Bootleg-1b", new("Bootleg", "Suspended") },
            { "GR_Bootleg-2b", new("Bootleg", "Growth") },
            { "GR_Bootleg-3a", new("Bootleg", "Shrouded Facility") },
            { "GR_Bootleg-4b", new("Bootleg", "Diseased") },
            { "GR_brownie-4a", new("brownie", "Respite") },
            { "GR_catatu-1a", new("Catatu", "Bridge") },
            { "GR_chipped-1a", new("Chipped", "Bridge") },
            { "GR_Coolkid-1a", new("coolkid", "Bridge") },
            { "GR_Coolkid-1b", new("coolkid", "Suspended") },
            { "GR_DEDNIKO-1b", new("Dedniko", "Suspended") },
            { "GR_DEDNIKO-2a", new("Dedniko", "Crumbling Crypt") },
            { "GR_DEDNIKO-3b", new("Dedniko", "Monolith") },
            { "GR_Deerstop-4a", new("Deerstop", "Respite") },
            { "GR_DepressionBomb-1a", new("Ovidia", "Bridge/Crumbling Crypt") },
            { "GR_Detrax-1b", new("Detrax", "Bridge/Suspended") },
            { "GR_Detrax-2a", new("Detrax", "Crumbling Crypt") },
            { "GR_Detrax-3b", new("Detrax", "Monolith") },
            { "GR_Detrax-4a", new("Detrax", "Respite") },
            { "GR_Detrax-4b", new("Detrax", "Diseased") },
            { "GR_DodoDecalogist-3a", new("Dodo Decalogist", "Shrouded Facility") },
            { "GR_Dogcat-1a", new("Dogcat", "Bridge") },
            { "GR_Dogcat-3b", new("Dogcat", "Monolith") },
            { "GR_Dogcat-4a", new("Dogcat", "Respite") },
            { "GR_Fatal-1a", new("Fatal", "Bridge") },
            { "GR_FFL-1a", new("Five Frozen Lakes", "Bridge") },
            { "GR_FFL-1b", new("Five Frozen Lakes", "Suspended") },
            { "GR_FFL-2a", new("Five Frozen Lakes", "Crumbling Crypt") },
            { "GR_FFL-3b", new("Five Frozen Lakes", "Monolith") },
            { "GR_Incogniko-1a", new("Incogniko", "Bridge") },
            { "GR_Incogniko-1b", new("Incogniko", "Suspended") },
            { "GR_Incogniko-2a", new("Incogniko", "Crumbling Crypt") },
            { "GR_Incogniko-2b", new("Incogniko", "Growth") },
            { "GR_Inspectnerd-1a", new("Inspectnerd", "Bridge/Crumbling Crypt") },
            { "GR_Inspectnerd-1b", new("Inspectnerd", "Suspended") },
            { "GR_Inspectnerd-2a", new("Inspectnerd", "Crumbling Crypt") },
            { "GR_Inspectnerd-2b", new("Inspectnerd", "Growth") },
            { "GR_Inspectnerd-3a", new("Inspectnerd", "Shrouded Facility") },
            { "GR_Inspectnerd-3b", new("Inspectnerd", "Monolith") },
            { "GR_Inspectnerd-4a", new("Inspectnerd", "Respite") },
            { "GR_Inspectnerd-4b", new("Inspectnerd", "Diseased") },
            { "GR_Iswto-1b", new("Iswto", "Suspended") },
            { "GR_Iswto-2b", new("Iswto", "Growth") },
            { "GR_Iswto-3b", new("Iswto", "Monolith") },
            { "GR_Iswto-4a", new("Iswto", "Respite") },
            { "GR_izuna-1a", new("izuna", "Bridge/Suspended") },
            { "GR_izuna-3a", new("izuna", "Shrouded Facility") },
            { "GR_Jevmen-1a", new("Jevmen", "Bridge") },
            { "GR_Jevmen-2a", new("Jevmen", "Crumbling Crypt") },
            { "GR_KAL-1a", new("KAL", "Bridge") },
            { "GR_klum-2b", new("klum", "Growth") },
            { "GR_klum-3b", new("klum", "Monolith") },
            { "GR_klum-4b", new("klum", "Diseased") },
            { "GR_KOF-1b", new("King Of Fire", "Suspended") },
            { "GR_KOF-2b", new("King Of Fire", "Growth") },
            { "GR_KOF-4a", new("King Of Fire", "Respite") },
            { "GR_lkelly-1b", new("lkelly", "Suspended") },
            { "GR_lkelly-3a", new("lkelly", "Shrouded Facility") },
            { "GR_mayhemmm-1b", new("mayhemmm", "Suspended") },
            { "GR_mayhemmm-2a", new("mayhemmm", "Crumbling Crypt") },
            { "GR_mayhemmm-4b", new("mayhemmm", "Diseased") },
            { "GR_meta-1a", new("meta", "Bridge") },
            { "GR_meta-1b", new("meta", "Suspended") },
            { "GR_meta-2b", new("meta", "Growth") },
            { "GR_Millisec-1b", new("Millisec", "Suspended") },
            { "GR_Millisec-2b", new("Millisec", "Growth") },
            { "GR_Millisec-3b", new("Millisec", "Monolith") },
            { "GR_Mojosoup-1a", new("Mojosoup", "Bridge") },
            { "GR_Mold-1a", new("Mold223", "Bridge") },
            { "GR_Mold-1b", new("Mold223", "Suspended") },
            { "GR_Mold-2a", new("Mold223", "Crumbling Crypt") },
            { "GR_Mold-2b", new("Mold223", "Growth") },
            { "GR_Mold-3b", new("Mold223", "Monolith") },
            { "GR_Mold-4a", new("Mold223", "Respite") },
            { "GR_Mold-4b", new("Mold223", "Diseased") },
            { "GR_Nekovo-1a", new("Nekovo", "Bridge") },
            { "GR_Nekovo-2a", new("Nekovo", "Crumbling Crypt/Growth") },
            { "GR_Nekovo-3a", new("Nekovo", "Shrouded Facility") },
            { "GR_Nekovo-4a", new("Nekovo", "Respite") },
            { "GR_nopuddles-1a", new("nopuddles", "Bridge") },
            { "GR_nopuddles-2b", new("nopuddles", "Growth") },
            { "GR_nopuddles-3b", new("nopuddles", "Monolith") },
            { "GR_nopuddles-4a", new("nopuddles", "Respite") },
            { "GR_Nora-3a", new("Nora", "Shrouded Facility") },
            { "GR_NOTGOODGAMER-2b", new("TheNotGoodGamer", "Growth") },
            { "GR_NOTGOODGAMER-3b", new("TheNotGoodGamer", "Monolith") },
            { "GR_NOTGOODGAMER-4b", new("TheNotGoodGamer", "Diseased") },
            { "GR_OCTO-3b", new("Octo", "Monolith") },
            { "GR_Oscarriaq-1a", new("Oscarriaq", "Bridge") },
            { "GR_Oscarriaq-1b", new("Oscarriaq", "Suspended") },
            { "GR_Oscarriaq-2b", new("Oscarriaq", "Growth") },
            { "GR_Oscarriaq-3a", new("Oscarriaq", "Shrouded Facility") },
            { "GR_parageist-1a", new("parageist", "Bridge") },
            { "GR_QTPI-1b", new("chloe", "Suspended") },
            { "GR_QTPI-4b", new("chloe", "Diseased") },
            { "GR_quivel-1b", new("quivel", "Suspended") },
            { "GR_quivel-2b", new("quivel", "Growth") },
            { "GR_quivel-3b", new("quivel", "Monolith") },
            { "GR_quivel-4b", new("quivel", "Diseased") },
            { "GR_Rbor-1a", new("Rbor", "Bridge") },
            { "GR_Rbor-2b", new("Rbor", "Growth") },
            { "GR_Rbor-3b", new("Rbor", "Monolith") },
            { "GR_Rbor-4b", new("Rbor", "Diseased") },
            { "GR_ren-1a", new("ren", "Bridge") },
            { "GR_RexNecroMachina-2a", new("Rex_Necro_Machina", "Crumbling Crypt") },
            { "GR_RexNecroMachina-3a", new("Rex_Necro_Machina", "Shrouded Facility") },
            { "GR_RexNecroMachina-3b", new("Rex_Necro_Machina", "Monolith") },
            { "GR_RexNecroMachina-4a", new("Rex_Necro_Machina", "Respite") },
            { "GR_RexNecroMachina-4b", new("Rex_Necro_Machina", "Diseased") },
            { "GR_ROSE-1a", new("Rose", "Bridge") },
            { "GR_ROSE-2a", new("Rose", "Crumbling Crypt/Growth") },
            { "GR_Seroen-1a", new("Seroen", "Bridge") },
            { "GR_Seroen-1b", new("Seroen", "Suspended") },
            { "GR_Seroen-2a", new("Seroen", "Crumbling Crypt") },
            { "GR_Seroen-2b", new("Seroen", "Growth") },
            { "GR_Seroen-3a", new("Seroen", "Shrouded Facility") },
            { "GR_Seroen-3b", new("Seroen", "Monolith") },
            { "GR_Seroen-4a", new("Seroen", "Respite") },
            { "GR_Seroen-4b", new("Seroen", "Diseased") },
            { "GR_snoodle-1a", new("snoodle", "Bridge") },
            { "GR_snoodle-2b", new("snoodle", "Growth") },
            { "GR_SPIKO-1a", new("SPIKO", "Bridge") },
            { "GR_SPIKO-1b", new("SPIKO", "Suspended") },
            { "GR_SPIKO-2a", new("SPIKO", "Crumbling Crypt") },
            { "GR_SPIKO-3a", new("SPIKO", "Shrouded Facility") },
            { "GR_SPIKO-3b", new("SPIKO", "Monolith") },
            { "GR_SPIKO-4a", new("SPIKO", "Respite") },
            // { "GR_SPIKO-4b", new("SPIKO", "Diseased") }, // removed due to mismatching geo and unable to contact creator
            { "GR_StarlightReactor-3b", new("StarlightReactor", "Monolith") },
            { "GR_StarlightReactor-4a", new("StarlightReactor", "Respite") },
            { "GR_Stoatykins-1a", new("Stoatykins", "Bridge") },
            { "GR_Stoatykins-2a", new("Stoatykins", "Crumbling Crypt/Growth") },
            { "GR_TaGRa-1a", new("TaGRa", "Bridge") },
            { "GR_TaGRa-2a", new("TaGRa", "Crumbling Crypt") },
            { "GR_tapok-1a", new("tapok", "Bridge") },
            { "GR_tapok-1b", new("tapok", "Suspended") },
            { "GR_tapok-2a", new("tapok", "Crumbling Crypt") },
            { "GR_tapok-2b", new("tapok", "Growth") },
            { "GR_tapok-3a", new("tapok", "Shrouded Facility") },
            { "GR_tapok-3b", new("tapok", "Monolith") },
            { "GR_tapok-4a", new("tapok", "Respite") },
            { "GR_tapok-4b", new("tapok", "Diseased") },
            { "GR_Tat-1b", new("Tat", "Suspended") },
            { "GR_Tcan-1b", new("Tcan", "Bridge/Suspended") },
            { "GR_Tcan-2a", new("Tcan", "Crumbling Crypt") },
            { "GR_TheCubifyer-2a", new("TheCubifyer", "Crumbling Crypt/Growth") },
            { "GR_Tolly-1b", new("TollyCastle", "Suspended") },
            { "GR_Tolly-2a", new("TollyCastle", "Crumbling Crypt") },
            { "GR_Tolly-4b", new("Tolly", "Diseased") },
            { "GR_Tronsx-1b", new("Tronsx", "Bridge/Suspended") },
            { "GR_Tronsx-2a", new("Tronsx", "Crumbling Crypt/Growth") },
            { "GR_Tronsx-3b", new("Tronsx", "Shrouded Facility/Monolith") },
            { "GR_Tronsx-4b", new("Tronsx", "Respite/Diseased") },
            { "GR_tyrix", new("Tyrix", "Every prompt") },
            { "GR_vprchm-1b", new("vprchm", "Suspended") },
            { "GR_vprchm-2a", new("vprchm", "Crumbling Crypt") },
            { "GR_WEX-1a", new("Wex", "Bridge") },
            { "GR_WEX-1b", new("Wex", "Suspended") },
            { "GR_WEX-2a", new("Wex", "Crumbling Crypt") },
            { "GR_Worldspire-1b", new("Worldspire", "Suspended") },
            { "GR_wrayk-1b", new("Wrayk", "Suspended") },
            { "GR_wrayk-2a", new("Wrayk", "Crumbling Crypt") },
            { "GR_WYNNE-1a", new("Wynne", "Bridge/Suspended") },
            { "GR_YIPPEE-2b", new("YippeeChan", "Growth") },
            { "GR_yofish-2b", new("yofish", "Growth") },
            { "GR_Zarquon-1b", new("Zarquon", "Suspended") },
            { "GR_Zarquon-2a", new("Zarquon", "Crumbling Crypt/Growth") },
            { "GR_Zarquon-3b", new("Zarquon", "Monolith") }
        };

        private static bool _pulled = false;
        private static readonly HashSet<string> _visited = [];

        private static void PullRooms()
        {
            if (!_pulled && Plugin.OI != null)
            {
                _pulled = true;
                var str = Plugin.OI.VisitedRooms.Value;
                try
                {
                    var json = ((List<object>)Json.Deserialize(str)).Cast<string>();
                    foreach (var item in json)
                    {
                        if (Rooms.ContainsKey(item))
                        {
                            _visited.Add(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    Plugin.Logger.LogError("Error reading visited rooms!");
                    Plugin.Logger.LogError(e);
                    Plugin.OI.VisitedRooms.Value = "[]";
                }
            }
        }

        public static List<string> VisitedRooms
        {
            get
            {
                PullRooms();
                return [.. _visited];
            }
        }

        public static int UpdateVisited(string room)
        {
            PullRooms();
            if (Rooms.ContainsKey(room) && _visited.Add(room))
            {
                Plugin.OI.VisitedRooms.Value = Json.Serialize(_visited.ToList());
                Plugin.OI.config.Save();
            }
            return _visited.Count; // return how many rooms have been visited
        }

        public static bool IsVisited(string room)
        {
            PullRooms();
            return Rooms.ContainsKey(room) && _visited.Contains(room);
        }

        public static RoomData GetRoomData(string room)
        {
            if (Rooms.TryGetValue(room, out var roomData))
            {
                return roomData;
            }

            return new RoomData()
            {
                author = "",
                prompts = ""
            };
        }

        public static void Reset()
        {
            if (_visited.Count > 0)
            {
                _visited.Clear();
                Plugin.OI.VisitedRooms.Value = "[]";
                Plugin.OI.config.Save();
            }
        }

        public static int VisitedCount => _visited.Count;
        public static int TotalRoomCount => Rooms.Count;
    }

    public class MutBox<T>(T value)
    {
        public T Value = value;
    }
}
