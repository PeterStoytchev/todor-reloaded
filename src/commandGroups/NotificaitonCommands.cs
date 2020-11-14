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

        [Command("addNotification")]
        [Description("It adds a notification to a notifications channel.")]
        [Aliases("addNot")]
        public async Task AddNotificationExecutor(CommandContext ctx, string notificationName, string notificationMesasge, DateTime triggerTime, string pattern, string groupName)
        {
            Notification notification = new Notification(notificationName, notificationMesasge, triggerTime, utils.CreateTimeSpan(pattern));

            await global.notificationSystem.AddNotificationToGroup(ctx, notification, groupName);
        }


        [Command("createNotificationChannel")]
        [Description("It creates a notification channel.")]
        [Aliases("cnc")]
        public async Task CreateNotifciationChannelExecutor(CommandContext ctx, string channelName)
        {
            NotificationGroup notificationGroup = new NotificationGroup(ctx.Member.Id, channelName);

            await global.notificationSystem.CreateNotificationGroup(ctx, notificationGroup);
        }
    }
}
