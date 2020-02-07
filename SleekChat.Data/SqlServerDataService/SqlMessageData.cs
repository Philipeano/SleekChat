using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.SqlServerDataService
{
    public class SqlMessageData : IMessageData
    {
        private readonly SleekChatContext dbcontext;
        private readonly IMembershipData membershipData;
        private readonly INotificationData notificationData;

        public SqlMessageData(SleekChatContext dbcontext, IMembershipData membershipData, INotificationData notificationData)
        {
            this.dbcontext = dbcontext;
            this.membershipData = membershipData;
            this.notificationData = notificationData;
        }

        public Message CreateNewMessage(string content, Guid senderId, Guid groupId, PriorityLevel priority, MessageStatus status)
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
            dbcontext.Messages.Add(newMessage);

            // Create a notification for each group member except the message sender
            NotifyGroupMembers(groupId, newMessage, out _);
            Commit();
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
            return dbcontext.Messages;
        }

        public IEnumerable<Message> GetAllMessagesFromAUser(Guid senderId)
        {
            return dbcontext.Messages.Where(m => m.SenderId == senderId);
        }

        public IEnumerable<Message> GetGroupMessages(Guid groupId)
        {
            return dbcontext.Messages.Where(m => m.GroupId == groupId);
        }

        public Message GetGroupMessageById(Guid groupId, Guid messageId)
        {
            return dbcontext.Messages.SingleOrDefault(m => m.GroupId == groupId && m.Id == messageId);
        }

        public IEnumerable<Message> GetGroupMessagesFromAUser(Guid groupId, Guid senderId)
        {
            return dbcontext.Messages.Where(m => m.GroupId == groupId && m.SenderId == senderId);
        }

        public Message GetMessageById(Guid messageId)
        {
            return dbcontext.Messages.SingleOrDefault(m => m.Id == messageId);
        }

        public bool IsMessageSender(Guid messageId, Guid userId)
        {
            return (dbcontext.Messages.SingleOrDefault(m => m.Id == messageId && m.SenderId == userId) != null);
        }

        public Message UpdateMessage(Guid id, string content, PriorityLevel priority, out Message updatedMessage)
        {
            updatedMessage = GetMessageById(id);
            updatedMessage.Content = content;
            updatedMessage.Priority = priority;

            EntityEntry<Message> messageEntity = dbcontext.Messages.Attach(updatedMessage);
            messageEntity.State = EntityState.Modified;
            Commit();
            return updatedMessage;
        }

        public void DeleteMessage(Guid messageId)
        {
            Message message = GetMessageById(messageId);
            if (message != null)
            {
                dbcontext.Messages.Remove(message);
                Commit();
            }
        }

        public int Commit()
        {
            return dbcontext.SaveChanges();
        }
    }
}
