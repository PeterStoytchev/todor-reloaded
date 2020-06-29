using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Google.Apis.Compute.v1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace todor_reloaded
{
    [Description("A series of commands for interacting with the Ark server hosted on GCP")]
    public class GcpCommands : BaseCommandModule
    {
        private DateTime lastActionTimeStamp = DateTime.Now;
        private bool firstTime = true;

        private async Task<bool> CheckState(CommandContext ctx, string zone, string instanceId)
        {
            DateTime expectedTime;
            if (!firstTime)
            {
                expectedTime = lastActionTimeStamp.AddMinutes(5);
            }
            else
            {
                expectedTime = lastActionTimeStamp;
                firstTime = false;
            }

            Instance instance = await global.googleClient.GetInstance(zone, instanceId);

            if (expectedTime <= DateTime.Now && instance.Status != "STOPPING")
            {
                lastActionTimeStamp = DateTime.Now;
                return true;
            }
            else
            {
                await ctx.RespondAsync($"Not enough time has passed since last action or the server is already starting! Please wait!");
                return false;
            }

        }

        [Command("start")]
        [Description("A command that starts the ARK server!")]
        public async Task StartInstance(CommandContext ctx, [Optional, DefaultParameterValue("europe-north1-a")] string zone, [Optional, DefaultParameterValue("7151927119081642430")] string instanceId)
        {
            if (await CheckState(ctx, zone, instanceId))
            {
                Operation op = await global.googleClient.StartInstance(zone, instanceId);

                await ctx.RespondAsync("Starting ark server, please wait for at least 5 minutes for the server to load!");
            }
        }

        [Command("stop")]
        [Description("A command that stops the ARK server!")]
        public async Task StopInstance(CommandContext ctx, [Optional, DefaultParameterValue("europe-north1-a")] string zone, [Optional, DefaultParameterValue("7151927119081642430")] string instanceId)
        {
            if (await CheckState(ctx, zone, instanceId))
            {
                Operation op = await global.googleClient.StopInstance(zone, instanceId);

                await ctx.RespondAsync("Shutting down ARK server!");
            }
        }

        [Command("state")]
        [Description("A command that prints the state of the server hosting the ARK server!")]
        public async Task GetInstanceState(CommandContext ctx, [Optional, DefaultParameterValue("europe-north1-a")] string zone, [Optional, DefaultParameterValue("7151927119081642430")] string instanceId)
        {
            Instance instance = await global.googleClient.GetInstance(zone, instanceId);

            await ctx.RespondAsync($"The server is {instance.Status.ToLower()}");
        }
    }
}
