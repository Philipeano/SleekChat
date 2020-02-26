using System.Text.Json;
using System.Threading;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Helpers
{
    public abstract class HttpHelper
    {

    }


    public enum Operation
    {
        Created,
        Retrieved,
        Updated,
        Deleted,
        Registered,
        Posted,
        Authenticated
    }


    public class ResponseBody
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }

        public override string ToString() => JsonSerializer.Serialize(this, null);
    }


    public class AuthRequestBody
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }


    public class RequestBody
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CPassword { get; set; }
        public bool IsActiveUser { get; set; }
        public string GroupId { get; set; }
        public string MemberId { get; set; }
        public string Title { get; set; }
        public string Purpose { get; set; }
        public bool IsActiveGroup { get; set; }
        public string CreatorId { get; set; }
        public string MemberRole { get; set; }
        public string Content { get; set; }
        public string Priority { get; set; }
        public string NotificationStatus { get; set; }
        public string SenderId { get; set; }

        // Request body for ..api/Users
        public void Deconstruct(out string username, out string email, out string password, out string cPassword, out bool isActive)
        {
            username = Username;
            email = Email;
            password = Password;
            cPassword = CPassword;
            isActive = IsActiveUser;
        }

        // Request body for ..api/Groups
        public void Deconstruct(out string title, out string purpose, out bool isActive)
        {
            //HINT: Pass creatorId in request header as userId
            title = Title;
            purpose = Purpose;
            isActive = IsActiveGroup;
        }

        // Request body for ..api/Memberships
        public void Deconstruct(out string memberId, out string role, out string groupId, out bool isDefaultMember)
        {
            memberId = MemberId;
            role = MemberRole;
            groupId = null;
            isDefaultMember = false;
        }

        // Request body for ..api/Messages
        // public void Deconstruct(out string content, out PriorityLevel priority)
        public void Deconstruct(out string content, out string priority)
        {
            //HINT: Pass senderId in request header as userId 
            content = Content;
            priority = Priority;
        }

        // Request body for ..api/Notifications
        public void Deconstruct(out string status)
        {
            status = NotificationStatus;
        }
    }
}
