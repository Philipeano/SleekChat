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
        private readonly IMembershipData membershipData;
        private readonly INotificationData notificationData;

        public MessageData(IMembershipData membershipData, INotificationData notificationData)
        {
            messages = new List<Message> { };
            this.membershipData = membershipData;
            this.notificationData = notificationData;
        }

        public Message CreateNewMessage(string content, Guid senderId, Guid groupId, PriorityLevel priority, MessageStatus status = MessageStatus.Visible)
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

            // Create a notification for each group member except the message sender
            NotifyGroupMembers(groupId, newMessage, out _);
            return newMessage;
        }

        public void NotifyGroupMembers(Guid groupId, Message message, out int recipientCount)
        {
            IEnumerable<Membership> groupMemberships = membershipData.GetAllMemberships().Where(m => m.GroupId == groupId && m.MemberId != message.SenderId);
            foreach (Membership membership in groupMemberships)
            {
                notificationData.CreateNewNotification(membership.MemberId, message.Id, NotificationStatus.Unread, DateTime.Now);
            } 
            recipientCount = groupMemberships.Count();
        }

        public IEnumerable<Message> GetAllMessages()
        {
            return messages;
        }

        public IEnumerable<Message> GetAllMessagesFromAUser(Guid senderId)
        {
            return messages.FindAll(m => m.SenderId == senderId);
        }

        public IEnumerable<Message> GetGroupMessages(Guid groupId)
        {
            return messages.FindAll(m => m.GroupId == groupId);
        }

        public Message GetGroupMessageById(Guid groupId, Guid messageId) 
        {
            return messages.SingleOrDefault(m => m.GroupId == groupId && m.Id == messageId);
        }

        public IEnumerable<Message> GetGroupMessagesFromAUser(Guid groupId, Guid senderId)
        {
            return messages.FindAll(m => m.GroupId == groupId && m.SenderId == senderId);
        }

        public Message GetMessageById(Guid messageId)
        {
            return messages.SingleOrDefault(m => m.Id == messageId);
        }

        public Message UpdateMessage(Guid id, string content, PriorityLevel priority, out Message updatedMessage)
        {
            IEnumerable<Message> query = messages.Where(m => m.Id == id)
                       .Select(m =>
                       {
                           m.Content = content;
                           m.Priority = priority;
                           return m;
                       });
            updatedMessage = query.First();
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
