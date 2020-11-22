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
            else if (global.queueCounter == 0)
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

                for (int i = 0; i < album.Tracks.Items.Count; i++)
                {
                    SimpleTrack track = album.Tracks.Items[i];
                    string searchQuery = $"{track.Name} - {track.Artists[0].Name}";
                    string[] searchQueryCache = utils.Cachify(searchQuery);

                    Song s1 = global.songCache.Get(searchQueryCache);
                    if (s1 != null)
                    {
                        Debug.WriteLine("Cache hit!");
                    }
                    else
                    {
                        s1 = new Song(utils.ArrayToString(searchQueryCache, ' '), SongType.Youtube);
                        global.songCache.Add(searchQueryCache, s1);

                    }

                    s1.DownloadYTDL(ctx, false);

                    SongQueue.Enqueue(s1);
                }

                await ctx.RespondAsync("Playlist added to queue!");
            }
        }

        public async Task YouTubePlaylistExecutor(CommandContext ctx, string playlistLink, int maxVideos)
        {
            if (await global.player.GetVoiceConnection(ctx) != null)
            {
                playlistLink = playlistLink.Split("=").Last().Substring(0, 34);

                PlaylistItemListResponse playlist = await global.googleClient.GetPlaylistVideos(playlistLink, maxVideos);

                Song s = new Song(playlist.Items[0]);

                s.DownloadYTDL(ctx, true);

                PlaySong(ctx, s);

                playlist.Items.RemoveAt(0);

                foreach (var item in playlist.Items)
                {
                    Song s1 = new Song(item);

                    s1.DownloadYTDL(ctx, false);

                    SongQueue.Enqueue(s1);
                }

                await ctx.RespondAsync($"Playlist added to queue!");
            }
        }

        public async Task DebugQueueExecutor(CommandContext ctx)
        {
            utils.DebugLog($"Song queue for guild {ctx.Guild.Name}");

            int tracker = 1;
            foreach (Song s in SongQueue)
            {
                utils.DebugLog("====================");
                utils.DebugLog($"{tracker}) name: {s.name}");
                utils.DebugLog($"{tracker}) type: {s.type}");
                utils.DebugLog($"{tracker}) uploader: {s.uploader}");
                utils.DebugLog($"{tracker}) path: {s.path}");
                utils.DebugLog("====================");

                tracker++;
            }

            await ctx.RespondAsync($"Queue printed to console!");
        }

        public async Task QueueExecutor(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Song queue for guild {ctx.Guild.Name}");

            DiscordEmbedBuilder EmbedBuilder = new DiscordEmbedBuilder
            {
                Title = "Current queue",
                Color = DiscordColor.Gold,
            };

            foreach (Song s in SongQueue)
            { 
                try
                {
                    EmbedBuilder.AddField("Title", s.name, true);
                    EmbedBuilder.AddField("Uploaded by", s.uploader, true);
                    EmbedBuilder.AddField("==========", "==========");
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync(embed: EmbedBuilder.Build());

                    EmbedBuilder = new DiscordEmbedBuilder
                    {
                        Title = "Current queue",
                        Color = DiscordColor.Gold,
                    };
                }
            }

            if (EmbedBuilder.Fields.Count != 0)
            {
                await ctx.Channel.SendMessageAsync(embed: EmbedBuilder.Build());
            }

        }

        public async Task<bool> JoinChannel(CommandContext ctx)
        {
            DiscordVoiceState commandSenderVoiceState = ctx.Member?.VoiceState;
            
            if (commandSenderVoiceState?.Channel == null)
            {
                //the user isnt in a voice channel, quit
                await ctx.RespondAsync(CommonMessages.VoiceChannelCouldNotConnect);
                return false;
            }

            await Voice.ConnectAsync(commandSenderVoiceState.Channel);
            await ctx.RespondAsync("Connected to " + commandSenderVoiceState.Channel.Name);
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
            GC.Collect();

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


        public static void CopyStreamSlow(Stream input, Stream output, CancellationToken token)
        {
            int bufferSize = global.botConfig.transcoderBufferSize;
            int threadSleepTime = global.botConfig.transcoderThreadSleepTime;

            byte[] buffer = new byte[bufferSize];
            int read;

            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                output.Write(buffer, 0, read);

                Thread.Sleep(threadSleepTime);
            }
        }

        public static Process CreateFFMPEGProcess(Song s)
        {
            Debug.WriteLine("FFMPEG looking for: " + s.path);


            var psi = new ProcessStartInfo
            {
                FileName = $"{global.botConfig.ffmpegPath}",
                Arguments = $"-y -i {s.path} -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var ffmpeg = Process.Start(psi);
            return ffmpeg;
        }
    }

}
