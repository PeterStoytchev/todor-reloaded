using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace todor_reloaded
{
    public class Song
    { 
        public SongType type { get; private set; }
        public string url { get; private set; }
        public string file { get; set; }

        public string name { get; private set; }
        public string uploader { get; private set; }
        public string duration { get; private set; }
    }

    public enum SongType
    {
        Youtube,
        Soundcloud,
        Spotify,
        Local
    }
}
