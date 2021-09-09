using System.Threading.Tasks;

using System.Collections.Generic;
using System;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;

namespace todor_reloaded
{
    class LavaSoundPlayback : BaseCommandModule
    {
        [Command("play")]
        [Description("Plays a song")]
        [Aliases("p")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            await SafeCall(ctx, async () =>
            {
                if (await global.lavaPlayer.Join(ctx))
                {
                    LavalinkTrack track;
                    if (search.StartsWith("http://") || search.StartsWith("https://"))
                    {
                        track = await LavaPlayer.UriToTrack(ctx, search);
                    }
                    else
                    {
                        track = await LavaPlayer.SearchToTrack(ctx, search);
                    }

                    await global.lavaPlayer.Play(ctx, track);
                }
            });
        }

        [Command("playlist")]
        [Description("Plays a song video")]
        [Aliases("pl")]
        public async Task Playlist(CommandContext ctx, string link)
        {
            await SafeCall(ctx, async () =>
            {
                if (await global.lavaPlayer.Join(ctx))
                {
                    var tracks = await LavaPlayer.UrisToPlaylist(ctx, link);

                    await global.lavaPlayer.PlayTracks(ctx, tracks);
                }
            });
        }

        [Command("soundcloud")]
        [Description("Plays a song video from soundcloud.")]
        [Aliases("sc")]
        public async Task PlaySoundcloud(CommandContext ctx, [RemainingText] string search)
        {
            await SafeCall(ctx, async () =>
            {
                await ctx.Channel.SendMessageAsync($"This is a maintance function. Abuse petkata to fix it!");

                if (await global.lavaPlayer.Join(ctx))
                {
                    if (search.StartsWith("http://") || search.StartsWith("https://"))
                    {
                        string[] splitSearch = search.Split("/");
                        search = $"{splitSearch[splitSearch.Length - 1]} - {splitSearch[splitSearch.Length - 2]}";
                    }

                    LavalinkTrack track = await LavaPlayer.SearchToTrack(ctx, search, LavalinkSearchType.SoundCloud);

                    await global.lavaPlayer.Play(ctx, track);
                }
            });
        }

        [Command("seek")]
        [Description("Seeks into the song.")]
        [Aliases("ds")]
        public async Task PlayDirectSeek(CommandContext ctx, int seconds)
        {
            await SafeCall(ctx, async () =>
            {
                await global.lavaPlayer.Seek(ctx, new TimeSpan(0, 0, seconds));
            });
        }

        [Command("volume")]
        [Description("Sets the playback volume.")]
        [Aliases("vol")]
        public async Task SetVolume(CommandContext ctx, int percentage)
        {
            await SafeCall(ctx, async () =>
            {
                await global.lavaPlayer.SetVol(ctx, percentage);
            });
        }

        [Command("pause")]
        [Description("Plays a song video")]
        public async Task Pause(CommandContext ctx)
        {
            await SafeCall(ctx, async () =>
            {
                await global.lavaPlayer.Pause(ctx);
            });
        }

        [Command("skip")]
        [Description("Skips a song.")]
        [Aliases("ss")]
        public async Task Skip(CommandContext ctx, int count = 1)
        {
            await SafeCall(ctx, async () =>
            {
                await global.lavaPlayer.Skip(ctx, count);
            });
        }

        [Command("queue")]
        [Description("Prints the current queue.")]
        [Aliases("q")]
        public async Task Queue(CommandContext ctx)
        {
            await SafeCall(ctx, async () =>
            {
                await global.lavaPlayer.Queue(ctx);
            });
        }

        [Command("leave")]
        [Description("Leaves a voice channel.")]
        [Aliases("stop", "l", "aufwiedersehen")]
        public async Task Leave(CommandContext ctx)
        {
            await SafeCall(ctx, async () =>
            {
                await global.lavaPlayer.Leave(ctx);
            });
        }

        private async Task SafeCall(CommandContext ctx, Func<Task> func)
        {
            ulong channelId = global.botConfig.discordMusicChannelId;
            if (ctx.Message.ChannelId == channelId)
            {
                await func();
            }
            else
            {
                await ctx.Message.DeleteAsync();
                await ctx.Member.SendMessageAsync($"The bot can only be used in channel '{ctx.Guild.GetChannel(channelId).Name}'");
            }
        }
    }
}
