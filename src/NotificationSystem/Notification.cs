using System;
using System.Collections.Generic;
using System.Text;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace todor_reloaded
{
    public class Notification
    {
        public string m_Message { get; set; }
        public string m_Name { get; set; }
        public DateTime m_ActivationTime { get; set; }
        public TimeSpan m_RescheduleTimespan { get; set; }

        public Notification(string name, string message, DateTime triggerTime, TimeSpan rescheduleTimespan)
        {
            this.m_Name = name;
            this.m_Message = message;
            this.m_ActivationTime = triggerTime;
        }

        public async void Send(DiscordClient client, DiscordMember subscriber)
        {
            await subscriber.SendMessageAsync(m_Message);
        }

        public Notification Reschedule()
        {
            this.m_ActivationTime = m_ActivationTime.Add(m_RescheduleTimespan);
            return this;
        }

    }
}
