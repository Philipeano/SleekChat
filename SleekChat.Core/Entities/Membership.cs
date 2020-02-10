using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SleekChat.Core.Entities
{
    public class Membership
    {
        [Column("MembershipId")]
        public Guid Id { get; set; }

        public Guid GroupId { get; set; }

        public Guid MemberId { get; set; }

        public string MemberRole { get; set; }

        public DateTime DateCreated { get; set; }

        /* RELATIONSHIPS ------------------------------------------------------------
         * Membership is a join table for User and Group many-to-many relationships
        ---------------------------------------------------------------------------*/
        //[Required]
        public Group Group { get; set; }

        //[Required]
        public User Member { get; set; }


        public void Deconstruct(out Guid id, out Guid groupId, out Group group, out Guid memberId, out User member,
                                out string role, out DateTime joined)
        {
            id = Id;
            groupId = GroupId;
            group = Group;
            memberId = MemberId;
            member = Member;
            role = MemberRole;
            joined = DateCreated;
        }
    }
}
