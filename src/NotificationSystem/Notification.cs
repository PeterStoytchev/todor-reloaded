using System;
using System.Collections.Generic;
using System.Text;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace todor_reloaded
{
    [Serializable]
    class Notification
    {
        private string m_Message;
        private DateTime m_ActivationTime { get; }
        private SchedulingType m_SchedulingMethod { get; }

        private Guid uniqueId { public get; }
        
        public Notification(string message, DateTime triggerTime, SchedulingType scheduling)
        {
            this.m_Message = message;
            this.m_ActivationTime = triggerTime;
            this.m_SchedulingMethod = scheduling;

            uniqueId = Guid.NewGuid();
        }

        public async void Send(DiscordClient client, DiscordMember subscriber)
        {
            await subscriber.SendMessageAsync(m_Message);
        }
    }
}
