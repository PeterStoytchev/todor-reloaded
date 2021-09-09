using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using System.Linq;
using System.Diagnostics;

namespace todor_reloaded
{
    public class LavaSpotifyCommands : BaseCommandModule
    {
        [Command("spotifyPlaylist")]
        [Description("plays a playlist from spotify")]
        [Aliases("spot", "spl", "spotifypl")]
        public async Task SpotifyPlaylist(CommandContext ctx, string uri)
        {
            await SafeCall(ctx, async () =>
            {
                await ctx.Message.Channel.SendMessageAsync("Loading...");

                List<LavalinkTrack> tracks = new List<LavalinkTrack>();

                if (uri.Contains("album"))
                {
                    uri = uri.Substring(31, 22);
                    FullAlbum album = await global.spotify.Albums.Get(uri);
                    foreach (SimpleTrack track in album.Tracks.Items)
                    {
                        LavalinkTrack lavaTrack = await LavaPlayer.SearchToTrack(ctx, $"{track.Name} - {track.Artists[0].Name}");

                        if (lavaTrack != null)
                        {
                            tracks.Add(lavaTrack);
                        }
                    }
                }
                else if (uri.Contains("playlist"))
                {
                    await ctx.Message.Channel.SendMessageAsync("Playlists are not supported yet!");
                    //uri = uri.Substring(34, 22);
                    //FullPlaylist playlist = await global.spotify.Playlists.Get(uri);
                }


                if (await global.lavaPlayer.Join(ctx) && tracks.Count != 0)
                {
                    await global.lavaPlayer.PlayTracksList(ctx, tracks);
                }
                else
                {
                    await ctx.Message.Channel.SendMessageAsync("Invalid link!");
                }
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
