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

        [Command("unsubscribe")]
        [Description("It unsubscribes you from a notifications channel.")]
        [Aliases("unsub")]
        public async Task UnsubscribeExecutor(CommandContext ctx, string groupName)
        {
            await global.notificationSystem.UnsubscribeUserFromGroup(ctx, groupName);
        }

        [Command("createchannel")]
        [Description("It creates a notifications channel.")]
        [Aliases("cc")]
        public async Task CreateChannelExecutor(CommandContext ctx, string groupName)
        {
            await global.notificationSystem.CreateNotificationGroup(ctx, groupName);
        }

        [Command("destroychannel")]
        [Description("It destroys a notifications channel.")]
        [Aliases("dc")]
        public async Task DestroyChannelExecutor(CommandContext ctx, string groupName)
        {
            await global.notificationSystem.DestroyNotificationGroup(ctx, groupName);
        }

        [Command("createnotification")]
        [Description("Creates and adds a notification to a notificaitons channel!")]
        [Aliases("cn")]
        public async Task CreateNotificaionExecutor(CommandContext ctx, string groupName, string name, string timeString, string rescheduleTimespan, params string[] message) 
        {
            //format: hour/minute/day/month/year

            string[] components = timeString.Split('/');

            int hour = int.Parse(components[0]);
            int minute = int.Parse(components[1]);

            int day = int.Parse(components[2]);
            int mounth = int.Parse(components[3]);
            int year = int.Parse(components[4]);

            await global.notificationSystem.AddNotificationToGroup(ctx, groupName, name, utils.ArrayToString(message, ' '), rescheduleTimespan, hour, minute, day, mounth, year);
        }

        [Command("removenotification")]
        [Description("Creates and adds a notification to a notificaitons channel!")]
        [Aliases("rn")]
        public async Task CreateNotificaionExecutor(CommandContext ctx, string name)
        {
            await global.notificationSystem.RemoveNotification(ctx, name);
        }
    }
}
