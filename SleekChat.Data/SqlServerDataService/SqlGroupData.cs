using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.SqlServerDataService
{
    public class SqlGroupData : IGroupData
    {
        private readonly SleekChatContext dbcontext;
        private readonly IMembershipData membershipData;

        public SqlGroupData(SleekChatContext dbcontext, IMembershipData membershipData)
        {
            this.dbcontext = dbcontext;
            this.membershipData = membershipData;
        }

        public Group CreateNewGroup(Guid creatorId, string title, string purpose, bool isActive)
        {
            Group newGroup = new Group
            {
                Id = DataHelper.GetGuid(),
                CreatorId = creatorId,
                Title = title,
                Purpose = purpose,
                IsActive = isActive,
                DateCreated = DateTime.Now
            };
            dbcontext.Groups.Add(newGroup);

            // Add creator as group member
            _ = membershipData.AddGroupMember(newGroup.Id, creatorId, "Creator");
            Commit();
            return newGroup;
        }

        public IEnumerable<Group> GetAllGroups()
        {
            return dbcontext.Groups;
        }

        public Group GetGroupById(Guid groupId)
        {
            return dbcontext.Groups.SingleOrDefault(g => g.Id == groupId);
        }

        public Group UpdateGroup(Guid id, string title, string purpose, bool isActive, out Group updatedGroup)
        {
            updatedGroup = GetGroupById(id);
            updatedGroup.Title = title;
            updatedGroup.Purpose = purpose;
            updatedGroup.IsActive = isActive;

            EntityEntry<Group> groupEntity = dbcontext.Groups.Attach(updatedGroup);
            groupEntity.State = EntityState.Modified;
            Commit();
            return updatedGroup;
        }

        public void DeleteGroup(Guid groupId)
        {
            Group group = GetGroupById(groupId);
            if (group != null)
            {
                dbcontext.Groups.Remove(group);
                Commit();
            }
        }

        public bool IsGroupCreator(Guid groupId, Guid userId)
        {
            return (dbcontext.Groups.SingleOrDefault(g => g.Id == groupId && g.CreatorId == userId) != null);
        }

        public bool TitleAlreadyTaken(string title, out Group matchingGroup)
        {
            matchingGroup = dbcontext.Groups.SingleOrDefault(g => g.Title == title);
            return matchingGroup != null;
        }

        public int Commit()
        {
            return dbcontext.SaveChanges();
        }
    }
}
