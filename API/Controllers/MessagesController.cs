using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessages(CreateMessageDTO createMessageDTO)
        {
            var userName = User.GetUserName();

            if (userName.Equals(createMessageDTO.RecipientUserName.ToLower()))
                return BadRequest("You can't send a message to yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUserNameAsync(userName);
            var recipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUserName.ToLower());

            if (recipient == null)
                return NotFound();

            var message = new Message
            {
                Content = createMessageDTO.Content,
                Recipient = recipient,
                RecipientUserName = recipient.UserName,
                Sender = sender,
                SenderUserName = sender.UserName
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
                return Ok(_mapper.Map<MessageDTO>(message));

            return BadRequest("Failed to send the message.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUserName();

            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var userName = User.GetUserName();
            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if (message.Sender.UserName != userName && message.Recipient.UserName != userName)
                return Unauthorized();

            if (message.Sender.UserName.Equals(userName))
                message.SenderDeleted = true;

            if (message.Recipient.UserName.Equals(userName))
                message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _unitOfWork.MessageRepository.DeleteMessage(message);

            if (await _unitOfWork.Complete())
                return Ok();

            return BadRequest("Failure in deleting message");
        }
    }
}
