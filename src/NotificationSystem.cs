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
            'notificaionRescheduleSpan' INTEGER NOT NULL, 
            PRIMARY KEY('id' AUTOINCREMENT));";

            SqliteCommand command = new SqliteCommand(sqlstring, m_DbConnection);
            command.ExecuteNonQuery();
        }

        public Task<bool> CreateNotificationGroup(CommandContext ctx, string groupName)
        {
            string sqlstring = $"INSERT INTO groupStorage(groupName, users) VALUES('{groupName}', '');";

            SqliteCommand command = new SqliteCommand(sqlstring, m_DbConnection);
            
            try
            {
                command.ExecuteNonQuery();

                ctx.RespondAsync($"Created notificaitions channel {groupName}.");
                ctx.Client.Logger.Log(LogLevel.Information, $"User {ctx.Member.DisplayName} created message channel {groupName}!");

                return Task.FromResult(true);
            }
            catch (SqliteException e)
            {
                ctx.RespondAsync($"Notificaitions channel {groupName} already exists!");
                ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to create message channel {groupName}, but it already exists!");

                return Task.FromResult(false);
            }
        }

        public Task<bool> SubscribeUserToGroup(CommandContext ctx, string groupName)
        {
            string sqlstring = $"SELECT * FROM 'groupStorage' WHERE groupName='{groupName}' LIMIT 1;";

            SqliteCommand command = new SqliteCommand(sqlstring, m_DbConnection);
            SqliteDataReader rdr = command.ExecuteReader();

            while (rdr.Read())
            {
                int index = rdr.GetInt16(0);
                string userIds = rdr.GetString(2);
                List<string> split = new List<string>(userIds.Split(','));

                string userid = Convert.ToString(ctx.Member.Id);

                if (!split.Contains(userid))
                {
                    userIds = $"{userIds}{userid},";

                    string sqlupdate = $"UPDATE groupStorage SET users='{userIds}' WHERE id='{index}';";

                    SqliteCommand command2 = new SqliteCommand(sqlupdate, m_DbConnection);
                    command2.ExecuteNonQuery();


                    ctx.RespondAsync($"You have been added to notifications channel: {groupName}");
                    ctx.Client.Logger.Log(LogLevel.Information, $"User {ctx.Member.DisplayName} has been added to notificaitions channel {groupName}");

                    return Task.FromResult(true);
                }
                else
                {
                    ctx.RespondAsync($"You are already subscribed to notifications channel {groupName}");
                    ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to subscribe to notificaitions channel {groupName}, but was already present in it.");

                    return Task.FromResult(false);
                }
            }


            ctx.RespondAsync($"Couldn't find a notificaton channel with name {groupName}");
            ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to subscribe to notificaitions channel {groupName}, but such channel doesn't exist!");

            return Task.FromResult(false);
        }


        ~NotificationSystem()
        {
            m_DbConnection.Close();
        }

        /*
        

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

        
        */
    }
}
