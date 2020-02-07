using System;
using System.Collections.Generic;
using System.Linq;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.SqlServerDataService
{
    public class SqlMembershipData : IMembershipData
    {
        private readonly SleekChatContext dbcontext;

        public SqlMembershipData(SleekChatContext dbcontext)
        {
            this.dbcontext = dbcontext;
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
            dbcontext.Memberships.Add(newMembership);
            Commit();
            return newMembership;
        }

        public IEnumerable<Membership> GetAllMemberships()
        {
            return dbcontext.Memberships;
        }

        public IEnumerable<Membership> GetGroupMemberships(Guid groupId)
        {
            return dbcontext.Memberships.Where(m => m.GroupId == groupId);
        }

        public IEnumerable<Membership> GetMembershipsForAUser(Guid userId)
        {
            return dbcontext.Memberships.Where(m => m.MemberId == userId);
        }

        public void RemoveGroupMember(Guid groupId, Guid userId)
        {
            Membership membership = dbcontext.Memberships.FirstOrDefault(m => m.GroupId == groupId && m.MemberId == userId);
            if (membership != null)
            {
                dbcontext.Memberships.Remove(membership);
                Commit();
            }
        }

        public bool IsGroupMember(Guid groupId, Guid userId)
        {
            return dbcontext.Memberships.FirstOrDefault(m => m.GroupId == groupId && m.MemberId == userId) != null;
        }

        public int Commit()
        {
            return dbcontext.SaveChanges();
        }
    }
}
