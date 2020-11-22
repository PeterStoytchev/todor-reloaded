using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace todor_reloaded
{
    class NotificaitonCommands : BaseCommandModule
    {
        [Command("subscribe")]
        [Description("It subscribes you to a notifications channel.")]
        [Aliases("sub")]
        public async Task SubscribeExecutor(CommandContext ctx, string groupName)
        {
            await global.notificationSystem.SubscribeUserToGroup(ctx, groupName);
        }

        [Command("createchannel")]
        [Description("It creates a notifications channel.")]
        [Aliases("cc")]
        public async Task CreateChannelExecutor(CommandContext ctx, string groupName)
        {
            await global.notificationSystem.CreateNotificationGroup(ctx, groupName);
        }

        [Command("createNotification")]
        [Description("Creates and adds a notification to a notificaitons channel!")]
        [Aliases("cn")]
        public async Task CreateNotificaionExecutor(CommandContext ctx, string groupName, string name, string rescheduleTimespan, int hour, int minute, int day, int mounth, int year, params string[] message)
        {
            await global.notificationSystem.AddNotificationToGroup(ctx, groupName, name, utils.ArrayToString(message, ' '), rescheduleTimespan, hour, minute, day, mounth, year);
        }
    }
}
