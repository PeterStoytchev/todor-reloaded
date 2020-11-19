using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using System.Security.Cryptography;
using Google.Apis.Compute.v1.Data;
using System.Runtime.InteropServices;

namespace todor_reloaded
{
    [Description("Commands that are ment to be executed only by the bot owner (petkata)")]
    [RequireOwner]
    public class OwnerCommands : BaseCommandModule
    {
        [Command("printCache")]
        [Description("Prints the contents of the song cache k/v to the console.")]
        [Aliases("pc")]
        public async Task PrintCache(CommandContext ctx, string destStr)
        {
            var pairs = global.songCache.GetPairs();

            int index = 1;

            PrintDest dest = utils.ParseDest(destStr);

            foreach (var pair in pairs)
            {
                string[] details = pair.Value.GetDebug();

                for (int i = 0; i < details.Length - 1; i++)
                {
                    string toPrint = $"{index}) {details[i]}";

                    await utils.OutputToConsole(toPrint, ctx, destStr, dest);
                }

                index++;
            }

            await ctx.RespondAsync($"Cache printed to {destStr} console");
        }

        [Command("reloadConfig")]
        [Description("Reloads the bot configuration from config.json")]
        public async Task ReloadConfigExecutor(CommandContext ctx)
        {
            try
            {
                global.botConfig = await BotConfig.CreateConfig(global.botConfig.configDir);
                await ctx.RespondAsync("Bot configuration reloaded!");
            }
            catch (FileNotFoundException fileException)
            {
                await ctx.RespondAsync($"Couldn't find config file at: {global.botConfig.configDir}"); 
            }

        }

        [Command("debugChannels")]
        [Aliases("dc")]
        public async Task debugChannels(CommandContext ctx)
        {
            Debug.WriteLine("Printing notificaiton groups");
            /*
            foreach (NotificationGroup notificationGroup in global.notificationSystem.GetGroups())
            {
                Debug.WriteLine(notificationGroup.m_Name);
            }
            */
        }
    }

}
