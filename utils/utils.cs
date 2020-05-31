using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using DSharpPlus.VoiceNext;
using System.Runtime.InteropServices;
using System.Data;

namespace todor_reloaded
{
    public static class utils
    {
        public static async Task TransmitToDiscord(VoiceNextConnection discordConnection, Process transcoder)
        {
            VoiceTransmitStream discordStream = discordConnection.GetTransmitStream();

            await transcoder.StandardOutput.BaseStream.CopyToAsync(discordStream);
            Debug.WriteLine("CopyToAsync Done");

            await discordStream.FlushAsync();
            Debug.WriteLine("discordStream FlushAsync");

            await transcoder.StandardOutput.BaseStream.FlushAsync();
            Debug.WriteLine("audioStream FlushAsync");

            transcoder.Close();
            Debug.WriteLine("transcoder freed");


            await discordConnection.WaitForPlaybackFinishAsync();

        }
       
        public static String DownloadYTDL(string link)
        {
            String newName = Guid.NewGuid().ToString();

            var downloadPsi = new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = @$"{link} --no-playlist -x --audio-format opus -o {newName}.opus",
                RedirectStandardOutput = false,
                UseShellExecute = false
            };
            var ytdl = Process.Start(downloadPsi);

            ytdl.WaitForExit();

            return $"{newName}"; 
        }

        public static Process CreateFFMPEGProcess(string filename)
        {
            
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {filename}.opus -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var ffmpeg = Process.Start(psi);
            return ffmpeg;
        }

        public static string ArrayToString(string[] arr, char seperator)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string str in arr)
            {
                sb.Append(str + seperator);
            }

            return sb.ToString();

        }

        public static DiscordEmbedBuilder GetEmbedBuilderForSong(DiscordColor color, string title, string source)
        {
            DiscordEmbedBuilder EmbedBuilder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
            };

            EmbedBuilder.AddField("Song name:", title);
            EmbedBuilder.AddField("Song link:", source);

            return EmbedBuilder;
        }

        public static void LogMessage(string msg, BaseDiscordClient client, LogLevel logLevel = LogLevel.Info)
        {
            client.DebugLogger.LogMessage(logLevel, client.CurrentUser.Username, msg, DateTime.Now);
        }
    }
}
