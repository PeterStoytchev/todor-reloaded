﻿using DSharpPlus.CommandsNext;
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
