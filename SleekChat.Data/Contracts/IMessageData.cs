using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Contracts
{
    public interface IMessageData
    {
        public Message CreateNewMessage(string content, Guid senderId, Guid groupId, MessageStatus status, PriorityLevel priority);

        public IEnumerable<Message> GetAllMessages();

        public Message GetMessageById(Guid messageId);

        public IEnumerable<Message> GetGroupMessages(Guid groupId);

        public Message UpdateMessage(Message message);

        public void DeleteMessage(Guid messageId);

        public bool IsMessageSender(Guid messageId, Guid userId);
    }
}
