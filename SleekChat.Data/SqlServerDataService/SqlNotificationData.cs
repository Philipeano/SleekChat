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
    public class SqlNotificationData : INotificationData
    {
        private readonly SleekChatContext dbcontext;

        public SqlNotificationData(SleekChatContext dbcontext)
        {
            this.dbcontext = dbcontext;
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
            dbcontext.Notifications.Add(newNotification);
            return newNotification;
        }

        public IEnumerable<Notification> GetAllNotifications()
        {
            return dbcontext.Notifications
                .Include(n => n.Recipient).Where(n => n.Recipient.IsActive == true)
                .Include(n => n.Message);
        }

        public IEnumerable<Notification> GetNotificationsForAUser(Guid userId)
        {
            return dbcontext.Notifications
                .Include(n => n.Recipient).Where(n => n.Recipient.IsActive == true)
                .Include(n => n.Message)
                .Where(n => n.RecipientId == userId);
        }

        public Notification GetNotificationById(Guid notificationId)
        {
            return dbcontext.Notifications
                .Include(n => n.Recipient).Where(n => n.Recipient.IsActive == true)
                .Include(n => n.Message)
                .SingleOrDefault(n => n.Id == notificationId);
        }

        public Notification UpdateNotification(Guid id, NotificationStatus status, out Notification updatedNotification)
        {
            updatedNotification = GetNotificationById(id);
            updatedNotification.Status = status;

            EntityEntry<Notification> entry = dbcontext.Notifications.Attach(updatedNotification);
            entry.State = EntityState.Modified;
            Commit();
            return updatedNotification;
        }

        public void DeleteNotification(Guid notificationId)
        {
            Notification notification = GetNotificationById(notificationId);
            if (notification != null)
            {
                dbcontext.Notifications.Remove(notification);
                Commit();
            }
        }

        public bool IsNotificationRecipient(Guid notificationId, Guid userId)
        {
            return dbcontext.Notifications
                .SingleOrDefault(n => n.Id == notificationId && n.RecipientId == userId) != null;
        }

        public int Commit()
        {
            return dbcontext.SaveChanges();
        }
    }
}
