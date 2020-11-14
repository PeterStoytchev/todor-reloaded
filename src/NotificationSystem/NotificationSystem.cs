using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        private string dataPath;

        public NotificationSystem(string dataPath)
        {
            this.dataPath = dataPath;
            if (File.Exists(dataPath))
            {
                string jsonSource = File.ReadAllText(dataPath);
                m_NotificationGroups = JsonSerializer.Deserialize<NotificationSystem>(jsonSource).m_NotificationGroups;
            }
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

        public async Task ProcessEvents()
        {
            //find some way to stop this greacefully
            while (true)
            {
                //could run the loop bellow in parralel wit tasks but given the scale at which the bot runs, it will not be necessary, at least for now
                foreach (NotificationGroup group in m_NotificationGroups)
                {
                    group.ProcessNotificaitons(global.bot);
                }

                await Task.Delay(1000); //wait for a second, before processing again
            }
        }

        ~NotificationSystem()
        {
            string output = JsonSerializer.Serialize(this);

            File.WriteAllText(dataPath, output);
        }
    }
}
