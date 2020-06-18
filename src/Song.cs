using DSharpPlus.CommandsNext;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace todor_reloaded
{
    [Serializable]
    public class Song
    { 
        public SongType type { get; private set; }

        public string url { get; private set; }
        public string path { get; private set; }

        public string name { get; private set; }
        public string uploader { get; private set; }
        public string videoId { get; private set; }
        public string publishedAt { get; private set; }

        public string thumbnail { get; private set; }

        public Song(PlaylistItem item)
        {
            this.name = item.Snippet.Title;
            this.uploader = item.Snippet.ChannelTitle;
            this.publishedAt = item.Snippet.PublishedAt;
            this.thumbnail = GetValidThunbnail(item.Snippet.Thumbnails);
            this.type = SongType.Youtube;
            this.videoId = item.Snippet.ResourceId.VideoId;
            this.url = $"https://youtu.be/{this.videoId}";
        }

        public Song(string query, SongType type)
        {
            if (query.StartsWith("https://"))
            {
                Video result = global.youtubeClient.GetVideoDetails(query).GetAwaiter().GetResult();
                VideoSnippet snippet = result.Snippet;

                this.name = snippet.Title;
                this.uploader = snippet.ChannelTitle;
                this.publishedAt = snippet.PublishedAt;
                this.thumbnail = GetValidThunbnail(snippet.Thumbnails);
                this.type = type;
                this.videoId = result.Id;
                this.url = $"https://youtu.be/{this.videoId}";
            }
            else
            {
                SearchResult result = global.youtubeClient.SearchForVideo(query).GetAwaiter().GetResult();
                SearchResultSnippet snippet = result.Snippet;

                this.name = snippet.Title;
                this.uploader = snippet.ChannelTitle;
                this.publishedAt = snippet.PublishedAt;
                this.thumbnail = GetValidThunbnail(snippet.Thumbnails);
                this.type = type;
                this.videoId = result.Id.VideoId;
                this.url = $"https://youtu.be/{this.videoId}";
            }

        }

        private string GetValidThunbnail(ThumbnailDetails thumbnails)
        {
            if (thumbnails.Standard != null)
            {
                return thumbnails.Standard.Url;
            }
            else if (thumbnails.Medium != null)
            {
                return thumbnails.Medium.Url;
            }
            else if (thumbnails.High != null)
            {
                return thumbnails.High.Url;
            }
            else if (thumbnails.Maxres != null)
            {
                return thumbnails.Maxres.Url;
            }

            return null;
        }

        public void PrintDebug()
        {
            string[] props = GetDebug();

            foreach (string str in props)
            {
                Debug.WriteLine(str);
            }
        }

        public string[] GetDebug()
        {
            string[] props = new string[8];

            props[0] = "================";

            props[1] = this.name;
            props[2] = this.uploader;
            props[3] = this.publishedAt.ToString();
            props[4] = this.url;
            props[5] = this.type.ToString();
            props[6] = this.videoId;

            props[7] = "================";

            return props;
        }

        public void DownloadYTDL(CommandContext ctx, bool shouldPrint)
        {
            if (shouldPrint)
            {
                ctx.RespondAsync($"Loading {name}");
            }

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
