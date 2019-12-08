using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Contracts
{
    public interface IMembershipData
    {
        public Membership AddGroupMember(Guid groupId, Guid memberId, string memberRole);

        public IEnumerable<Membership> GetAllMemberships();

        public IEnumerable<Membership> GetGroupMemberships(Guid groupId);

        public IEnumerable<Membership> GetMembershipsForAUser(Guid userId);

        public void RemoveGroupMember(Guid groupId, Guid userId);

        public bool IsGroupMember(Guid groupId, Guid userId);
    }
}
