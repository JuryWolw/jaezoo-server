using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace JaezooServer.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var login = Context.GetHttpContext()?.Request.Query["login"].ToString();
            if (!string.IsNullOrEmpty(login))
            {
                OnlineUsers.TryAdd(Context.ConnectionId, login);
                await Clients.All.SendAsync("UpdateOnlineUsers", OnlineUsers.Values.ToList());
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            OnlineUsers.TryRemove(Context.ConnectionId, out _);
            await Clients.All.SendAsync("UpdateOnlineUsers", OnlineUsers.Values.ToList());
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendPrivateMessage(string fromUser, string toUser, string message)
        {
            await Clients.All.SendAsync("ReceivePrivateMessage", fromUser, toUser, message);
        }

        public async Task SendFriendRequest(string fromUser, string toUser)
        {
            await Clients.All.SendAsync("ReceiveFriendRequest", fromUser, toUser);
        }
    }
}