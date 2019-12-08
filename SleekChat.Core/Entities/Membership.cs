using System;
namespace SleekChat.Core.Entities
{
    public class Membership
    {
        public Guid Id { get; set; }

        public Guid GroupId { get; set; }

        public Guid MemberId { get; set; }

        public string MemberRole { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
