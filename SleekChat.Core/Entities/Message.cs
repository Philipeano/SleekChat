using System;
namespace SleekChat.Core.Entities
{
    public class Message
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public MessageStatus Status { get; set; }

        public PriorityLevel Priority { get; set; }

        public Guid GroupId { get; set; }

        public Guid SenderId { get; set; }

        public DateTime DateCreated { get; set; }

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
