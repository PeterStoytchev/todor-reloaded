using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;


using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace todor_reloaded
{
    public class NotificationGroup
    {
        public string m_Name { get; set; }

        public ulong m_creatorId { get; set; }

        public List<ulong> m_Subscribers { get; set; }
        public List<Notification> m_Notifications { get; set; }

        public NotificationGroup(ulong creatorId, string name)
        {
            this.m_Name = name;
            this.m_creatorId = creatorId;
        }

        public void ProcessNotificaitons(DiscordClient client)
        {
            foreach (Notification notification in m_Notifications)
            {
                int cmp = notification.m_ActivationTime.CompareTo(DateTime.Now);
                
                if (cmp < 0 || cmp == 0) //check if we are after or at the notification time; should we send the notificaiton
                {
                    var enumerator = client.Guilds.Values.GetEnumerator();
                    DiscordGuild guild = enumerator.Current; //lets not support multiple guilds for now

                    foreach (ulong userId in m_Subscribers)
                    {
                        DiscordMember member;

                        if (guild.Members.TryGetValue(userId, out member)) //try and get the member
                        {
                            notification.Send(client, member); //send the notification
                        }
                        else
                        {
                            client.Logger.LogWarning($"Couldnt find user with id: {userId}");
                        }
                        
                    }

                    //reschedule the notificaiton and replace it in the list, kinds sus since we are still looping
                    Notification newNotification = notification.Reschedule();
                    m_Notifications.Remove(notification);
                    m_Notifications.Add(newNotification);
                }

            }
        }

        public Notification GetNotificaiton(string name)
        {
            foreach (Notification notification in m_Notifications)
            {
                if (notification.m_Name.ToLower() == name.ToLower())
                {
                    return notification;
                }
            }

            return null;
        }

        public Task<bool> AddNotification(CommandContext ctx, Notification notification)
        {
            if (!m_Notifications.Contains(notification))
            {
                m_Notifications.Add(notification);

                ctx.RespondAsync($"Added a notification called {notification.m_Name} to notification group {m_Name}");
                ctx.Client.Logger.Log(LogLevel.Information, $"User {ctx.Member.DisplayName} added a notification called {notification.m_Name} to notification group {m_Name}");

                return Task.FromResult(true);
            }

            ctx.RespondAsync($"Failed to add notification with name {notification.m_Name} to notification group {m_Name}, because said group, already has a notification with such a name!");
            ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to add notification with name {notification.m_Name} to channel {m_Name}, but such notificaiton already exists there!");

            return Task.FromResult(false);
        }

        public Task<bool> RemoveNotification(CommandContext ctx, string notificationName)
        {
            Notification notification = GetNotificaiton(notificationName);

            if (notification != null)
            {
                m_Notifications.Remove(notification);

                ctx.RespondAsync($"Removed notification {notificationName} from notification group {m_Name}");
                ctx.Client.Logger.Log(LogLevel.Information, $"User {ctx.Member.DisplayName} removed notification {notificationName} from notification group {m_Name}");

                return Task.FromResult(true);
            }
            else
            {
                ctx.RespondAsync($"Failed to find notification with name: {notificationName}!");
                ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to remove notification with name {notification.m_Name} from channel {m_Name}, but such notificaiton doesn't exist there!");

                return Task.FromResult(false);
            }
        }

        public Task<bool> AddSubscriber(CommandContext ctx) 
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


        //TODO: add messeging and logging
        public Task<bool> RemoveSubscriber(DiscordMember subscriber)
        {
            if (m_Subscribers.Contains(subscriber.Id))
            {
                m_Subscribers.Remove(subscriber.Id);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        //TODO: add messeging and logging
        public Task<bool> isUserASubscriber(ulong userId)
        {
            if (m_Subscribers.Contains(userId))
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

    }
}
