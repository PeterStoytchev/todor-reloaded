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
using Google.Apis.YouTube.v3.Data;
using SpotifyAPI.Web;
using System.Security.Cryptography;

namespace todor_reloaded
{
    public class Player
    {
        private VoiceNextExtension Voice = null;

        //the song queue
        private ConcurrentQueue<Song> SongQueue = new ConcurrentQueue<Song>();
        public CancellationTokenSource cancellationTokenSourceTranscoder = new CancellationTokenSource();
        public bool isPlaying = false;

        public Player()
        {
            Voice = global.bot.UseVoiceNext();
        }


        public async Task PlaySong(CommandContext ctx, Song s)
        {
            VoiceNextConnection connection = await GetVoiceConnection(ctx);
            
            if (connection != null)
            {
                

                if (isPlaying)
                {
                    SongQueue.Enqueue(s);

                    await ctx.RespondAsync($"{s.name} added to queue.");

                    return;
                }

                cancellationTokenSourceTranscoder = new CancellationTokenSource();

                //create the ffmpeg process that transcodes the file to pcm
                Process ffmpeg = PlayerUtils.CreateFFMPEGProcess(s);

                await ctx.RespondAsync($"Playing {s.name}");
                global.queueCounter--;
                isPlaying = true;

                //transmit the signal to discord
                await PlayerUtils.TransmitToDiscord(connection, ffmpeg, cancellationTokenSourceTranscoder.Token);

                isPlaying = false;
                //^^ potential fuck up


                await PlayNext(ctx);
            }
        }

        public async Task PlayNext(CommandContext ctx)
        {
            Song NextSong;
            if (SongQueue.TryDequeue(out NextSong))
            {
                await PlaySong(ctx, NextSong);
            }
            else if (global.queueCounter != 0)
            {
                await ctx.RespondAsync("End of queue");
            }
        }


        public async Task SpotifyPlaylistExecutor(CommandContext ctx, string uri)
        {
            if (await global.player.GetVoiceConnection(ctx) != null)
            {
                FullAlbum album = await global.spotify.Albums.Get(uri.Split(":").Last());

                Song s = new Song($"{album.Tracks.Items[0].Name} - {album.Tracks.Items[0].Artists[0].Name}", SongType.Spotify);

                s.DownloadYTDL(ctx, true);

                PlaySong(ctx, s);

                album.Tracks.Items.RemoveAt(0);

                Parallel.ForEach<SimpleTrack>(album.Tracks.Items, track =>
                {
                    string searchQuery = $"{track.Name} - {track.Artists[0].Name}";
                    string searchQueryCache = utils.Cachify(searchQuery);

                    Song s;

                    if (global.songCache.Contains(searchQueryCache))
                    {
                        s = global.songCache.Get(searchQueryCache);
                        Debug.WriteLine("Cache hit!");
                    }
                    else
                    {
                        s = new Song(searchQuery, SongType.Spotify);
                    }

                    global.songCache.Add(searchQueryCache, s);

                    s.DownloadYTDL(ctx, false);

                    SongQueue.Enqueue(s);
                });

                await ctx.RespondAsync("Playlist added to queue!");
            }
        }

        public async Task YouTubePlaylistExecutor(CommandContext ctx, string playlistLink, int maxVideos)
        {
            if (await global.player.GetVoiceConnection(ctx) != null)
            {
                playlistLink = playlistLink.Split("=").Last().Substring(0, 34);

                PlaylistItemListResponse playlist = await global.youtubeClient.GetPlaylistVideos(playlistLink, maxVideos);

                Song s = new Song(playlist.Items[0]);

                s.DownloadYTDL(ctx, true);

                PlaySong(ctx, s);

                playlist.Items.RemoveAt(0);

                Parallel.ForEach<PlaylistItem>(playlist.Items, item =>
                {
                    Song s = new Song(item);

                    s.DownloadYTDL(ctx, false);

                    SongQueue.Enqueue(s);
                });

                await ctx.RespondAsync($"Playlist added to queue!");
            }
        }

        public async Task QueueExecutor(CommandContext ctx, string outputLocation)
        {
            PrintDest dest = utils.ParseDest(outputLocation);

            await utils.OutputToConsole($"Song queue for guild {ctx.Guild.Name}", ctx, outputLocation, dest);

            int tracker = 1;
            foreach (Song s in SongQueue)
            {
                await utils.OutputToConsole("====================", ctx, outputLocation, dest);
                await utils.OutputToConsole($"{tracker}) name: {s.name}", ctx, outputLocation, dest);
                await utils.OutputToConsole($"{tracker}) type: {s.type}", ctx, outputLocation, dest);
                await utils.OutputToConsole($"{tracker}) name: {s.uploader}", ctx, outputLocation, dest);
                await utils.OutputToConsole($"{tracker}) path: {s.path}", ctx, outputLocation, dest);
                await utils.OutputToConsole("====================", ctx, outputLocation, dest);

                tracker++;
            }

            await ctx.RespondAsync($"Queue printed to {outputLocation} console!");

        }

        public async Task<bool> JoinChannel(CommandContext ctx)
        {
            DiscordVoiceState CommandSenderVoiceState = ctx.Member?.VoiceState;
            
            if (CommandSenderVoiceState?.Channel == null)
            {
                //the user isnt in a voice channel, quit
                await ctx.RespondAsync(CommonMessages.VoiceChannelCouldNotConnect);
                return false;
            }

            await Voice.ConnectAsync(CommandSenderVoiceState.Channel);
            await ctx.RespondAsync("Connected to " + CommandSenderVoiceState.Channel.Name);
            return true;
        }

        public async Task LeaveChannel(CommandContext ctx, bool shouldClearQueue)
        {
            VoiceNextConnection connection = Voice.GetConnection(ctx.Guild);
           
            if (connection == null)
            {
                // not connected
                await ctx.RespondAsync(CommonMessages.NotConnectedGuild);
                return;
            }

            // disconnect
            cancellationTokenSourceTranscoder.Cancel();
            isPlaying = false;

            if (shouldClearQueue)
            {
                SongQueue.Clear();
            }
            
            connection.Dispose();

            //do a memory collection, just in case
            System.GC.Collect();

            await ctx.RespondAsync("Disconnected from " + ctx.Channel.Name);
        }

        public async Task SkipExecutor(CommandContext ctx)
        {
            VoiceNextConnection connection = Voice.GetConnection(ctx.Guild);

            if (connection != null && isPlaying == true)
            {
                await LeaveChannel(ctx, false);

                await JoinChannel(ctx);

                isPlaying = false;

                await PlayNext(ctx);
            }
            else
            {
                await ctx.RespondAsync(CommonMessages.NoPlayingToSkip);
            }
        }


        public async Task<VoiceNextConnection> GetVoiceConnection(CommandContext ctx)
        {
            VoiceNextConnection connection = Voice.GetConnection(ctx.Guild);

            if (connection == null)
            {
                bool isConnected = await JoinChannel(ctx);

                connection = Voice.GetConnection(ctx.Guild);

                if (isConnected == false)
                {
                    return null;
                }
            }

            return connection;
        }
    }

    public static class PlayerUtils
    {
        public static async Task TransmitToDiscord(VoiceNextConnection discordConnection, Process transcoder, CancellationToken token)
        {
            await discordConnection.SendSpeakingAsync(true);

            VoiceTransmitStream discordStream = discordConnection.GetTransmitStream();

            CopyStreamSlow(transcoder.StandardOutput.BaseStream, discordStream, token);

            await discordStream.FlushAsync();

            discordStream.Dispose();

            transcoder.StandardOutput.BaseStream.Dispose();
            
            transcoder.Close();

            await discordConnection.WaitForPlaybackFinishAsync();

            await discordConnection.SendSpeakingAsync(false);
        }

        public static void CopyStreamSlow(Stream src, Stream dst, CancellationToken token)
        {
            int bufferSize = global.botConfig.transcoderBufferSize;
            int threadSleepTime = global.botConfig.transcoderThreadSleepTime;

            int count = 0;
            byte[] buffer = new byte[bufferSize];

            while ((count = src.Read(buffer, 0, bufferSize)) != 0)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                dst.Write(buffer, 0, count);

                Thread.Sleep(threadSleepTime);
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
