using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.EventArgs;

using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;

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

        public static async Task Bot_VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            try
            {
                if (e.After.Channel.Id == global.pcm.channel.Id)
                {
                    if (!global.pcm.JoinToChannel(e.User.Id))
                    {
                        DiscordMember m = await e.After.Guild.GetMemberAsync(e.User.Id);
                        await m.ModifyAsync(delegate (MemberEditModel kick)
                        {
                            kick.VoiceChannel = null;
                        });
                    }
                }

                if (e.Before.Channel.Id == global.pcm.channel.Id)
                {
                    global.pcm.LeaveFromChannel(e.User.Id);
                }
            }
            catch (NullReferenceException nre)
            {
                global.bot.Logger.LogDebug(nre.ToString());
            }
           
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

    }
}
