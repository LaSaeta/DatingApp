using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                         .Include(g => g.Connections)
                         .Where(g => g.Connections.Any(c => c.ConnectionId.Equals(connectionId)))
                         .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                         .Include(message => message.Sender)
                         .Include(message => message.Recipient)
                         .SingleOrDefaultAsync(message => message.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                         .Include(g => g.Connections)
                         .FirstOrDefaultAsync(g => g.Name.Equals(groupName));
        }

        public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(m => m.MessageSent)
                                         .AsQueryable()
                                         .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(message => message.RecipientUserName == messageParams.UserName && !message.RecipientDeleted),
                "Outbox" => query.Where(message => message.SenderUserName == messageParams.UserName && !message.SenderDeleted),
                _ => query.Where(message => message.RecipientUserName == messageParams.UserName && !message.RecipientDeleted && message.MessageRead == null),
            };

            return await PagedList<MessageDTO>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var messages = await _context.Messages
                           .Where(
                                m => (m.Recipient.UserName.Equals(currentUserName) && !m.RecipientDeleted && m.Sender.UserName.Equals(recipientUserName))
                                  || (m.Sender.UserName.Equals(currentUserName) && !m.SenderDeleted && m.Recipient.UserName.Equals(recipientUserName))
                           ).OrderBy(m => m.MessageSent)
                           .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider)
                           .ToListAsync();
                           
            var unreadMessages = messages.Where(m => m.MessageRead == null && m.RecipientUserName == currentUserName).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.MessageRead = DateTime.UtcNow;
                }
            }

            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}
