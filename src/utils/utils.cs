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
        public static async Task OutputToConsole(string str, CommandContext ctx, string printDest = "", PrintDest dest = PrintDest.Empty)
        {
            if (dest == PrintDest.Empty)
            {
                dest = ParseDest(printDest);
            }

            switch (dest)
            {
                case PrintDest.VSDebug:
                    Debug.WriteLine(str);
                    break;

                case PrintDest.ConsoleOut:
                    LogMessage(str);
                    break;

                case PrintDest.DiscordChn:
                    await ctx.Channel.SendMessageAsync(str);
                    break;
            }
        }

        public static PrintDest ParseDest(string str)
        {
            //for later: load these from config.json
            string[] vsdebug = { "debug", "vsdebug", "debugconsole" };
            string[] dischannel = { "discord", "discordchannel" };

            str = str.ToLower();

            if (vsdebug.Contains(str))
            {
                return PrintDest.VSDebug;
            }
            else if (dischannel.Contains(str))
            {
                return PrintDest.DiscordChn;
            }

            return PrintDest.ConsoleOut;
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

        public static void LogMessage(string msg)
        {
            //client.DebugLogger.LogMessage(logLevel, client.CurrentUser.Username, msg, DateTime.Now);
            //client.Logger.LogDebug(msg);
            Console.WriteLine(msg);
        }
    }

    public enum PrintDest
    {
        VSDebug,
        ConsoleOut,
        DiscordChn,
        Empty
    }


}
