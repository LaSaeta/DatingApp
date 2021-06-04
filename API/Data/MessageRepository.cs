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

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                         .Include(message => message.Sender)
                         .Include(message => message.Recipient)
                         .SingleOrDefaultAsync(message => message.Id == id);
        }

        public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(message => message.Recipient.UserName == messageParams.UserName && !message.RecipientDeleted),
                "Outbox" => query.Where(message => message.Sender.UserName == messageParams.UserName && !message.SenderDeleted),
                _ => query.Where(message => message.Recipient.UserName == messageParams.UserName && !message.RecipientDeleted && message.MessageRead == null),
            };

            var messages = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDTO>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var messages = await _context.Messages
                           .Include(m => m.Sender).ThenInclude(u => u.Photos)
                           .Include(m => m.Recipient).ThenInclude(u => u.Photos)
                           .Where(
                                m => (m.Recipient.UserName.Equals(currentUserName) && !m.RecipientDeleted && m.Sender.UserName.Equals(recipientUserName))
                                  || (m.Sender.UserName.Equals(currentUserName) && !m.SenderDeleted && m.Recipient.UserName.Equals(recipientUserName))
                           ).OrderBy(m => m.MessageSent).ToListAsync();

            var unreadMessages = messages.Where(m => m.MessageRead == null && m.Recipient.UserName == currentUserName).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.MessageRead = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDTO>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
