using System;
using System.Collections.Generic;
using System.Linq;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.InMemoryDataService
{
    public class NotificationData : INotificationData
    {

        private readonly List<Notification> notifications;

        public NotificationData()
        {
            notifications = new List<Notification> { };
        }

        public Notification CreateNewNotification(Guid recipientId, Guid messageId, NotificationStatus status, DateTime dateCreated)
        {
            Notification newNotification = new Notification
            {
                Id = DataHelper.GetGuid(),
                RecipientId = recipientId,
                MessageId = messageId,
                Status = status,
                DateCreated = DateTime.Now
            };
            notifications.Add(newNotification);
            return newNotification;
        }

        public IEnumerable<Notification> GetAllNotifications() 
        {
            return notifications;
        }

        public IEnumerable<Notification> GetNotificationsForAUser(Guid userId)
        {
            return notifications.FindAll(n => n.RecipientId == userId);
        }

        public Notification GetNotificationById(Guid notificationId) 
        {
            return notifications.SingleOrDefault(n => n.Id == notificationId);
        }

        public Notification UpdateNotification(Guid id, NotificationStatus status, out Notification updatedNotification)
        {
            IEnumerable<Notification> query = notifications.Where(n => n.Id == id)
                       .Select(n =>
                       {
                           n.Status = status;
                           return n;
                       });
            updatedNotification = query.First();
            return updatedNotification;            
        }

        public void DeleteNotification(Guid notificationId)
        {
            notifications.RemoveAll(n => n.Id == notificationId);
        }

        public bool IsNotificationRecipient(Guid notificationId, Guid userId)        
        {
            return (notifications.SingleOrDefault(n => n.Id == notificationId && n.RecipientId == userId) != null);
        }
    }
}
