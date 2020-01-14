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

        public void Deconstruct(out Guid id, out Guid groupId, out Guid memberId, out string role, out DateTime joined)
        {
            id = Id;
            groupId = GroupId;
            memberId = MemberId;
            role = MemberRole;
            joined = DateCreated;
        }
    }
}
