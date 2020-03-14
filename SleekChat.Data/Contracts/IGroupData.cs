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

        public Group UpdateGroup(Guid id, string title, string purpose, out Group updatedGroup);

        public void DeleteGroup(Guid groupId);

        public bool IsGroupCreator(Guid groupId, Guid userId);

        public bool TitleAlreadyTaken(string title, out Group matchingGroup);
    }
}
