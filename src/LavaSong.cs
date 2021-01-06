using System;
using System.Collections.Generic;
using System.Text;

using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace todor_reloaded
{
    class LavaSong
    {
        public LavaSong(LavalinkTrack track, DiscordChannel requestChannel)
        {
            this.lavaTrack = track;
            this.requestChannel = requestChannel;
        }

        public LavalinkTrack lavaTrack;
        public DiscordChannel requestChannel;
    }
}
