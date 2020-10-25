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
            //connect if not connected
            VoiceNextConnection conn = await global.player.GetVoiceConnection(ctx);

            if (conn != null)
            {
                await ctx.RespondAsync($"Connected to {ctx.Member.VoiceState.Channel.Name}");
            }

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
            Song s;
            if (!search[0].StartsWith("http"))
            {
                for (int i = 0; i < search.Length; i++)
                {
                    search[i] = search[i].ToLower();
                }
            }

            s = global.songCache.Get(search);

            if (s != null)
            {
                Debug.WriteLine("Cache hit!");
            }
            else
            {
                s = new Song(utils.ArrayToString(search, ' '), SongType.Youtube);
            }


            global.queueCounter++;

            s.DownloadYTDL(ctx, true);
            
            global.songCache.Add(search, s);

            await global.player.PlaySong(ctx, s);
        }

        [Command("playlist")]
        [Description("it plays a youtube playlist from a link")]
        [Aliases("pl")]
        public async Task YouTubePlaylist(CommandContext ctx, [Description("A link to a YouTube playlist.")] string playlistLink, [Description("How many videos to load at maximum, default 15"), Optional, DefaultParameterValue(15)] int maxVideos)
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
        public async Task Queue(CommandContext ctx, [Optional, DefaultParameterValue("")] string printDest)
        {
            await global.player.QueueExecutor(ctx, printDest);

        }
    }
}
