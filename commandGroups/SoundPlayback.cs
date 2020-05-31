﻿using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace todor_reloaded
{

    public class SoundPlayback : BaseCommandModule
    {

        
        [Command("join"), Description("Joins a voice channel.")]
        public async Task Join(CommandContext ctx, DiscordChannel chn = null)
        {
            // check whether VNext is enabled
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we aren't already connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                // already connected
                await ctx.RespondAsync("Already connected in this guild.");
                return;
            }

            // get member's voice state
            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && chn == null)
            {
                // they did not specify a channel and are not in one
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            // channel not specified, use user's
            if (chn == null)
                chn = vstat.Channel;

            // connect
            vnc = await vnext.ConnectAsync(chn);
            await ctx.RespondAsync($"Connected to `{chn.Name}`");
        }

        [Command("leave"), Description("Leaves a voice channel.")]
        public async Task Leave(CommandContext ctx)
        {
            // check whether VNext is enabled
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we are connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // not connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            // disconnect
            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected");

            System.GC.Collect();
        }

        [Command("queue")]
        public async Task Queue(CommandContext ctx)
        {
            foreach (string str in songs)
            {
                await ctx.Channel.SendMessageAsync(str);
            }

            await ctx.Channel.SendMessageAsync("queue printed");

        }


        [Command("play"), Description("Plays a youtube video")]
        public async Task Play(CommandContext ctx, [Description("Youtube url")] string filename)
        {
            var vnc = ctx.Client.GetVoiceNext().GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // already connected
                await Join(ctx);
                vnc = ctx.Client.GetVoiceNext().GetConnection(ctx.Guild);
            }

            if (vnc.IsPlaying)
            {
                songs.Enqueue(filename);
                return;
            }

            // play

            String file = utils.DownloadYTDL(filename);

            Process ffmpegProcess = utils.CreateFFMPEGProcess(file);

            await ctx.Message.RespondAsync($"Playing `{file}`");

            await vnc.SendSpeakingAsync(true);

            await utils.TransmitToDiscord(vnc, ffmpegProcess);

            await vnc.SendSpeakingAsync(false);
            await ctx.Message.RespondAsync($"Finished playing `{file}`");

            string newSong = null;

            if (songs.TryDequeue(out newSong))
            {
                Play(ctx, newSong);
            }
            else
            {
                Leave(ctx);
            }

        }
    }
}
