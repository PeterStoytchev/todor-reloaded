using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
            global.botConfig = await BotConfig.CreateConfig(args[0]);
            global.bot = new DiscordClient(global.botConfig.GetDiscordConfiguration());

            global.songCache = new PersistentDictionary<string, Song>(global.botConfig.songCacheDir + "kvPairs.kolba");

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
            global.commands.RegisterCommands<SoundPlayback>();
            global.commands.RegisterCommands<SpotifyCommands>();
            //global.commands.RegisterCommands<GcpCommands>();

            //voice stuffs
            global.player = new Player();
            global.googleClient = new GoogleClient(global.botConfig.youtubeServiceKey);
            global.spotify = new SpotifyClient(global.botConfig.GetClientConfig());

            //connect
            await global.bot.ConnectAsync();
            await Task.Delay(-1);

        }
        
    }
}
