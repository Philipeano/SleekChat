using System;
using System.Text.Json.Serialization;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Helpers
{
    public static class DataHelper
    {

        public static Guid GetGuid()
        {
            return Guid.NewGuid();
        }

        public static PriorityLevel GetPriority(string priority)
        {
            return priority switch
            {
                "Critical" => PriorityLevel.Critical,
                "Urgent" => PriorityLevel.Urgent,
                _ => PriorityLevel.Normal,
            };
        }

        public static NotificationStatus GetStatus(string status)
        {
            return status switch
            {
                "Read" => NotificationStatus.Read,
                "Archived" => NotificationStatus.Archived,
                _ => NotificationStatus.Unread,
            };
        }
    }

    /// <summary>
    /// A user with Id, Username, Email and Registered (date) fields
    /// </summary>
    public class SimplifiedUser
    {
        /// <summary>
        /// The id of the user
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The username of the user
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The email address of the user
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The date the user was registered
        /// </summary>
        public DateTime Registered { get; set; }
    }

    /// <summary>
    /// A signed-in user with Token, Id, Username, Email and Registered (date) fields
    /// </summary>
    public class AuthenticatedUser: SimplifiedUser
    {
        /// <summary>
        /// The bearer token generated for the user
        /// </summary>
        public string Token { get; set; }

        public void Deconstruct(out Guid id, out string username, out string email, out DateTime registered, out string token)
        {
            id = Id;
            username = Username;
            email = Email;
            registered = Registered;
            token = Token;
        }
    }

    /// <summary>
    /// A group with Id, Title, Purpose, Creator and Created (date) fields
    /// </summary>
    public class SimplifiedGroup
    {
        /// <summary>
        /// The id of the group
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The title/name of the group
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The purpose or description of the group
        /// </summary>
        public string Purpose { get; set; }
        [JsonIgnore]
        public Guid CreatorId { get; set; }

        /// <summary>
        /// The user who created the group
        /// </summary>
        public SimplifiedUser Creator { get; set; }

        /// <summary>
        /// The date the group was created
        /// </summary>
        public DateTime Created { get; set; }
    }

    /// <summary>
    /// A membership with Id, Group, Member, MemberRole and Joined (date) fields
    /// </summary>
    public class SimplifiedMembership
    {
        /// <summary>
        /// The id of the membership
        /// </summary>
        public Guid Id { get; set; }

        [JsonIgnore]
        public Guid GroupId { get; set; }

        /// <summary>
        /// The group to which the membership belongs
        /// </summary>
        public SimplifiedGroup Group { get; set; }

        [JsonIgnore]
        public Guid MemberId { get; set; }

        /// <summary>
        /// The member/user associated with the membership
        /// </summary>
        public SimplifiedUser Member { get; set; }

        /// <summary>
        /// The role of the member within the group
        /// </summary>
        public string MemberRole { get; set; }

        /// <summary>
        /// The date the member joined the group
        /// </summary>
        public DateTime Joined { get; set; }
    }

    /// <summary>
    /// A message with Id, Content, Status, Priority, Group, Sender and Sent (date) fields
    /// </summary>
    public class SimplifiedMessage
    {
        /// <summary>
        /// The id of the message
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The content/text of the message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The status of the message. The default is 'visible'
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The priority associated with the message. The default is 'normal'
        /// </summary>
        public string Priority { get; set; }

        [JsonIgnore]
        public Guid GroupId { get; set; }

        /// <summary>
        /// The group within which the message was sent
        /// </summary>
        public SimplifiedGroup Group { get; set; }

        [JsonIgnore]
        public Guid SenderId { get; set; }

        /// <summary>
        /// The user/member who sent the message
        /// </summary>
        public SimplifiedUser Sender { get; set; }

        /// <summary>
        /// The date the message was sent
        /// </summary>
        public DateTime Sent { get; set; }
    }

    /// <summary>
    /// A notification with Id, Recipient, Message, Status, and Received (date) fields
    /// </summary>
    public class SimplifiedNotification
    {
        /// <summary>
        /// The id of the notification
        /// </summary>
        public Guid Id { get; set; }

        [JsonIgnore]
        public Guid RecipientId { get; set; }

        /// <summary>
        /// The user/member who received the notification
        /// </summary>
        public SimplifiedUser Recipient { get; set; }
        
        [JsonIgnore]
        public Guid MessageId { get; set; }

        /// <summary>
        /// The message associated with the notification
        /// </summary>
        public SimplifiedMessage Message { get; set; }

        /// <summary>
        /// The status of the notification. The default is 'unread'
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The date the notification was received
        /// </summary>
        public DateTime Received { get; set; }
    }

    /// <summary>
    /// A notification with Id, Group, Sender, Recipient, Message, Status, and Received (date) fields
    /// </summary>
    public class BasicNotification
    {
        /// <summary>
        /// The id of the notification
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The group associated with the notification
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// The user/member who sent the message associated with the notification
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// The user/member who received the notification
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// The content/text of the message associated with the notification
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The status of the notification. The default is 'unread'
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The date the notification was received
        /// </summary>
        public DateTime Received { get; set; }
    }
}