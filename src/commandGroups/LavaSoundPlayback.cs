using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;

namespace todor_reloaded
{
    class LavaSoundPlayback : BaseCommandModule
    {
        [Command("join")]
        [Description("Joins the voice channel that you are in.")]
        [Aliases("joint", "j")]
        public async Task Join(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            
            if (lava.ConnectedNodes.Count == 0)
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.GetIdealNodeConnection();

            if (ctx.Channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            await node.ConnectAsync(ctx.Channel);
            await ctx.RespondAsync($"Joined {ctx.Channel.Name}!");
        }

        [Command("leave")]
        [Description("Leaves a voice channel.")]
        [Aliases("l", "aufwiedersehen")]
        public async Task Leave(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            if (lava.ConnectedNodes.Count == 0)
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.GetIdealNodeConnection();

            var conn = node.GetGuildConnection(ctx.Channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Left {ctx.Channel.Name}!");
        }

    }
}
