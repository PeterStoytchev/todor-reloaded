using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using DSharpPlus.VoiceNext;
using System.Runtime.InteropServices;
using System.Data;
using System.Linq;

namespace todor_reloaded
{
    public static class utils
    {

        //a temporary function for extracting youtube urls, will be used until support for YouTube Data API V3 arrives
        public static string ExtractYoutubeId(string link)
        {
            //a normal youtube url contains a '='
            if (link.Contains("="))
            {
                return link.Split("=").Last();
            }
            //a shortened youtube url is 28 chars long
            else if (link.Length == 28)
            {
                return link.Split("/").Last();
            }
            //the function doesnt support any other types
            else
            {
                throw new InvalidDataException("Invalid or unsuported url passed.");
            }

        }

        public static string ArrayToString(string[] arr, char seperator)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string str in arr)
            {
                sb.Append(str + seperator);
            }

            return sb.ToString();

        }

        public static DiscordEmbedBuilder GetEmbedBuilderForSong(DiscordColor color, string title, string source)
        {
            DiscordEmbedBuilder EmbedBuilder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
            };

            EmbedBuilder.AddField("Song name:", title);
            EmbedBuilder.AddField("Song link:", source);

            return EmbedBuilder;
        }

        public static void LogMessage(string msg, BaseDiscordClient client, LogLevel logLevel = LogLevel.Info)
        {
            client.DebugLogger.LogMessage(logLevel, client.CurrentUser.Username, msg, DateTime.Now);
        }
    }
}
