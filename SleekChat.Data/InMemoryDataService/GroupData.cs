using System;
using System.Collections.Generic;
using System.Linq;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.InMemoryDataService
{
    public class GroupData : IGroupData
    {
        private readonly List<Group> groups;

        public GroupData()
        {
            groups = new List<Group> { };
        }

        public Group CreateNewGroup(Guid creatorId, string title, string purpose, bool isActive = true)
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
            groups.Add(newGroup);

            // Add creator as group member
            MembershipData membershipData = new MembershipData();
            _ = membershipData.AddGroupMember(newGroup.Id, creatorId, "Creator");

            return newGroup;            
        }

        public IEnumerable<Group> GetAllGroups()
        {
            return groups;
        }

        public Group GetGroupById(Guid groupId)
        {
            return groups.SingleOrDefault(g => g.Id == groupId);
        }

        public Group UpdateGroup(Guid id, string title, string purpose, bool isActive, out Group updatedGroup)
        {
            IEnumerable<Group> query = groups.Where(g => g.Id == id)
                       .Select(g =>
                       {
                           g.Title = title;
                           g.Purpose = purpose;
                           g.IsActive = isActive;
                           return g;
                       });
            updatedGroup = query.First();
            return updatedGroup;
        }

        public void DeleteGroup(Guid groupId)
        {
            groups.RemoveAll(g => g.Id == groupId);
        }

        public bool IsGroupCreator(Guid groupId, Guid userId)
        {
            return (groups.SingleOrDefault(g => g.Id == groupId && g.CreatorId == userId) != null);
        }

        public bool TitleAlreadyTaken(string title, out Group matchingGroup)
        {
            matchingGroup = groups.SingleOrDefault(g => g.Title == title);
            return matchingGroup != null;
        }
    }
}


