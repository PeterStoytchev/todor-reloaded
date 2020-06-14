using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using System.Linq;
using System.Diagnostics;

namespace todor_reloaded
{
    public class SpotifyCommands : BaseCommandModule
    {
        [Command("spotifyPlaylist")]
        [Description("plays a playlist from spotify")]
        [Aliases("spot", "spl", "spotifypl")]
        public async Task SpotifyPlaylist(CommandContext ctx, string uri)
        {
            await global.player.SpotifyPlaylistExecutor(ctx, uri);
        }
    }
}
