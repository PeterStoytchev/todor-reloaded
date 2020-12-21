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
            FullAlbum album = await global.spotify.Albums.Get(uri.Split(":").Last());

            if (album.Tracks.Items.Count != 0)
            {
                await ctx.Message.Channel.SendMessageAsync("Loading playlist...");
                if (await global.lavaPlayer.Join(ctx))
                {
                    List<LavalinkTrack> tracks = new List<LavalinkTrack>(album.Tracks.Items.Count);
                    foreach (SimpleTrack track in album.Tracks.Items)
                    {
                        LavalinkTrack lavaTrack = await LavaPlayer.SearchToTrack(ctx, $"{track.Name} - {track.Artists[0].Name}");

                        if (lavaTrack != null)
                        {
                            tracks.Add(lavaTrack);
                        }
                    }

                    tracks.TrimExcess();
                    await global.lavaPlayer.PlayTracksList(ctx, tracks);
                }
            }
            else
            {
                await ctx.Message.Channel.SendMessageAsync("Invalid link!");
            }
        }
    }
}
