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
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace todor_reloaded
{
    public class Player
    {
        private VoiceNextExtension Voice = null;

        //the song queue
        private ConcurrentQueue<Song> SongQueue = new ConcurrentQueue<Song>();
        public CancellationTokenSource cancellationTokenSourceTranscoder = new CancellationTokenSource();

        public Player()
        {
            Voice = global.bot.UseVoiceNext();
        }


        public async Task PlaySong(Song s)
        {
            VoiceNextConnection connection = Voice.GetConnection(s.ctx.Guild);
            
            if (connection != null)
            {
                if (connection.IsPlaying)
                {
                    SongQueue.Enqueue(s);

                    await s.ctx.RespondAsync($"{s.name} added to queue.");

                    return;
                }

                cancellationTokenSourceTranscoder = new CancellationTokenSource();

                //create the ffmpeg process that transcodes the file to pcm
                Process ffmpeg = PlayerUtils.CreateFFMPEGProcess(s);

                //transmit the signal to discord
                await PlayerUtils.TransmitToDiscord(connection, ffmpeg, cancellationTokenSourceTranscoder.Token);

                PlayNext(s.ctx);
            }
            else
            {
                await s.ctx.RespondAsync("Bot not in voice channel!");
            }

        }

        public async Task PlayNext(CommandContext ctx)
        {
            Song NextSong;
            if (SongQueue.TryDequeue(out NextSong))
            {
                PlaySong(NextSong);
            }
            else
            {
                await ctx.RespondAsync("End of queue");
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
               // Debug.WriteLine($"{tracker}) name: {s.uploader}");
                Debug.WriteLine($"{tracker}) name: {s.path}");
                Debug.WriteLine("====================");
            }

            await ctx.RespondAsync("Queue printed to Visual Studio debug console!");

        }

        public async Task<bool> JoinChannel(CommandContext ctx)
        {
            DiscordVoiceState CommandSenderVoiceState = ctx.Member?.VoiceState;
            
            if (CommandSenderVoiceState?.Channel == null)
            {
                //the user isnt in a voice channel, quit
                await ctx.RespondAsync("You are not in a voice channel, I don't know where to join!");
                return false;
            }

            await Voice.ConnectAsync(CommandSenderVoiceState.Channel);
            await ctx.RespondAsync("Connected to " + CommandSenderVoiceState.Channel.Name);
            return true;
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
            cancellationTokenSourceTranscoder.Cancel();
            connection.Dispose();

            //do a memory collection, just in case
            System.GC.Collect();

            await ctx.RespondAsync("Disconnected from " + ctx.Channel.Name);
        }


        public VoiceNextConnection GetVoiceConnection(DiscordGuild guild)
        {
            return Voice.GetConnection(guild);
        }

        public void ClearQueue()
        {
            SongQueue.Clear();
        }

    }

    public static class PlayerUtils
    {
        //TODO: expose the bufferSize and thread sleep time as parameters in the config
        public static async Task TransmitToDiscord(VoiceNextConnection discordConnection, Process transcoder, CancellationToken token)
        {
            await discordConnection.SendSpeakingAsync(true);

            VoiceTransmitStream discordStream = discordConnection.GetTransmitStream();

            CopyStreamSlow(transcoder.StandardOutput.BaseStream, discordStream, 1300, 5, token);

            await discordStream.FlushAsync();

            discordStream.Dispose();

            transcoder.StandardOutput.BaseStream.Dispose();
            
            transcoder.Close();

            await discordConnection.WaitForPlaybackFinishAsync();

            await discordConnection.SendSpeakingAsync(false);
        }

        public static void CopyStreamSlow(Stream src, Stream dst, int bufferSize, int sleepTime, CancellationToken token)
        {
            int count = 0;
            byte[] buffer = new byte[bufferSize];

            while ((count = src.Read(buffer, 0, bufferSize)) != 0)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                dst.Write(buffer, 0, count);

                Thread.Sleep(sleepTime);
            }

        }

        public static Process CreateFFMPEGProcess(Song s)
        {
            Debug.WriteLine("FFMPEG looking for: " + s.path);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-y -i {s.path} -ac 2 -f s16le -ar 48000 pipe:1",
                //Arguments = $"-y -i {s.file} -ac 2 -f s16le -ar 48000 pipe:1 -f wav C:/Users/TheEagle/Desktop/oof/test.wav", //this will be used later for outputing the pcm to a file so we dont have to transcode every time
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var ffmpeg = Process.Start(psi);
            return ffmpeg;
        }
    }

}
