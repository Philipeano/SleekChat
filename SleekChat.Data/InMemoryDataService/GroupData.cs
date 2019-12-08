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

        public Group UpdateGroup(Group group)
        {
            Group updatedGroup = new Group { };
            groups.Where(g => g.Id == group.Id)
                       .Select(g => {
                           g.Title = group.Title;
                           g.Purpose = group.Purpose;
                           g.IsActive = group.IsActive;
                           updatedGroup = g;
                           return g;
                       })
                       .ToList();
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

    }

}


