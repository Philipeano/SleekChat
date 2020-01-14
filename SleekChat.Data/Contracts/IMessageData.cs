using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Contracts
{
    public interface IMessageData
    {
        public Message CreateNewMessage(string content, Guid senderId, Guid groupId, PriorityLevel priority, MessageStatus status);

        public void NotifyGroupMembers(Guid groupId, Message message, out int recipientCount);

        public IEnumerable<Message> GetAllMessages();

        public IEnumerable<Message> GetAllMessagesFromAUser(Guid senderId);

        public IEnumerable<Message> GetGroupMessages(Guid groupId);

        public Message GetGroupMessageById(Guid groupId, Guid messageId);

        public IEnumerable<Message> GetGroupMessagesFromAUser(Guid groupId, Guid senderId);

        public Message GetMessageById(Guid messageId);

        public Message UpdateMessage(Guid id, string content, PriorityLevel priority, out Message updatedMessage);

        public void DeleteMessage(Guid messageId);

        public bool IsMessageSender(Guid messageId, Guid userId);
    }
}
