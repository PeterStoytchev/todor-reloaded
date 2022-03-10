using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using DSharpPlus.Entities;

namespace todor_reloaded
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            //bot init 
            global.botConfig = await BotConfig.CreateConfig(args[0]);
            global.bot = new DiscordClient(global.botConfig.GetDiscordConfiguration());

            //Setup the private channel feature
            global.pcm = null;
            ulong privateChannelId = 0;
            if (ulong.TryParse(global.botConfig.privateChannelId, out privateChannelId))
            {
                DiscordChannel ch = await global.bot.GetChannelAsync(privateChannelId);
                global.pcm = new PrivateChannelManager(ch);

                //register the commands for it
                global.commands.RegisterCommands<PrivateChannelCommands>();
            }

            //bot events
            global.bot.Ready += BotEvents.Bot_Ready;
            global.bot.ClientErrored += BotEvents.Bot_ClientErrored;
            global.bot.VoiceStateUpdated += BotEvents.Bot_VoiceStateUpdated;

            //commands setup and events
            global.commands = global.bot.UseCommandsNext(global.botConfig.GetCommandsNextConfiguration());
            global.commands.CommandExecuted += BotEvents.Commands_CommandExecuted;
            global.commands.CommandErrored += BotEvents.Commands_CommandErrored;

            //command definitions
            global.commands.RegisterCommands<GeneralCommands>();
            global.commands.RegisterCommands<OwnerCommands>();

            //connect
            await global.bot.ConnectAsync();

            global.lavaPlayer = new LavaPlayer();
            global.commands.RegisterCommands<LavaSoundPlayback>();

            if (global.botConfig.spotifyEnabled)
            {
                global.spotify = new SpotifyClient(global.botConfig.GetClientConfig());
                global.commands.RegisterCommands<LavaSpotifyCommands>();
            }

            await Task.Delay(-1);
        }

        
    }
}
