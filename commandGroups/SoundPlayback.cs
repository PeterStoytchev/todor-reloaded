using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

namespace todor_reloaded
{

    public class SoundPlayback : BaseCommandModule
    {
        [Command("join"), Description("Joins the voice channel that you are in.")]
        public async Task Join(CommandContext ctx)
        {
            await global.player.JoinChannel(ctx);
        }

        [Command("leave"), Description("Leaves a voice channel.")]
        public async Task Leave(CommandContext ctx)
        {
            await global.player.LeaveChannel(ctx);
        }

        [Command("debugqueue")]
        [Description("a command that pritns the queeue to the visual studio debug console, do not use")]
        [RequireOwner]
        public async Task Queue(CommandContext ctx)
        {
            await global.player.QueueExecutor(ctx);

        }


        [Command("play"), Description("Plays a youtube video")]
        public async Task Play(CommandContext ctx, [Description("Youtube url")] string url)
        {

            Song s = new Song();
            s.type = SongType.Youtube;
            s.url = url;
            s.ctx = ctx;

            global.player.PlaySong(s);
        }

        [Command("skip"), Description("skips a song")]
        public async Task Skip(CommandContext ctx)
        {
            VoiceNextConnection connection = null;

            try
            {
                connection = global.player.GetVoiceConnection(ctx.Guild);
            }
            catch (Exception e)
            {
                await ctx.RespondAsync("Bot not in voice channel");
                return;
            }

            if (connection == null)
            {
                await ctx.RespondAsync("You aren't in the same channel as the bot");
                return;
            }

            connection.GetTransmitStream().Dispose();

            global.player.PlayNext(ctx);
        }
    }
}
