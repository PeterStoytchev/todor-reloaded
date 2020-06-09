using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;

namespace todor_reloaded
{

    public class SoundPlayback : BaseCommandModule
    {
        [Command("join")]
        [Description("Joins the voice channel that you are in.")]
        [Aliases("joint", "j")]
        public async Task Join(CommandContext ctx)
        {
            // check for connection
            VoiceNextConnection connection = global.player.GetVoiceConnection(ctx.Guild);
            if (connection != null)
            {
                // already connected
                await ctx.RespondAsync("Already connected in this guild.");
                return;
            }
            
            await global.player.JoinChannel(ctx);
        }

        [Command("leave")]
        [Description("Leaves a voice channel.")]
        [Aliases("l", "aufwiedersehen")]
        public async Task Leave(CommandContext ctx)
        {
            await global.player.LeaveChannel(ctx);
            global.player.ClearQueue(); //clear the queue when leaving
        }

        [Command("play")]
        [Description("Plays a youtube video")]
        [Aliases("p")]
        public async Task Play(CommandContext ctx, [Description("A search query for youtube or a direct link")] params string[] search)
        {
            // check for connection
            VoiceNextConnection connection = global.player.GetVoiceConnection(ctx.Guild);
            if (connection == null)
            {
                bool isConnected = await global.player.JoinChannel(ctx);

                if (isConnected == false)
                {
                    return;
                }
            }

            string searchQuery = utils.ArrayToString(search, ' ');

            Song s = new Song(searchQuery, SongType.Youtube, ctx);
            global.queueCounter++;

            s.DownloadYTDL();

            global.player.PlaySong(s);
        }

        [Command("skip")]
        [Description("Skips the song that is currently playing")]
        public async Task Skip(CommandContext ctx)
        {
            VoiceNextConnection connection = global.player.GetVoiceConnection(ctx.Guild);

            if (connection != null && connection.IsPlaying == true)
            {
                global.player.cancellationTokenSourceTranscoder.Cancel();

                global.player.PlayNext(ctx);
            }
            else
            {
                await ctx.RespondAsync("No song is playing for me to skip!");
            }

        }


        [Command("debugqueue")]
        [Description("a command that pritns the queeue to the visual studio debug console, do not use")]
        [RequireOwner]
        public async Task Queue(CommandContext ctx)
        {
            await global.player.QueueExecutor(ctx);

        }
    }
}
