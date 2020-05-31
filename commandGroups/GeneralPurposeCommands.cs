using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;


namespace todor_reloaded
{
    public class GeneralPurposeCommands : BaseCommandModule
    {
        [Command("gc")]
        [Description("do not run")]
        public async Task GCExecutor(CommandContext ctx)
        {
            System.GC.Collect();
        }

        [Command("ping")]
        [Description("ping command madafaka")]
        public async Task PingExecutor(CommandContext ctx)
        {
            await ctx.RespondAsync($"Pong! The delay is: {ctx.Client.Ping}ms");
        }

        [Command("add")]
        [Description("adds two shits together")]
        [Aliases("sum")]
        public async Task SayExecutor(CommandContext ctx, int shit1, int shit2)
        {
            await ctx.RespondAsync(Convert.ToString(shit1 + shit2));
        }
    }
}
