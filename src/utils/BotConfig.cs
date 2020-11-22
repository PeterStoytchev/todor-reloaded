using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using SpotifyAPI.Web;

namespace todor_reloaded
{
    public class BotConfig
    {
        [JsonProperty("DebugMode")]
        public bool DebugMode { get; private set; }

        [JsonProperty("notificationDataPath")]
        public string notificationDataPath { get; private set; }

        [JsonProperty("transcoderThreadSleepTime")]
        public int transcoderThreadSleepTime { get; private set; }

        [JsonProperty("transcoderBufferSize")]
        public int transcoderBufferSize { get; private set; }
        
        [JsonProperty("discordGuildId")]
        public ulong discordGuildId { get; private set; }

        [JsonProperty("discordToken")]
        public string discordToken { get; private set; }

        [JsonProperty("googleServiceKey")]
        public string googleServiceKey { get; private set; }

        [JsonProperty("countryCode")]
        public string countryCode { get; private set; }

        [JsonProperty("spotifyEnabled")]
        public bool spotifyEnabled { get; private set; }

        [JsonProperty("spotifyClientId")]
        public string spotifyClientId { get; private set; }

        [JsonProperty("spotifyClientSecret")]
        public string spotifyClientSecret { get; private set; }

        [JsonProperty("ytdlPath")]
        public string ytdlPath { get; private set; }

        [JsonProperty("ffmpegPath")]
        public string ffmpegPath { get; private set; }
        
        [JsonProperty("songCacheDir")]
        public string songCacheDir { get; private set; }

        [JsonProperty("commandPrefixes")]
        public IEnumerable<string> prefixes { get; private set; }
        
        public string configDir { get; set; }

        public SpotifyClientConfig GetClientConfig()
        {
            return SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(spotifyClientId, spotifyClientSecret));
        }

        public DiscordConfiguration GetDiscordConfiguration()
        {
            DiscordConfiguration configuration = new DiscordConfiguration
            {
                Token = discordToken,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
            };

            return configuration;
        }

        public CommandsNextConfiguration GetCommandsNextConfiguration()
        {
            CommandsNextConfiguration configuration = new CommandsNextConfiguration
            {
                StringPrefixes = prefixes,
                EnableMentionPrefix = false,
                EnableDms = false
            };

            return configuration;
        }

        public static async Task<BotConfig> CreateConfig(string path)
        {
            var json = "";
            using (var fs = File.OpenRead(path))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            var cfgjson = JsonConvert.DeserializeObject<BotConfig>(json);
            cfgjson.configDir = path;

            return cfgjson;
        }
    }
}
