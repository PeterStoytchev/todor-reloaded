using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace todor_reloaded
{
    class NotificaitonCommands : BaseCommandModule
    {
        [Command("subscribe")]
        [Description("It subscribes you to a notifications channel.")]
        [Aliases("sub")]
        public async Task SubscribeExecutor(CommandContext ctx, string groupName)
        {
            await ctx.Channel.SendMessageAsync("Not implemented!");

            //await global.notificationSystem.SubscribeUserToGroup(ctx, groupName);
        }
    }
}
