// <copyright file="ChatHub.cs" company="Eppendorf AG - 2018">
// Copyright (c) Eppendorf AG - 2018. All rights reserved.
// </copyright>

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Eppendorf.VNCloud.StatusDataPushService.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public void SubscribeToUnitsGroup(string jwt)
        {
            string tenantId = ExtractTenantIdFromJwt(jwt);
            Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
        }

        public void UnsubscribeToUnitsGroup(string jwt)
        {
            string tenantId = ExtractTenantIdFromJwt(jwt);
            Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message + " (echo from server)");
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        private static string ExtractTenantIdFromJwt(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            string authHeader = jwt;
            authHeader = authHeader.Replace("Bearer ", string.Empty, StringComparison.InvariantCultureIgnoreCase);
            var jsonToken = handler.ReadToken(authHeader);
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            return tokenS.Claims.First(claim => claim.Type == "TenantId").Value;
        }
    }
}
