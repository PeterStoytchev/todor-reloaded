using System;
using System.Collections.Generic;
using System.Text;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace todor_reloaded
{
    [Serializable]
    class NotificationGroup
    {
        private string m_Name { get; }
        private List<DiscordMember> m_Subscribers;
        private Dictionary<DateTime, Notification> m_Notifications;

        public NotificationGroup(string name)
        {
            this.m_Name = name;
        }

        public void AddNotification(CommandContext ctx, Notification notification, DateTime activationTime)
        {
            m_Notifications.Add(activationTime, notification);

            ctx.RespondAsync($"Added a notification with id {notification}");
        }

        public void AddSubscriber(DiscordMember subscriber) 
        { 
            m_Subscribers.Add(subscriber);  
        }

        public void RemoveSubscriber(DiscordMember subscriber)
        {
            m_Subscribers.Remove(subscriber);
        }

    }
}
