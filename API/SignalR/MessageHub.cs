using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class MessageHub : Hub
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;

        public MessageHub(IMapper mapper, IUnitOfWork unitOfWork,
                           IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _presenceHub = presenceHub;
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUserName(), otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToMessageGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUserName(), otherUser);

            if (_unitOfWork.HasChanges())
                await _unitOfWork.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDTO createMessageDTO)
        {
            var userName = Context.User.GetUserName();

            if (userName.Equals(createMessageDTO.RecipientUserName.ToLower()))
                throw new HubException("You can't send a message to yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUserNameAsync(userName);
            var recipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUserName.ToLower());

            if (recipient == null)
                throw new HubException("User not found");

            var message = new Message
            {
                Content = createMessageDTO.Content,
                Recipient = recipient,
                RecipientUserName = recipient.UserName,
                Sender = sender,
                SenderUserName = sender.UserName
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(c => c.UserName.Equals(recipient.UserName)))
            {
                message.MessageRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new { userName = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDTO>(message));
            }
        }

        private async Task<Group> AddToMessageGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

            if (group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _unitOfWork.Complete())
                return group;

            throw new HubException("Failed to add to the message group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(c => c.ConnectionId.Equals(Context.ConnectionId));

            _unitOfWork.MessageRepository.RemoveConnection(connection);

            if (await _unitOfWork.Complete())
                return group;

            throw new HubException("Failed to remove from group");
        }

        private static string GetGroupName(string sender, string recipient)
        {
            bool stringCompare = string.CompareOrdinal(sender, recipient) < 0;

            return stringCompare ? string.Format("{0}-{1}", sender, recipient) : string.Format("{0}-{1}", recipient, sender);
        }
    }
}
