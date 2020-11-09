using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.EventArgs;

using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;

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
            utils.LogMessage($"Woops: {e.Exception.GetType()}: {e.Exception.Message}");
            utils.LogMessage($"ExceptionMessage: {e.Exception.Message}");

            return Task.CompletedTask;
        }


        public static Task Commands_CommandExecuted(CommandsNextExtension extension, CommandExecutionEventArgs e)
        {
            utils.LogMessage($"{e.Context.User.Username} executed command {e.Command.QualifiedName}");

            return Task.CompletedTask;
        }

        public static async Task Commands_CommandErrored(CommandsNextExtension extension, CommandErrorEventArgs e)
        {
            utils.LogMessage($"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}");

            await e.Context.RespondAsync(e.Exception.Message ?? "<no message>");

        }

    }
}
