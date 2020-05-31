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

        //the key is a DiscordGuild id, the value is a DiscorChannel id
        private Dictionary<ulong, ulong> CurrentVoiceChannels = new Dictionary<ulong, ulong>();

        //the key is a DiscordGuild id, the key is a queue of songs for that guild
        private Dictionary<ulong, Queue<Song>> SongQueue = new Dictionary<ulong, Queue<Song>>();

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

            CurrentVoiceChannels.Add(ctx.Guild.Id, CommandSenderVoiceState.Channel.Id);

            await Voice.ConnectAsync(CommandSenderVoiceState.Channel);
            await ctx.RespondAsync("Connected to " + CommandSenderVoiceState.Channel.Name);
        }

        public async Task LeaveChannel(CommandContext ctx)
        {
            VoiceNextConnection connection = Voice.GetConnection(ctx.Guild);
           
            if (connection == null)
            {
                // not connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            // disconnect
            connection.Disconnect();
            await ctx.RespondAsync("Disconnected from " + ctx.Channel.Name);


            //run garbadge collection, helps with memory usage a after lots of songs have been player, this isnt a great idea but it will stay as it is for the time being
            System.GC.Collect();
        }

    }
}
