﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace todor_reloaded
{
    class PrivateChannelCommands : BaseCommandModule
    {
        [Command("allow")]
        [Description("Can be used by the current private channel owner, to allow somebody to join it.")]
        [Aliases("a", "grantasylum", "gs")]
        public async Task Allow(CommandContext ctx, DiscordMember member)
        {
            if (global.pcm != null)
            {
               if (global.pcm.AllowMember(ctx.User.Id, member.Id))
               {
                    await ctx.RespondAsync($"{member.Username} has been added to the whitelist!");
               }
               else
               {
                    await ctx.RespondAsync("Only the current channel owner can allow you enrty into the private channel!");
               }
            }
            else
            {
                await ctx.RespondAsync("The private channel feature is not enabled! Contact Kiro Slepoto!");
            }
        }

        [Command("gowner")]
        [Aliases("go")]
        public async Task Allow(CommandContext ctx)
        {
            await ctx.RespondAsync("THIS IS A DEBUG COMMAND. REMOVE IT!");
            ulong ownerid = global.pcm.GetOwner();

            if (ownerid == 0)
            {
                await ctx.RespondAsync("No current owner!");
                return;
            }

            DiscordMember m = await ctx.Guild.GetMemberAsync(ownerid);
            await ctx.RespondAsync($"Current owner: {m.Username}");
        }
    }
}