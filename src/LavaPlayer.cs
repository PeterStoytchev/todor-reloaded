using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

//TODO: check for bugs related to conn == null
namespace todor_reloaded
{
    public class LavaPlayer
    {
        private Queue<LavaSong> m_Queue = new Queue<LavaSong>();
        private DiscordChannel lastChannel = null;
        private bool isPaused = false;

        private async Task Conn_PlaybackFinished(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            if (m_Queue.Count != 0)
            {
                var track = m_Queue.Dequeue();
                await sender.PlayAsync(track.lavaTrack);
                await track.requestChannel.SendMessageAsync($"Playing {track.lavaTrack.Title}");
                lastChannel = track.requestChannel;
            }
            else
            {
                if (lastChannel != null)
                {
                    await lastChannel.SendMessageAsync("Queue end!");
                }
                else
                {
                    await sender.Guild.GetDefaultChannel().SendMessageAsync("Queue end!");
                }

                isPaused = false;
            }
        }

        public LavaPlayer()
        {
            LavalinkConfiguration lavaConfig = global.botConfig.GetLavalinkConfiguration();
            LavalinkExtension lavalink = global.bot.UseLavalink();

            lavalink.ConnectAsync(lavaConfig).GetAwaiter().GetResult();
        }

        public async Task Play(CommandContext ctx, LavalinkTrack track)
        {
            if (track == null) { return; }

            var lava = ctx.Client.GetLavalink();
            var node = lava.GetIdealNodeConnection();
            var conn = node.GetGuildConnection(ctx.Member.Guild);

            if (conn.CurrentState.CurrentTrack == null)
            {
                await conn.PlayAsync(track);
                await ctx.Message.Channel.SendMessageAsync($"Playing {track.Title}");
            }
            else
            {
                LavaSong s = new LavaSong(track, ctx.Message.Channel);
                
                m_Queue.Enqueue(s);
                await ctx.Message.Channel.SendMessageAsync($"Track added to queue. Currently there are {m_Queue.Count} tracks in the queue!");
            }
        }

        public async Task PlayTracks(CommandContext ctx, IEnumerable<LavalinkTrack> tracks)
        {
            await PlayTracksList(ctx, new List<LavalinkTrack>(tracks));
        }

        public async Task PlayTracksList(CommandContext ctx, List<LavalinkTrack> tracks)
        {
            if (tracks == null) { return; }

            var lava = ctx.Client.GetLavalink();
            var node = lava.GetIdealNodeConnection();
            var conn = node.GetGuildConnection(ctx.Member.Guild);

            if (conn.CurrentState.CurrentTrack == null)
            {
                int index = 0;
                foreach (LavalinkTrack track in tracks)
                {
                    if (index != 0)
                    {
                        LavaSong s = new LavaSong(track, ctx.Message.Channel);

                        m_Queue.Enqueue(s);
                    }
                    else
                    {
                        await conn.PlayAsync(track);
                        await ctx.Message.Channel.SendMessageAsync($"Playing {track.Title}");
                        
                        index++;
                    }
                }
            }
            else
            {
                foreach (LavalinkTrack track in tracks)
                {
                    await conn.PlayAsync(track);
                    await ctx.Message.Channel.SendMessageAsync($"Playing {track.Title}");
                }
            }
        }

        public async Task Skip(CommandContext ctx, int count)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.GetIdealNodeConnection();
            var conn = node.GetGuildConnection(ctx.Member.Guild);

            if (conn != null)
            {
                try
                {
                    for (int i = 0; i < count - 1; i++)
                    {
                        if (m_Queue.Count != 0)
                        {
                            m_Queue.Dequeue();
                        }
                        else
                        {
                            break;
                        }
                    }

                    await conn.SeekAsync(conn.CurrentState.CurrentTrack.Length);
                    await ctx.Message.Channel.SendMessageAsync($"Tracks skipped!");
                }
                catch (NullReferenceException e)
                {
                    await ctx.Message.Channel.SendMessageAsync($"Nothing is playing!");
                }
            }
            else
            {
                await ctx.Message.Channel.SendMessageAsync($"Not in a voice channel!");
            }
        }

        public async Task Seek(CommandContext ctx, TimeSpan seekTimeSpan)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.GetIdealNodeConnection();
            var conn = node.GetGuildConnection(ctx.Member.Guild);

            try
            {
                if (conn.CurrentState.CurrentTrack.IsSeekable)
                {
                    await conn.SeekAsync(seekTimeSpan);
                }
                else
                {
                    await ctx.Message.Channel.SendMessageAsync("The current track isn't seekable!");
                }
            }
            catch (NullReferenceException e)
            {
                await ctx.Message.Channel.SendMessageAsync("No song is currently playing!");
            }
        }

        public async Task SetVol(CommandContext ctx, int percentage)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.GetIdealNodeConnection();
            var conn = node.GetGuildConnection(ctx.Member.Guild);

            if (percentage < 0)
            {
                await ctx.Channel.SendMessageAsync("The volume percentage has to be positive!");
                return;
            }

            await conn.SetVolumeAsync(percentage);
        }

        public async Task Pause(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.GetIdealNodeConnection();
            var conn = node.GetGuildConnection(ctx.Member.Guild);

            if (isPaused)
            {
                await conn.ResumeAsync();
                isPaused = false;
                await ctx.Message.Channel.SendMessageAsync($"Resumed playback!");
            }
            else
            {
                await conn.PauseAsync();
                isPaused = true;
                await ctx.Message.Channel.SendMessageAsync($"Playback paused!");
            }
        }

        public async Task<bool> Join(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            if (lava.ConnectedNodes.Count == 0)
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return false;
            }

            var node = lava.GetIdealNodeConnection();

            var conn = node.GetGuildConnection(ctx.Member.Guild);
            if (conn == null)
            {
                await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                conn = node.GetGuildConnection(ctx.Member.Guild);
                
                conn.PlaybackFinished += Conn_PlaybackFinished;

                await ctx.RespondAsync($"Joined {ctx.Member.VoiceState.Channel.Name}!");
            }


            return true;
        }

        public async Task<bool> Leave(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            if (lava.ConnectedNodes.Count == 0)
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return false;
            }

            var node = lava.GetIdealNodeConnection();

            var conn = node.GetGuildConnection(ctx.Channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return false;
            }

            await conn.DisconnectAsync();

            m_Queue.Clear();
            isPaused = false;

            await ctx.RespondAsync($"Left {ctx.Channel.Name}!");
            
            return true;
        }

        public async Task Status(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.GetIdealNodeConnection();
            var conn = node.GetGuildConnection(ctx.Member.Guild);
            
            DiscordChannel channel = ctx.Message.Channel;
            
            try
            {
                await channel.SendMessageAsync($"Current track: {conn.CurrentState.CurrentTrack.Title}");
                await channel.SendMessageAsync($"{m_Queue.Count} tracks left in the queue!");
            }
            catch (NullReferenceException e)
            {
                await channel.SendMessageAsync($"No current track!");
            }

            await channel.SendMessageAsync($"Paused: {isPaused}");
        }

        public async Task Queue(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Song queue for guild {ctx.Guild.Name}");

            DiscordEmbedBuilder EmbedBuilder = new DiscordEmbedBuilder
            {
                Title = "Current queue",
                Color = DiscordColor.Gold,
            };

            int index = 1;
            foreach (LavaSong s in m_Queue)
            {
                try
                {
                    EmbedBuilder.AddField($"{index})", $"{ index})", true);
                    EmbedBuilder.AddField("Title", s.lavaTrack.Title, true);
                    EmbedBuilder.AddField("Uploaded by", s.lavaTrack.Author, true);
                    index++;
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
        
        public static async Task<LavalinkTrack> SearchToTrack(CommandContext ctx, string search, LavalinkSearchType searchType = LavalinkSearchType.Youtube)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.GetIdealNodeConnection();
            
            var result = await node.Rest.GetTracksAsync(search, searchType);

            if (result.LoadResultType == LavalinkLoadResultType.LoadFailed || result.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.Channel.SendMessageAsync($"Couldn't load track with name {search}");
                return null;
            }

            var enumirator = result.Tracks.GetEnumerator();
            enumirator.MoveNext();
            return enumirator.Current;
        }

        public static async Task<LavalinkTrack> UriToTrack(CommandContext ctx, string link, LavalinkSearchType searchType = LavalinkSearchType.Youtube)
        {
            if (searchType == LavalinkSearchType.Youtube)
            {
                var lava = ctx.Client.GetLavalink();
                var node = lava.GetIdealNodeConnection();

                Uri uri = new Uri(link);
                var result = await node.Rest.GetTracksAsync(uri);

                if (result.LoadResultType == LavalinkLoadResultType.LoadFailed)
                {
                    await ctx.Channel.SendMessageAsync($"Couldn't load track with url {link}");
                    return null;
                }

                var enumirator = result.Tracks.GetEnumerator();
                enumirator.MoveNext();

                return enumirator.Current;
            }
            else
            {
                string[] splitSearch = link.Split("/");
                link = $"{splitSearch[splitSearch.Length - 1]} - {splitSearch[splitSearch.Length - 2]}";

                return await SearchToTrack(ctx, link, LavalinkSearchType.SoundCloud);
            }
        }

        public static async Task<IEnumerable<LavalinkTrack>> UrisToPlaylist(CommandContext ctx, string link)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.GetIdealNodeConnection();

            Uri uri = new Uri(link);
            var result = await node.Rest.GetTracksAsync(uri);
            
            if (result.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                await ctx.Message.Channel.SendMessageAsync("Invalid Url");
                return null;
            }

            return result.Tracks;
        }
    }
}