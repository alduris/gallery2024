using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            { "GR_0rchid-1a", new("0rchid", "Bridge/Suspended") }
        };
    }
}
