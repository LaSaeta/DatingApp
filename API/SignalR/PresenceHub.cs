using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.GetUserName();
            var isOnline = await _tracker.UserConnected(userName, Context.ConnectionId);

            if (isOnline)
                await Clients.Others.SendAsync("UserIsOnline", userName);

            string[] onlineUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", onlineUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.GetUserName();
            var isOffline = await _tracker.UserDisconnected(userName, Context.ConnectionId);

            if (isOffline)
                await Clients.Others.SendAsync("UserIsOffline", userName);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
