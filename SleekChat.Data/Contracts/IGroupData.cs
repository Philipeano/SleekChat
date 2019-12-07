using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Contracts
{
    public interface IGroupData
    {
        public Group CreateNewGroup(Guid creatorId, string title, string purpose, bool isActive);

        public IEnumerable<Group> GetAllGroups();

        public Group GetGroupById(Guid groupId);

        public Group UpdateGroup(Group group);

        public void DeleteGroup(Guid groupId);

        public bool IsGroupCreator(Guid groupId, Guid userId);
    }
}
