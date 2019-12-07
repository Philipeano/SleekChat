using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data
{
    public interface ISleekChatData
    {

        // User contract
        public User CreateNewUser(Guid userId, string username, string email, string password, bool isActive, DateTime dateCreated);

        public IEnumerable<User> GetAllUsers();

        public User GetSpecificUser(Guid userId);

        public string DeleteUser(Guid groupId);


        // Group contract
        public User CreateNewGroup(Guid groupId, Guid creatorId, string title, string purpose, bool isActive, DateTime dateCreated);

        public IEnumerable<Group> GetAllGroups();

        public Group GetSpecificGroup(Guid groupId);

        public bool IsGroupCreator(Guid groupId, Guid userId);

        public string DeactivateGroup(Guid groupId);

        public string DeleteGroup(Guid groupId);


        // Membership contract
        public Membership AddGroupMember(Guid membershipId, Guid groupId, Guid memberId, string memberRole, DateTime dateCreated);

        public IEnumerable<Membership> GetAllMemberships();

        public IEnumerable<Membership> GetGroupMemberships(Guid groupId);

        public bool IsGroupMember(Guid groupId, Guid userId);

        public string RemoveGroupMember(Guid groupId, Guid userId);


        // Message contract
        public Message CreateNewMessage(Guid messageId, string content, string status, string priority, Guid senderId, Guid groupId, DateTime dateCreated);

        public IEnumerable<Message> GetAllMessages();

        public IEnumerable<Message> GetGroupMessages(Guid groupId);

        public bool IsMessageSender(Guid messageId, Guid userId);

        public string DeleteMessage(Guid messageId);


        // Notification contract
        public Notification CreateNewNotification(Guid notificationId, Guid recipientId, Guid messageId, string status, DateTime dateCreated);

        public IEnumerable<Notification> GetNotificationsForUser(Guid userId);

        public string DeleteNotification(Guid notificationId);

    }
}