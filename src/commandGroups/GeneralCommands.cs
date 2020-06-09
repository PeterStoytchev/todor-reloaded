﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace todor_reloaded
{
    public class GeneralCommands : BaseCommandModule
    {
        [Command("mute")]
        [Description("gosheto e pulen geitak")]
        [RequireOwner]
        [RequireBotPermissions(Permissions.MuteMembers)]
        public async Task MuteExecutor(CommandContext ctx, DiscordMember discordMember)
        {
            Debug.WriteLine(discordMember.ToString());

            if (discordMember.IsMuted)
            {
                await discordMember.SetMuteAsync(true);
            }
            else
            {
                await discordMember.SetMuteAsync(false);
            }

        }


        [Command("move")]
        [Description("moves everyone from one discord channel to another")]
        [Aliases("m")]
        public async Task MoveExecutor(CommandContext ctx, [Description("The channel to move from.")] string fromChannel, [Description("The channel to move to.")] string toChannel)
        {
            IReadOnlyList<DiscordChannel> channels = await ctx.Guild.GetChannelsAsync();

            DiscordChannel channelFrom = null;
            DiscordChannel channelTo = null;

            foreach (DiscordChannel channel in channels)
            {
                if (channel.Type == ChannelType.Voice)
                {
                    if (channel.Name == fromChannel)
                    {
                        channelFrom = channel;
                    }
                    else if (channel.Name == toChannel)
                    {
                        channelTo = channel;
                    }
                }
            }

            if (channelFrom == null)
            {
                await ctx.RespondAsync($"Couldn't find channel named {fromChannel}");
                return;
            }

            if (channelTo == null)
            {
                await ctx.RespondAsync($"Couldn't find channel named {toChannel}");
                return;
            }

            List<Task> moveTasks = new List<Task>();

            foreach (DiscordMember user in channelFrom.Users)
            {
                moveTasks.Add(channelTo.PlaceMemberAsync(user));
            }

            await Task.WhenAll(moveTasks);

            await ctx.RespondAsync($"Everyone has been moved from {channelFrom.Name} to {channelTo.Name}");
        }


        [Command("ping")]
        [Description("ping command madafaka")]
        public async Task PingExecutor(CommandContext ctx)
        {
            await ctx.RespondAsync($"Pong! The delay is: {ctx.Client.Ping}ms");
        }
    }
}
