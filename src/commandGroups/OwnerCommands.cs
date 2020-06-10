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

namespace todor_reloaded
{
    [Description("Commands that are ment to be executed only by the bot owner (petkata)")]
    [RequireOwner]
    public class OwnerCommands : BaseCommandModule
    {
        [Command("printCache")]
        [Description("Prints the contents of the song cache k/v to the console.")]
        [Aliases("pc")]
        public async Task PrintCache(CommandContext ctx)
        {
            var pairs = global.songCache.GetPairs();

            int index = 1;
            foreach (var pair in pairs)
            {
                string[] details = pair.Value.GetDebug();

                for (int i = 0; i < details.Length - 1; i++)
                {
                    await ctx.RespondAsync($"{index}) {details[i]}");
                }
            }
        }

        [Command("gc")]
        [Description("do not run")]
        public async Task GCExecutor(CommandContext ctx)
        {
            System.GC.Collect();
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
    }
}
