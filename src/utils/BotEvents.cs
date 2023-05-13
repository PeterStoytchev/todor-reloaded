using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.EventArgs;

using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using DSharpPlus.Lavalink;

namespace todor_reloaded
{
    public static class BotEvents
    {
        public static Task Bot_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation("Bot online!");

            return Task.CompletedTask;
        }

        public static Task Bot_ClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError($"Woops: {e.Exception.GetType()}: {e.Exception.Message}");
            sender.Logger.LogError($"ExceptionMessage: {e.Exception.Message}");

            return Task.CompletedTask;
        }

        public static Task Commands_CommandExecuted(CommandsNextExtension extension, CommandExecutionEventArgs e)
        {
            extension.Client.Logger.LogInformation($"{e.Context.User.Username} executed command {e.Command.QualifiedName}");
            
            return Task.CompletedTask;
        }

        public static async Task Commands_CommandErrored(CommandsNextExtension extension, CommandErrorEventArgs e)
        {
            extension.Client.Logger.LogWarning($"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}");

            await e.Context.RespondAsync(e.Exception.Message ?? "<no message>");
        }

        private static bool ShouldLeave(VoiceStateUpdateEventArgs args)
        {
            /* True If:
             * 1) The event was in a voice channel that the bot was playing in
             * 2) After the event, there was 1 person in the channel
             * 3) If that person is the bot
             */

            var lava = global.bot.GetLavalink();
            var node = lava.GetIdealNodeConnection();
            var conn = node.GetGuildConnection(args.Guild);

            if (conn is null)
                return false;

            bool sameChannel = false;
            if (args.Before is not null)
            {
                sameChannel = args.Before.Channel.Id == conn.Channel.Id;
            }
            else
            {
                sameChannel = args.After.Channel.Id == conn.Channel.Id;
            }

            return sameChannel && conn.Channel.Users.Count == 1 && conn.Channel.Users[0].IsCurrent;
        }

        public static async Task Bot_VoiceStateUpdated(DiscordClient sender, DSharpPlus.EventArgs.VoiceStateUpdateEventArgs args)
        {
            if (ShouldLeave(args))
            {
                global.bot.Logger.LogInformation("Leaving due to inactivity!");
                await global.lavaPlayer.LeaveDueToEmpty(args.Guild);
            }
        }
    }
}
