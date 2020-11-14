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

        public static Player player { get; set; }

        public static GoogleClient googleClient { get; set; }

        public static PersistentDictionary songCache { get; set; }

        public static SpotifyClient spotify { get; set; }

        public static NotificationSystem notificationSystem { get; set; }

        public static int queueCounter { get; set; }
    }
}
