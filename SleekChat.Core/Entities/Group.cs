using System;
namespace SleekChat.Core.Entities
{
    public class Group
    {
        public Guid Id { get; set; }

        public Guid CreatorId { get; set; }

        public string Title { get; set; }

        public string Purpose { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateCreated { get; set; }

    }
}
