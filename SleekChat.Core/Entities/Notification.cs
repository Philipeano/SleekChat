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

        public void Deconstruct(out Guid id, out Guid recipientId, out Guid messageId, out string status, out DateTime received)
        {
            id = Id;
            recipientId = RecipientId;
            messageId = MessageId;
            status = Status.ToString();
            received = DateCreated;
        }
    }
}
