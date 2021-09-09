using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.VoiceNext;

namespace todor_reloaded
{
    public class GeneralCommands : BaseCommandModule
    {
        [Command("ping")]
        [Description("ping command madafaka")]
        public async Task PingExecutor(CommandContext ctx)
        {
            await ctx.RespondAsync($"Pong! The delay is: {ctx.Client.Ping}ms");
        }
    }
}
