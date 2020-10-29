using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.EventArgs;

using DSharpPlus.CommandsNext;

namespace todor_reloaded
{
    public static class BotEvents
    {
        public static Task Bot_Ready(ReadyEventArgs e)
        {
            utils.LogMessage("Bot online!", e.Client);

            return Task.CompletedTask;
        }

        public static Task Bot_ClientErrored(ClientErrorEventArgs e)
        {
            utils.LogMessage($"Woops: {e.Exception.GetType()}: {e.Exception.Message}", e.Client, LogLevel.Error);
            utils.LogMessage($"ExceptionMessage: {e.Exception.Message}", e.Client, LogLevel.Error);

            return Task.CompletedTask;
        }


        public static Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            utils.LogMessage($"{e.Context.User.Username} executed command {e.Command.QualifiedName}", e.Context.Client);

            return Task.CompletedTask;
        }

        public static async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            utils.LogMessage($"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", e.Context.Client);

            await e.Context.RespondAsync(e.Exception.Message ?? "<no message>");

        }

    }
}
