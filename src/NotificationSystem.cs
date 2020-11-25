using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Diagnostics;

using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Timers;

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
        private System.Timers.Timer m_ProcessingTimer;
        private BackgroundWorker m_ProcessingWorker;

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
            ('id'	INTEGER NOT NULL, 
            'notificationName'  TEXT NOT NULL UNIQUE, 
            'notificationMessage' TEXT NOT NULL,
            'notificationGroupId' INTEGER NOT NULL, 
            'notificationActivationDate' INTEGER NOT NULL, 
            'notificaionRescheduleSpan' INTEGER NOT NULL, 
            PRIMARY KEY('id' AUTOINCREMENT));";

            SqliteCommand cmd = new SqliteCommand(sqlstring, m_DbConnection);
            cmd.ExecuteNonQuery();

            //initialize the worker that periodically runs through the db and sends notifications
            m_ProcessingWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            m_ProcessingWorker.DoWork += ProcessNotifications;

            m_ProcessingTimer = new System.Timers.Timer(1000 * 10); //TODO: expose the refersh time in the config
            m_ProcessingTimer.Elapsed += M_ProcessingTimer_Elapsed;
            m_ProcessingTimer.Start();

        }

        private void M_ProcessingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_ProcessingWorker.RunWorkerAsync();
        }

        public void ProcessNotifications(object sender, DoWorkEventArgs e)
        {
            utils.DebugLog($"[{DateTime.Now}] Starting notificaitons processing...");

            SqliteCommand command = new SqliteCommand("SELECT * FROM data;", m_DbConnection);
            SqliteDataReader rdr = command.ExecuteReader();

            while (rdr.Read())
            {
                short notificationId = rdr.GetInt16(0);
                string notificationMsg = rdr.GetString(2);
                short groupId = rdr.GetInt16(3);

                DateTime triggerTime = new DateTime(rdr.GetInt64(4));
                TimeSpan rescheduleTimeSpan = new TimeSpan(rdr.GetInt64(5));

                int cmp = DateTime.Compare(triggerTime, DateTime.Now);
                if (cmp < 0 || cmp == 0) //check if the time for the notification has passed or is now
                {
                    //get the users from the db
                    SqliteCommand command2 = new SqliteCommand($"SELECT users FROM groupStorage WHERE id='{groupId}' LIMIT 1;", m_DbConnection);
                    SqliteDataReader rdr2 = command2.ExecuteReader();

                    while (rdr2.Read())
                    {
                        string usersStr = rdr2.GetString(0); //get the comma seperated string that contains the users
                        string[] users = usersStr.Substring(0, usersStr.Length - 1).Split(','); //remove the last char and split the string by comma

                        DiscordGuild guild = global.bot.GetGuildAsync(global.botConfig.discordGuildId).GetAwaiter().GetResult();

                        //get the member from discord by the id and send him the message
                        foreach (string str in users)
                        {
                            ulong id = Convert.ToUInt64(str);
                            DiscordMember member = guild.GetMemberAsync(id).GetAwaiter().GetResult();

                            utils.DebugLog($"[{DateTime.Now}] Sending notification scheduled for {triggerTime.ToString()} to user {member.Username}");

                            member.SendMessageAsync(notificationMsg).GetAwaiter().GetResult();
                        }
                    }

                    DateTime newTime = DateTime.Now.Add(rescheduleTimeSpan);

                    //update the activation time, based on the reshedule timespan
                    SqliteCommand command3 = new SqliteCommand($"UPDATE data SET notificationActivationDate='{newTime.Ticks}' WHERE id='{notificationId}';", m_DbConnection);
                    command3.ExecuteNonQuery();

                    utils.DebugLog($"Notification's trigger time updated to {newTime}");
                }
            }
        }

        public Task<bool> CreateNotificationGroup(CommandContext ctx, string groupName)
        {
            SqliteCommand command = new SqliteCommand($"INSERT INTO groupStorage(groupName, users) VALUES('{groupName}', '');", m_DbConnection);

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
      
        public Task<bool> DestroyNotificationGroup(CommandContext ctx, string groupName)
        {
            SqliteCommand getDetailsCmd = new SqliteCommand($"SELECT id FROM groupStorage WHERE groupName='{groupName}' LIMIT 1;", m_DbConnection);
            SqliteDataReader detailsReader = getDetailsCmd.ExecuteReader();
            
            while (detailsReader.Read())
            {
                short groupId = detailsReader.GetInt16(0);

                SqliteCommand removeTable = new SqliteCommand($"DELETE FROM groupStorage WHERE id={groupId} LIMIT 1;", m_DbConnection);
                removeTable.ExecuteNonQuery();

                SqliteCommand removeNotifications = new SqliteCommand($"DELETE FROM data WHERE notificationGroupId={groupId};", m_DbConnection);
                removeNotifications.ExecuteNonQuery();

                ctx.RespondAsync($"Notificaitions channel {groupName} removed!");
                ctx.Client.Logger.Log(LogLevel.Information, $"User {ctx.Member.DisplayName} removed message channel {groupName}!");

                return Task.FromResult(true);
            }

            ctx.RespondAsync($"Notificaitions channel {groupName} doesn't exist!");
            ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to remove message channel {groupName}, but it doesn't exist!");

            return Task.FromResult(false);
        }

        public Task<bool> ListGroups(CommandContext ctx)
        {
            string sql = $"SELECT groupName FROM groupStorage;";

            SqliteCommand command = new SqliteCommand(sql, m_DbConnection);
            SqliteDataReader rdr = command.ExecuteReader();

            DiscordEmbedBuilder EmbedBuilder = new DiscordEmbedBuilder
            {
                Title = "Notification Groups",
                Color = DiscordColor.Azure,
            };

            bool hasRead;
            while (hasRead = rdr.Read())
            {
                string name = rdr.GetString(0);

                try
                {
                    EmbedBuilder.AddField("Group name:", name, true);
                }
                catch (Exception e)
                {
                    ctx.Channel.SendMessageAsync(embed: EmbedBuilder.Build());

                    EmbedBuilder.ClearFields();
                    return Task.FromResult(true);
                }

            }

            if (EmbedBuilder.Fields.Count != 0)
            {
                ctx.Channel.SendMessageAsync(embed: EmbedBuilder.Build());
            }

            if (!hasRead)
            {
                ctx.Channel.SendMessageAsync("There are no notification channels!");
                return Task.FromResult(false);
            }
            else
            {
                return Task.FromResult(true);
            }

        }

        public Task<bool> SubscribeUserToGroup(CommandContext ctx, string groupName)
        {
            SqliteCommand command = new SqliteCommand($"SELECT id,users FROM 'groupStorage' WHERE groupName='{groupName}' LIMIT 1;", m_DbConnection);
            SqliteDataReader rdr = command.ExecuteReader();

            while (rdr.Read())
            {
                int index = rdr.GetInt16(0);
                string userIds = rdr.GetString(1);
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

        public Task<bool> UnsubscribeUserFromGroup(CommandContext ctx, string groupName)
        {
            SqliteCommand command = new SqliteCommand($"SELECT id,users FROM 'groupStorage' WHERE groupName='{groupName}' LIMIT 1;", m_DbConnection);
            SqliteDataReader rdr = command.ExecuteReader();

            while (rdr.Read())
            {
                int index = rdr.GetInt16(0);
                string userIds = rdr.GetString(1);
                List<string> split = new List<string>(userIds.Split(','));

                string userid = Convert.ToString(ctx.Member.Id);

                if (split.Contains(userid))
                {
                    split.Remove(userid);

                    string newIds = utils.ArrayToString(split.ToArray(), ',');
                    string sqlupdate = $"UPDATE groupStorage SET users='{newIds}' WHERE id='{index}';";

                    SqliteCommand command2 = new SqliteCommand(sqlupdate, m_DbConnection);
                    command2.ExecuteNonQuery();

                    ctx.RespondAsync($"You have been removed from notifications channel: {groupName}");
                    ctx.Client.Logger.Log(LogLevel.Information, $"User {ctx.Member.DisplayName} has been removed from notificaitions channel {groupName}");

                    return Task.FromResult(true);
                }
                else
                {
                    ctx.RespondAsync($"You are aren't subscribed to notifications channel {groupName}");
                    ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to subscribe to notificaitions channel {groupName}, but wasn't in it.");

                    return Task.FromResult(false);
                }
            }

            ctx.RespondAsync($"Couldn't find a notificaton channel with name {groupName}");
            ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to subscribe to notificaitions channel {groupName}, but such channel doesn't exist!");

            return Task.FromResult(false);
        }

        public Task<bool> AddNotificationToGroup(CommandContext ctx, string groupName, string name, string message, string timespan, int hour, int minute, int day, int mounth, int year)
        {
            string checkValidGroupNameSQL = $"SELECT id FROM 'groupStorage' WHERE groupName='{groupName}' LIMIT 1;";

            SqliteCommand command = new SqliteCommand(checkValidGroupNameSQL, m_DbConnection);
            SqliteDataReader rdr = command.ExecuteReader();

            
            while (rdr.Read())
            {
                int channelId = rdr.GetInt16(0);

                DateTime dt = new DateTime(year, mounth, day, hour, minute, 0, 0);
                TimeSpan ts = utils.CreateTimeSpan(timespan);

                string insertSQL = $"INSERT INTO data(notificationName, notificationMessage, notificationGroupId, notificationActivationDate, notificaionRescheduleSpan) VALUES('{name}', '{message}', '{channelId}', '{dt.Ticks}', '{ts.Ticks}');";

                SqliteCommand insertCommand = new SqliteCommand(insertSQL, m_DbConnection);
                
                try
                {
                    insertCommand.ExecuteNonQuery();

                    ctx.RespondAsync($"Notificaion with name {name} has been added to channel {groupName}!");
                    ctx.Client.Logger.Log(LogLevel.Information, $"User {ctx.Member.DisplayName} added a notification with name {name} to notificaitions channel {groupName}!");
    
                    return Task.FromResult(true);

                }
                catch (Exception e)
                {
                    ctx.RespondAsync($"Notificaion with name {name} already exists!");
                    ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to add a notification to notificaitions channel {groupName}, but such notificiaon already exists!");

                    return Task.FromResult(false);
                }

            }

            ctx.RespondAsync($"Couldn't find a notificaton channel with name {groupName}");
            ctx.Client.Logger.Log(LogLevel.Warning, $"User {ctx.Member.DisplayName} tried to add a notification to notificaitions channel {groupName}, but such channel doesn't exist!");

            return Task.FromResult(false);
        }
        
        public Task<bool> RemoveNotification(CommandContext ctx, string notificationName)
        {
            SqliteCommand command = new SqliteCommand($"DELETE FROM data WHERE notificationName='{notificationName}';", m_DbConnection);
            command.ExecuteNonQuery();
            
            ctx.RespondAsync($"Notificaion with name {notificationName} has been removed!");
            ctx.Client.Logger.Log(LogLevel.Information, $"User {ctx.Member.DisplayName} added a notification with name {notificationName} has been removed!");

            return Task.FromResult(true);

        }
        
        public Task<bool> ListNotifications(CommandContext ctx, string groupName)
        {
            string sql = $"SELECT id FROM groupStorage WHERE groupName='{groupName}' LIMIT 1;";
            SqliteCommand command = new SqliteCommand(sql, m_DbConnection);
            SqliteDataReader rdr = command.ExecuteReader();
            
            rdr.Read();
            short groupId = rdr.GetInt16(0);

            string sql2 = $"SELECT notificationName,notificationMessage,notificationActivationDate,notificaionRescheduleSpan FROM data WHERE notificationGroupId='{groupId}';";
            SqliteCommand command2 = new SqliteCommand(sql2, m_DbConnection);
            SqliteDataReader rdr2 = command2.ExecuteReader();

            DiscordEmbedBuilder EmbedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Notifications for group: {groupName}",
                Color = DiscordColor.Cyan,
            };

            while (rdr2.Read())
            {
                string notName = rdr2.GetString(0);
                string notMessage = rdr2.GetString(1);
                
                DateTime nextDate = new DateTime(rdr2.GetInt64(2));
                TimeSpan repetitionPattern = new TimeSpan(rdr2.GetInt64(3));

                EmbedBuilder.AddField("Name:", notName, true);
                EmbedBuilder.AddField("Message:", notMessage, true);
                EmbedBuilder.AddField("Next date:", nextDate.ToString(), true);
                EmbedBuilder.AddField("Repetition pattern:", repetitionPattern.ToString(), true);

                ctx.Channel.SendMessageAsync(embed: EmbedBuilder.Build());

                EmbedBuilder.ClearFields();

                return Task.FromResult(true);
            }

            ctx.Channel.SendMessageAsync($"There are no notifications for group {groupName}");
            return Task.FromResult(false);
        }

        ~NotificationSystem()
        {
            m_DbConnection.Close();
        }
    }
}