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

namespace todor_reloaded
{
    class Program
    {
        public static void Main(string[] args)
        {
            var prog = new Program();
            prog.RunBotAsync(args).GetAwaiter().GetResult();
        }


        public async Task RunBotAsync(string[] args)
        {
            //bot init 
            global.botConfig = await BotConfig.CreateConfig("C:/Users/Seph/Desktop/todor-reloaded/config.json");
            global.bot = new DiscordClient(global.botConfig.GetDiscordConfiguration());


            //bot events
            global.bot.Ready += BotEvents.Bot_Ready;
            global.bot.ClientErrored += BotEvents.Bot_ClientErrored;

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
