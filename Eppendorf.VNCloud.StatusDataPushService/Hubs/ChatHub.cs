// <copyright file="ChatHub.cs" company="Eppendorf AG - 2018">
// Copyright (c) Eppendorf AG - 2018. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.SignalR;

namespace Eppendorf.VNCloud.StatusDataPushService.Hubs
{
    public class ChatHub : Hub
    {
        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message + " (echo from server)");
        }
    }
}
