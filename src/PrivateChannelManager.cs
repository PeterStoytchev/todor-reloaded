using System;
using System.Collections.Generic;
using System.Text;

using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace todor_reloaded
{
    class PrivateChannelManager
    {
        private DiscordChannel m_channel = null;
        private DiscordMember m_currentChannelOwner = null;
    
        public PrivateChannelManager(DiscordChannel channel)
        {
            this.m_channel = channel;

            if (m_channel.Type != DSharpPlus.ChannelType.Voice)
                throw new ArgumentException("The private channel, must be of type voice!");


        }
    }
}
