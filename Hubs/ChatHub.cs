using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using JaezooServer.Models;
using System.Collections.Generic;

namespace JaezooServer.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();
        private static readonly List<User> Users = new();
        private static readonly List<Message> ChatHistory = new();

        public override async Task OnConnectedAsync()
        {
            var login = Context.GetHttpContext()?.Request.Query["login"].ToString();
            if (!string.IsNullOrEmpty(login))
            {
                OnlineUsers.TryAdd(Context.ConnectionId, login);
                await Clients.All.SendAsync("ReceiveOnlineUsers", OnlineUsers.Select(u => new { Username = u.Value, Status = "Online" }).ToList());
                await SendFriendsList(login);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            OnlineUsers.TryRemove(Context.ConnectionId, out _);
            await Clients.All.SendAsync("ReceiveOnlineUsers", OnlineUsers.Select(u => new { Username = u.Value, Status = "Online" }).ToList());
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendPrivateMessage(string fromUser, string toUser, string message)
        {
            var msg = new Message
            {
                FromUser = fromUser,
                ToUser = toUser,
                Content = message,
                Timestamp = DateTime.Now
            };
            ChatHistory.Add(msg);
            await Clients.All.SendAsync("ReceivePrivateMessage", fromUser, toUser, message);
        }

        public async Task SendFriendRequest(string fromUser, string toUser)
        {
            await Clients.All.SendAsync("ReceiveFriendRequest", fromUser, toUser);
            await SendFriendsList(fromUser);
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong");
        }

        public async Task GetOnlineUsers()
        {
            await Clients.Caller.SendAsync("ReceiveOnlineUsers", OnlineUsers.Select(u => new { Username = u.Value, Status = "Online" }).ToList());
        }

        public async Task GetChatHistory(string user1, string user2)
        {
            var messages = ChatHistory
                .Where(m => (m.FromUser == user1 && m.ToUser == user2) || (m.FromUser == user2 && m.ToUser == user1))
                .OrderBy(m => m.Timestamp)
                .Select(m => $"{m.FromUser}: {m.Content}")
                .ToList();
            await Clients.Caller.SendAsync("ReceiveChatHistory", messages);
        }

        private async Task SendFriendsList(string login)
        {
            var user = Users.FirstOrDefault(u => u.Login == login);
            if (user != null)
            {
                await Clients.Caller.SendAsync("ReceiveFriendsList", user.Friends);
            }
        }
    }
}