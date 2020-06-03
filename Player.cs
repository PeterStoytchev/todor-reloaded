using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.VoiceNext;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.ComTypes;
using System.Linq;

namespace todor_reloaded
{
    public class Player
    {
        private VoiceNextExtension Voice = null;

        //the song queue
        private ConcurrentQueue<Song> SongQueue = new ConcurrentQueue<Song>();

        public Player()
        {
            Voice = global.bot.UseVoiceNext();
        }

        public async Task PlaySong(Song s)
        {
            VoiceNextConnection connection = Voice.GetConnection(s.ctx.Guild);

            if (connection != null)
            {
                String newName = s.url.Split("/").Last();

                if (connection.IsPlaying)
                {
                    //if we are already playing something, add the song to the queue

                    //if the song isnt downloaded, download it
                    if (s.file == null)
                    {
                        s.file = PlayerUtils.DownloadYTDL(s.url, newName);
                        await s.ctx.RespondAsync("Loading " + s.url);
                    }

                    SongQueue.Enqueue(s);

                    return;
                }

                if (s.file == null)
                {
                    s.ctx.RespondAsync($"Loading {s.url}, please wait for playback to start.....");
                    s.file = PlayerUtils.DownloadYTDL(s.url, newName);
                }

                //create the ffmpeg process that transcodes the file to pcm
                Process ffmpeg = PlayerUtils.CreateFFMPEGProcess(s.file);

                //transmit the signal to discord
                await PlayerUtils.TransmitToDiscord(connection, ffmpeg);

                Song NextSong;

                if (SongQueue.TryDequeue(out NextSong))
                {
                    PlaySong(NextSong);
                }
                else
                {
                    await s.ctx.RespondAsync("End of queue");
                }
            }
            else
            {
                await s.ctx.RespondAsync("Bot not in voice channel!");
            }

        }

        public async Task QueueExecutor(CommandContext ctx)
        {
            Debug.WriteLine($"Song queue for guild {ctx.Guild.Name}");

            int tracker = 1;
            foreach (Song s in SongQueue)
            {
                Debug.WriteLine("====================");
                Debug.WriteLine($"{tracker}) name: {s.name}");
                Debug.WriteLine($"{tracker}) name: {s.type}");
                Debug.WriteLine($"{tracker}) name: {s.uploader}");
                Debug.WriteLine($"{tracker}) name: {s.file}");
                Debug.WriteLine("====================");
            }

            await ctx.RespondAsync("Queue printed to Visual Studio debug console!");

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

    public static class PlayerUtils
    {
        public static String DownloadYTDL(string link, String newName)
        {

            ProcessStartInfo downloadPsi = new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = @$"{link} --no-playlist -x --audio-format {global.botConfig.fileExtention} -o {newName}.{global.botConfig.fileExtention}",
                RedirectStandardOutput = false,
                UseShellExecute = false
            };
            var ytdl = Process.Start(downloadPsi);

            ytdl.WaitForExit();

            return $"{newName}.{global.botConfig.fileExtention}";
        }
        public static async Task TransmitToDiscord(VoiceNextConnection discordConnection, Process transcoder)
        {
            await discordConnection.SendSpeakingAsync(true);

            VoiceTransmitStream discordStream = discordConnection.GetTransmitStream();

            await transcoder.StandardOutput.BaseStream.CopyToAsync(discordStream);

            await discordStream.FlushAsync();

            await transcoder.StandardOutput.BaseStream.FlushAsync();

            transcoder.Close();

            await discordConnection.WaitForPlaybackFinishAsync();

            await discordConnection.SendSpeakingAsync(false);
        }

        public static Process CreateFFMPEGProcess(string filename)
        {

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {filename} -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var ffmpeg = Process.Start(psi);
            return ffmpeg;
        }
    }

}
