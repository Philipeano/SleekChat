using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SleekChat.Core.Entities
{
    public class Notification
    {
        [Column("NotificationId")]
        public Guid Id { get; set; }

        public Guid RecipientId { get; set; }

        public Guid MessageId { get; set; }

        public NotificationStatus Status { get; set; }

        public DateTime DateCreated { get; set; }

        /* RELATIONSHIPS ---------------------------------------------------------------------------------
         * A Message can generate many Notifications while a Notification must be attached to one Message 
         * A Notification must have one Recipient (User) while a User can have many Notifications
        ------------------------------------------------------------------------------------------------*/
        //[Required]
        public User Recipient { get; set; }

        //[Required]
        public Message Message { get; set; }


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
