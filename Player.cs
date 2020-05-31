using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.VoiceNext;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace todor_reloaded
{
    public class Player
    {
        private VoiceNextExtension Voice = null;

        //the ulong is a discord guild id
        private Dictionary<ulong, DiscordChannel> VoiceChannels = new Dictionary<ulong, DiscordChannel>();

        private static Queue<Song> songs = new Queue<Song>();

        public Player()
        {
            Voice = global.bot.UseVoiceNext();
        }

        public async Task JoinChannel(CommandContext ctx)
        {
            // check for connection
            VoiceNextConnection connection = Voice.GetConnection(ctx.Guild);
            if (connection != null)
            {
                // already connected
                await ctx.RespondAsync("Already connected in this guild.");
                return;
            }

            DiscordVoiceState CommandSenderVoiceState = ctx.Member?.VoiceState;
            
            if (CommandSenderVoiceState?.Channel == null)
            {
                //the user isnt in a voice channel, quit
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            
            await Voice.ConnectAsync(CommandSenderVoiceState.Channel);
            await ctx.RespondAsync("Connected to " + CommandSenderVoiceState.Channel.Name);
        }

    }
}
