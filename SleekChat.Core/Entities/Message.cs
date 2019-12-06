using System;
namespace SleekChat.Core.Entities
{
    public class Message
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public MessageStatus Status { get; set; }

        public PriorityLevel Priority { get; set; }

        public Guid SenderId { get; set; }

        public Guid GroupId { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
