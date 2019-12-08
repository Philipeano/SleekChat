using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Contracts
{
    public interface INotificationData
    {
        public Notification CreateNewNotification(Guid notificationId, Guid recipientId, Guid messageId, string status, DateTime dateCreated);

        public IEnumerable<Notification> GetNotificationsForUser(Guid userId);

        public string DeleteNotification(Guid notificationId);
    }
}
