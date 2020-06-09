﻿using DSharpPlus.CommandsNext;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace todor_reloaded
{
    public class Song
    { 
        public SongType type { get; private set; }
        public CommandContext ctx { get; private set; }

        public string url { get; private set; }
        public string path { get; private set; }

        public string name { get; private set; }
        public string uploader { get; private set; }
        public string videoId { get; private set; }
        public DateTime? publishedAt { get; private set; }

        public Thumbnail thumbnail { get; private set; }

        public Song(string query, SongType type, CommandContext ctx)
        {
            if (query.StartsWith("https://"))
            {
                SearchResult result = global.tubeUtils.SearchForVideo(query).GetAwaiter().GetResult();
                SearchResultSnippet snippet = result.Snippet;

                this.name = snippet.Title;
                this.uploader = snippet.ChannelTitle;
                this.publishedAt = snippet.PublishedAt;
                this.thumbnail = snippet.Thumbnails.Standard;
                this.url = $"https://youtu.be/{result.Id.VideoId}";
                this.type = type;
                this.ctx = ctx;
                this.videoId = result.Id.VideoId;
            }
            else
            {
                Video result = global.tubeUtils.GetVideoDetails(query).GetAwaiter().GetResult();
                VideoSnippet snippet = result.Snippet;

                this.name = snippet.Title;
                this.uploader = snippet.ChannelTitle;
                this.publishedAt = snippet.PublishedAt;
                this.thumbnail = snippet.Thumbnails.Standard;
                this.url = $"https://youtu.be/{result.Id}";
                this.type = type;
                this.ctx = ctx;
                this.videoId = result.Id;
            }

        }

        
        public void DownloadYTDL()
        {
            ctx.RespondAsync($"Loading {name}");

            BotConfig CurrentConfig = global.botConfig;

            path = $"{CurrentConfig.songCacheDir}{videoId}.{CurrentConfig.fileExtention}";

            Debug.WriteLine($"Downloading to {path} if not already downloaded!");

            ProcessStartInfo downloadPsi = new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = @$"{url} --no-playlist -x --audio-format {CurrentConfig.fileExtention} -o {path}",
                RedirectStandardOutput = false,
                UseShellExecute = false
            };
            var ytdl = Process.Start(downloadPsi);

            ytdl.WaitForExit();
        }

        

    }

    public enum SongType
    {
        Youtube,
        Soundcloud,
        Spotify,
        Local
    }
}
