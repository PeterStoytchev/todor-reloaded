using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Diagnostics;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace todor_reloaded
{
    public class NotificationSystem
    {
        private List<NotificationGroup> m_NotificationGroups { get; set; }

        public List<NotificationGroup> GetGroups()
        {
            return m_NotificationGroups;
        }

        public Task<bool> SubscribeUserToGroup(CommandContext ctx, string groupName)
        {
            foreach (NotificationGroup group in m_NotificationGroups)
            {
                if (group.m_Name == groupName)
                {
                    group.AddSubscriber(ctx);
                    return Task.FromResult(true);
                }
            }

            ctx.RespondAsync($"Couldn't find a notificaton channel with name {groupName}");
            ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to subscribe to notificaitions channel {groupName}, but such channel doesn't exist!");


            return Task.FromResult(false);
        }

        public Task<bool> UnsubscribeSubscriberFromGroup(CommandContext ctx, string groupName)
        {
            ulong subscriberId = ctx.Member.Id;

            if (!m_Subscribers.Contains(subscriberId))
            {
                m_Subscribers.Add(subscriberId);

                ctx.RespondAsync($"You have subscibed to {m_Name}");
                ctx.Client.Logger.Log(LogLevel.Information, $"User {ctx.Member.DisplayName} subscribed to {m_Name}!");

                return Task.FromResult(true);
            }

            ctx.RespondAsync($"You are already subscribed to {m_Name}");
            ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to subscribe to {m_Name}!");

            return Task.FromResult(false);
        }

        public Task<bool> AddNotificationToGroup(CommandContext ctx, Notification notification, string groupName)
        {
            foreach (NotificationGroup group in m_NotificationGroups)
            {
                if (group.m_Name == groupName)
                {
                    group.AddNotification(ctx, notification);
                    return Task.FromResult(true);
                }
            }

            ctx.RespondAsync($"Couldn't find a notificaton channel with name {groupName}");
            ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to add a notification to notificaitions channel {groupName}, but such channel doesn't exist!");


            return Task.FromResult(false);
        }

        public Task<bool> CreateNotificationGroup(CommandContext ctx, NotificationGroup group)
        {
            if (!m_NotificationGroups.Contains(group))
            {

                m_NotificationGroups.Add(group);

                ctx.RespondAsync($"Created notificaitions channel {group.m_Name}.");
                ctx.Client.Logger.Log(LogLevel.Information, $"User {ctx.Member.DisplayName} created message channel {group.m_Name}!");
                
                return Task.FromResult(true);
            }

            ctx.RespondAsync($"Notificaitions channel {group.m_Name} already exists!");
            ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to create message channel {group.m_Name}, but it already exists!");

            return Task.FromResult(false);
        }
    }
}
