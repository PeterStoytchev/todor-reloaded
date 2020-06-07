﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;
using Newtonsoft.Json;

namespace todor_reloaded
{
    public class BotConfig
    {
        [JsonProperty("transcoderThreadSleepTime")]
        public int transcoderThreadSleepTime { get; private set; }

        [JsonProperty("transcoderBufferSize")]
        public int transcoderBufferSize { get; private set; }

        [JsonProperty("discordToken")]
        public string discordToken { get; private set; }

        [JsonProperty("youtubeApiKey")]
        public string youtubeApiKey { get; private set; }

        [JsonProperty("fileStorageExtention")]
        public string fileExtention { get; private set; }

        [JsonProperty("songCacheDir")]
        public string songCacheDir { get; private set; }

        [JsonProperty("commandPrefixes")]
        public IEnumerable<string> prefixes { get; private set; }
        
        [JsonProperty("discordLogLevel")]
        public LogLevel discordLogLevel { get; private set; }

        public string configDir { get; set; }

        public DiscordConfiguration GetDiscordConfiguration()
        {
            DiscordConfiguration configuration = new DiscordConfiguration
            {
                Token = discordToken,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = discordLogLevel,

                UseInternalLogHandler = true
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
