using System;
namespace SleekChat.Core.Entities
{
    public class Notification
    {

        public Guid Id { get; set; }

        public Guid RecipientId { get; set; }

        public Guid MessageId { get; set; }

        public NotificationStatus Status { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
