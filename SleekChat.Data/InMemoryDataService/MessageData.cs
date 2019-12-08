using System;
using System.Collections.Generic;
using System.Linq;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.InMemoryDataService
{
    public class MessageData : IMessageData
    {

        private readonly List<Message> messages;

        public MessageData()
        {
            messages = new List<Message> { };
        }

        public Message CreateNewMessage(string content, Guid senderId, Guid groupId, MessageStatus status = MessageStatus.Visible, PriorityLevel priority = PriorityLevel.Normal)
        {
            Message newMessage = new Message
            {
                Id = DataHelper.GetGuid(),
                Content = content,
                Status = status,
                Priority = priority,
                SenderId = senderId,
                GroupId = groupId,
                DateCreated = DateTime.Now
            };
            messages.Add(newMessage);
            return newMessage;
        }

        public IEnumerable<Message> GetAllMessages()
        {
            return messages;
        }

        public Message GetMessageById(Guid messageId)
        {
            return messages.SingleOrDefault(m => m.Id == messageId);
        }

        public IEnumerable<Message> GetGroupMessages(Guid groupId)
        {
            return messages.FindAll(m => m.GroupId == groupId);
        }

        public Message UpdateMessage(Message message)
        {
            Message updatedMessage = new Message { };
            messages.Where(m => m.Id == message.Id)
                       .Select(m =>
                       {
                           m.Content = message.Content;
                           m.Status = message.Status;
                           m.Priority = message.Priority;
                           updatedMessage = m;
                           return m;
                       })
                       .ToList();
            return updatedMessage;
        }

        public void DeleteMessage(Guid messageId)
        {
            messages.RemoveAll(m => m.Id == messageId);
        }

        public bool IsMessageSender(Guid messageId, Guid userId)
        {
            return (messages.SingleOrDefault(m => m.Id == messageId && m.SenderId == userId) != null);
        }
    }
}
