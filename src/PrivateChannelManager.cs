using System;
using System.Collections.Generic;
using System.Text;

using DSharpPlus.Net;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus.Net.Models;

namespace todor_reloaded
{
    public class PrivateChannelManager
    {
        public DiscordChannel channel { get; private set; }
        private ulong m_currentChannelOwner = 0;
        private List<ulong> m_whitelist = new List<ulong>();

        public PrivateChannelManager(DiscordChannel channel)
        {
            this.channel = channel;

            if (this.channel.Type != DSharpPlus.ChannelType.Voice)
                throw new ArgumentException("The private channel, must be of type voice!");
        }

        public List<ulong> GetChannelMembers()
        {
            List<ulong> members = new List<ulong>();
            foreach (var member in channel.Users)
                members.Add(member.Id);

            return members;
        }

        public bool JoinToChannel(ulong member)
        {
            List<ulong> members = GetChannelMembers();
            if (members.Count == 0)
            {
                m_currentChannelOwner = member;
                return true;
            }

            if (m_whitelist.Contains(member))
                return true;

            return false;
        }

        public async void LeaveFromChannel(ulong member)
        {
            if (member == m_currentChannelOwner)
            {
                List<ulong> members = GetChannelMembers();
                foreach (ulong mid in members)
                {
                    DiscordMember m = await channel.Guild.GetMemberAsync(mid);
                    await m.ModifyAsync(delegate (MemberEditModel kick)
                    {
                        kick.VoiceChannel = null;
                    });
                }
                m_currentChannelOwner = 0;
            }
        }

        public bool AllowMember(ulong sender, ulong member)
        {
            if (sender != m_currentChannelOwner)
                return false;

            if (!m_whitelist.Contains(member) && sender != member)
                m_whitelist.Add(member);
            
            return true;
        }
    }
}
