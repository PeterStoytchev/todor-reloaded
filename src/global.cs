using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Text;
using SpotifyAPI.Web;


namespace todor_reloaded
{
    public static class global
    {
        public static DiscordClient bot { get; set; }

        public static CommandsNextExtension commands { get; set; }

        public static BotConfig botConfig { get; set; }

        public static SpotifyClient spotify { get; set; }

        public static LavaPlayer lavaPlayer { get; set; }
    }
}
