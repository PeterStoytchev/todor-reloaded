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
using Google.Apis.YouTube.v3.Data;

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
            await global.player.LeaveChannel(ctx, true);
            global.songCache.StoreDictionary();
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
            string searchQueryCache = utils.Cachify(searchQuery);

            Song s;

            if (global.songCache.Contains(searchQueryCache))
            {
                s = global.songCache.Get(searchQueryCache);
                Debug.WriteLine("Cache hit!");
            }
            else
            {
                s = new Song(searchQuery, SongType.Youtube);
            }

            global.queueCounter++;

            s.DownloadYTDL(ctx);

            global.songCache.Add(searchQueryCache, s);

            await global.player.PlaySong(ctx, s);
        }

        [Command("playlist")]
        [Description("it plays a youtube playlist from a link")]
        [Aliases("pl")]
        public async Task YouTubePlaylist(CommandContext ctx, [Description("A link to a YouTube playlist.")] string playlistLink, [Description("How many videos to load at maximum, default 6"), Optional, DefaultParameterValue(6)] int maxVideos)
        {
            await global.player.YouTubePlaylistExecutor(ctx, playlistLink, maxVideos);
        }

        [Command("skip")]
        [Description("Skips the song that is currently playing")]
        public async Task Skip(CommandContext ctx)
        {
            await global.player.SkipExecutor(ctx);
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
