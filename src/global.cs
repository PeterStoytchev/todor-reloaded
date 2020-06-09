using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Text;

namespace todor_reloaded
{
    public static class global
    {
        public static DiscordClient bot { get; set; }

        public static CommandsNextExtension commands { get; set; }

        public static BotConfig botConfig { get; set; }

        public static Player player { get; set; }

        public static YouTubeUtils tubeUtils { get; set; }
        
        public static int queueCounter { get; set; }



    }
}
