using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace todor_reloaded
{
    public class Song
    { 
        public SongType type { get; set; }
        public CommandContext ctx { get; set; }

        public string url { get; set; }
        public string file { get; set; }

        public string name { get; set; }
        public string uploader { get; set; }
        public string duration { get; set; }

        public bool isCached = false;

    }

    public enum SongType
    {
        Youtube,
        Soundcloud,
        Spotify,
        Local
    }
}
