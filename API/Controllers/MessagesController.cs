using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using Microsoft.AspNetCore.Mvc;
using API.Extensions;
using API.Entities;
using AutoMapper;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public MessagesController(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if(username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You can not send message to yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if(recipient == null) return NotFound();
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };
            _unitOfWork.MessageRepository.AddMessage(message);

            if(await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,
                messages.TotalCount,messages.TotalPages);
            return messages;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var currentUsername = User.GetUsername();
            var message = await _unitOfWork.MessageRepository.GetMessage(id);
            if(message.Sender.UserName != currentUsername && message.Recipient.UserName != currentUsername) 
                return Unauthorized();
            if(message.Sender.UserName == currentUsername) message.SenderDeleted = true;
             if(message.Recipient.UserName == currentUsername) message.RecipientDeleted = true;

             if(message.SenderDeleted && message.RecipientDeleted) 
                _unitOfWork.MessageRepository.DeleteMessage(message);
            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}