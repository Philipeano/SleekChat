using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Contracts
{
    public interface IMembershipData
    {
        public Membership AddGroupMember(Guid membershipId, Guid groupId, Guid memberId, string memberRole, DateTime dateCreated);

        public IEnumerable<Membership> GetAllMemberships();

        public IEnumerable<Membership> GetGroupMemberships(Guid groupId);

        public bool IsGroupMember(Guid groupId, Guid userId);

        public string RemoveGroupMember(Guid groupId, Guid userId);
    }
}
