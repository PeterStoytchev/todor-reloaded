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
        public async Task SubscribeExecutor(CommandContext ctx, [Description("The name of the notification channel.")] string channelName)
        {
            await global.notificationSystem.SubscribeUserToGroup(ctx, channelName);
        }

        [Command("unsubscribe")]
        [Description("It unsubscribes you from a notifications channel.")]
        [Aliases("unsub")]
        public async Task UnsubscribeExecutor(CommandContext ctx, [Description("The name of the notification channel.")] string channelName)
        {
            await global.notificationSystem.UnsubscribeUserFromGroup(ctx, channelName);
        }

        [Command("createchannel")]
        [Description("It creates a notifications channel.")]
        [Aliases("cc")]
        public async Task CreateChannelExecutor(CommandContext ctx, [Description("The name of the notification channel.")] string channelName)
        {
            await global.notificationSystem.CreateNotificationGroup(ctx, channelName);
        }

        [Command("destroychannel")]
        [Description("It destroys a notifications channel.")]
        [Aliases("dc")]
        public async Task DestroyChannelExecutor(CommandContext ctx, [Description("The name of the notification channel.")] string channelName)
        {
            await global.notificationSystem.DestroyNotificationGroup(ctx, channelName);
        }

        [Command("createnotification")]
        [Description("Creates and adds a notification to a notificaitons channel!")]
        [Aliases("cn")]
        public async Task CreateNotificaionExecutor(CommandContext ctx, 
             [Description("The name of the notification channel.")] string channelName,
             [Description("The name of the notification.")] string name,
             [Description("When should the notification be sent first (format: hour/minute/day/month/year")] string timeString,
             [Description("After what period of time should the notification repeat itself (example: days:2). If you don't want it to repeat, type none.")] string rescheduleTimespan,
             [Description("What should the notification say.")] params string[] message) 
        {
            //format: hour/minute/day/month/year

            string[] components = timeString.Split('/');

            int hour = int.Parse(components[0]);
            int minute = int.Parse(components[1]);

            int day = int.Parse(components[2]);
            int mounth = int.Parse(components[3]);
            int year = int.Parse(components[4]);

            await global.notificationSystem.AddNotificationToGroup(ctx, channelName, name, utils.ArrayToString(message, ' '), rescheduleTimespan, hour, minute, day, mounth, year);
        }

        [Command("removenotification")]
        [Description("Creates and adds a notification to a notificaitons channel!")]
        [Aliases("rn")]
        public async Task CreateNotificaionExecutor(CommandContext ctx, [Description("The name of the notification.")] string name)
        {
            await global.notificationSystem.RemoveNotification(ctx, name);
        }

        [Command("listgroups")]
        [Description("Lists all notifiction channels.")]
        [Aliases("lg")]
        public async Task ListNotificationGroupsExecutor(CommandContext ctx)
        {
            await global.notificationSystem.ListGroups(ctx);
        }

        [Command("listnotifications")]
        [Description("Lists all notifiction in a channel.")]
        [Aliases("ln")]
        public async Task ListNotificationsExecutor(CommandContext ctx, [Description("The name of the notification channel.")] string channelName)
        {
            await global.notificationSystem.ListNotifications(ctx, channelName);
        }
    }
}
