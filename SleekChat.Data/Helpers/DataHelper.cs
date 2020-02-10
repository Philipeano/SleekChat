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

        public static string Encrypt(string rawText)
        {
            string encryptedText = rawText;
            // Perform encryption here
            return encryptedText;
        }

        public static string Decrypt(string encryptedText)
        {
            string decryptedText = encryptedText;
            // Perform decryption here
            return decryptedText;
        }
        public static PriorityLevel GetPriority(string priority)
        {
            switch (priority)
            {
                case "Critical":
                    return PriorityLevel.Critical;
                case "Urgent":
                    return PriorityLevel.Urgent;
                default:
                    return PriorityLevel.Normal;
            }
        }
        public static NotificationStatus GetStatus(string status)
        {
            switch (status)
            {
                case "Read":
                    return NotificationStatus.Read;
                case "Archived":
                    return NotificationStatus.Archived;
                default:
                    return NotificationStatus.Unread;
            }
        }        
    }


    public class SimplifiedUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime Registered { get; set; }
    }

    public class SimplifiedGroup
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Purpose { get; set; }
        [JsonIgnore]
        public Guid CreatorId { get; set; }
        public SimplifiedUser Creator { get; set; }
        public DateTime Created { get; set; }
    }

    public class SimplifiedMembership
    {
        public Guid Id { get; set; }
        [JsonIgnore]
        public Guid GroupId { get; set; }
        public SimplifiedGroup Group { get; set; }
        [JsonIgnore]
        public Guid MemberId { get; set; }
        public SimplifiedUser Member { get; set; }
        public string MemberRole { get; set; }
        public DateTime Joined { get; set; }
    }

    public class SimplifiedMessage
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        [JsonIgnore]
        public Guid GroupId { get; set; }
        public SimplifiedGroup Group { get; set; }
        [JsonIgnore]
        public Guid SenderId { get; set; }
        public SimplifiedUser Sender { get; set; }
        public DateTime Sent { get; set; }
    }

    public class SimplifiedNotification
    {
        public Guid Id { get; set; }
        [JsonIgnore]
        public Guid RecipientId { get; set; }
        public SimplifiedUser Recipient { get; set; }
        [JsonIgnore]
        public Guid MessageId { get; set; }
        public SimplifiedMessage Message { get; set; }
        public string Status { get; set; }
        public DateTime Received { get; set; }
    }
}
