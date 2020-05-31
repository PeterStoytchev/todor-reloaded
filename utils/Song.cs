using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace todor_reloaded
{
    public class Song
    { 
        public SongType type { get; private set; }
        public string path { get; private set; } //can be a link to youtube

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
