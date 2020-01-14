using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Contracts
{
    public interface INotificationData
    {
        public Notification CreateNewNotification(Guid recipientId, Guid messageId, NotificationStatus status, DateTime dateCreated);

        public IEnumerable<Notification> GetAllNotifications();

        public IEnumerable<Notification> GetNotificationsForAUser(Guid userId);

        public Notification GetNotificationById(Guid notificationId);

        public Notification UpdateNotification(Guid id, NotificationStatus status, out Notification updatedNotification);

        public void DeleteNotification(Guid notificationId);

        public bool IsNotificationRecipient(Guid notificationId, Guid userId);
    }
}
