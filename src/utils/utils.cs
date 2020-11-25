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
using Google.Apis.Http;
using Microsoft.Extensions.Logging;

namespace todor_reloaded
{
    public static class utils
    {
        public static void DebugLog(string message, LogLevel logLevel = LogLevel.Information)
        {
            if (global.botConfig.DebugMode)
            {
                global.bot.Logger.Log(logLevel, message);
            }
        }

        public static TimeSpan CreateTimeSpan(string stringRep)
        {
            //days:2 <-- stringRep string

            string[] components = stringRep.Split(':');

            int time = 0;            
            if (components.Length == 2)
            {
                time = int.Parse(components[1]);

                if (time < 1) { time = 1; }
            }

            switch (components[0].ToLower())
            {
                case "months":
                    return new TimeSpan(time * 30, 0, 0, 0);

                case "weeks":
                    return new TimeSpan(time * 7, 0, 0, 0);

                case "days":
                    return new TimeSpan(time, 0, 0, 0);

                case "hours":
                    return new TimeSpan(time, 0, 0);

                case "minutes":
                    return new TimeSpan(0, time, 0);

                case "seconds":
                    return new TimeSpan(0, 0, time);

                case "none":
                    return new TimeSpan(0);
                default:
                    throw new Exception("Invalid repetition parrern provided!");
            }
        }

        public static string[] Cachify(string src)
        {
            string[] toReturn;

            if (!src.StartsWith("https"))
            {
                toReturn = src.ToLower().Split(" ");

                for (int i = 0; i < toReturn.Length; i++)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (char c in toReturn[i])
                    {
                        int code = (int)c;

                        //check if it is not a symbol and add it
                        if ((code > 47 && code < 58) || (code > 64 && code < 91) || code > 96)
                        {
                            sb.Append(c);
                        }
                    }

                    toReturn[i] = sb.ToString();
                }
            }
            else
            {
                toReturn = new string[1];
                toReturn[0] = src;
            }

            return toReturn;
        }


        //a temporary function for extracting youtube urls, will be used until support for YouTube Data API V3 arrives
        public static string ExtractYoutubeId(string link)
        {
            //a normal youtube url contains a '='
            if (link.Length >= 43)
            {
                return link.Substring(32, 11);
            }
            //a shortened youtube url is 28 chars long
            else if (link.Length >= 28 && link.Length < 43)
            {
                return link.Substring(17, 11);
            }
            //the function doesnt support any other types
            else
            {
                throw new InvalidDataException("Invalid or unsuported url given.");
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
    }
}
