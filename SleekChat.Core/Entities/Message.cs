using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SleekChat.Core.Entities
{
    public class Message
    {
        [Column("MessageId")]
        public Guid Id { get; set; }

        public string Content { get; set; }

        public MessageStatus Status { get; set; }

        public PriorityLevel Priority { get; set; }

        public Guid GroupId { get; set; }

        public Guid SenderId { get; set; }

        public DateTime DateCreated { get; set; }

        /* RELATIONSHIPS -------------------------------------------------------------
         * A Message must have one Sender (User) while a User can send many Messages
         * A Message must have one target Group while a Group can have many Messages
         * A Message can generate many Notifications while a Notification must be attached to one Message 
        -----------------------------------------------------------------------------*/
        [Required]
        public Group Group { get; set; }

        [Required]
        public User Sender { get; set; }

        public List<Notification> Notifications { get; set; }


        public void Deconstruct(out Guid id, out string content, out string status, out string priority, out Guid groupId, out Guid senderId, out DateTime sent)
        {
            id = Id;
            content = Content;
            status = Status.ToString();
            priority = Priority.ToString();
            groupId = GroupId;
            senderId = SenderId;
            sent = DateCreated;
        }
    }
}