using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Diagnostics;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;

using Microsoft.Extensions.Logging;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace todor_reloaded
{
    public class NotificationSystem
    {
        private SqliteConnection m_DbConnection;

        public NotificationSystem(string dbPath)
        {
            m_DbConnection = new SqliteConnection($"Data Source=file:{dbPath}");
            m_DbConnection.Open();

            //this will create the two tables that the system needs in the db, if they dont already exist
            string sqlstring =
            @"
            CREATE TABLE IF NOT EXISTS 'groupStorage' (
	        'id' INTEGER NOT NULL UNIQUE,
            'groupName' TEXT NOT NULL UNIQUE,
            'users' TEXT NOT NULL,
	        PRIMARY KEY('id'));

            CREATE TABLE IF NOT EXISTS 'data'
            ('id'	INTEGER NOT NULL, 'notificationName'  TEXT NOT NULL UNIQUE, 
            'notificationMessage' TEXT NOT NULL,
            'notificationGroupId' INTEGER NOT NULL, 
            'notificationActivationDate' INTEGER NOT NULL, 
            'notificaionRescheduleSpan' INTEGER, 
            PRIMARY KEY('id' AUTOINCREMENT));";

            SqliteCommand command = new SqliteCommand(sqlstring, m_DbConnection);
            command.ExecuteScalar();
        }

        ~NotificationSystem()
        {
            m_DbConnection.Close();
        }

        /*
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
        */
    }
}
