using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Contracts
{
    public interface IMessageData
    {
        public Message CreateNewMessage(Guid messageId, string content, string status, string priority, Guid senderId, Guid groupId, DateTime dateCreated);

        public IEnumerable<Message> GetAllMessages();

        public IEnumerable<Message> GetGroupMessages(Guid groupId);

        public bool IsMessageSender(Guid messageId, Guid userId);

        public string DeleteMessage(Guid messageId);
    }
}
