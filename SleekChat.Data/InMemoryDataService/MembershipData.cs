using System;
using System.Collections.Generic;
using System.Linq;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.InMemoryDataService
{
    public class MembershipData : IMembershipData
    {

        private readonly List<Membership> memberships;

        public MembershipData()
        {
            memberships = new List<Membership> { };
        }

        public Membership AddGroupMember(Guid groupId, Guid memberId, string memberRole)
        {
            Membership newMembership = new Membership
            {
                Id = DataHelper.GetGuid(),
                GroupId = groupId,
                MemberId = memberId,
                MemberRole = memberRole,
                DateCreated = DateTime.Now
            };
            memberships.Add(newMembership);
            return newMembership;
        }

        public IEnumerable<Membership> GetAllMemberships()
        {
            return memberships;
        }

        public IEnumerable<Membership> GetGroupMemberships(Guid groupId)
        {
            return memberships.FindAll(m => m.GroupId == groupId);
        }

        public IEnumerable<Membership> GetMembershipsForAUser(Guid userId)
        {
            return memberships.FindAll(m => m.MemberId == userId);
        }

        public void RemoveGroupMember(Guid groupId, Guid userId)
        {
            memberships.RemoveAll(m => m.GroupId == groupId && m.MemberId == userId);
        }

        public bool IsGroupMember(Guid groupId, Guid userId)
        {
            return (memberships.SingleOrDefault(m => m.GroupId == groupId && m.MemberId == userId) != null);
        }
    }
}
