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
using Microsoft.Extensions.Logging;

namespace todor_reloaded
{
    public static class utils
    {
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
