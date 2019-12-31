using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Helpers
{
    public class FormatHelper
    {

        public Response Render(dynamic output, string outputType, Operation operation)
        {
            Response response = new Response
            {
                Status = "normal",
                Message = $"{outputType} {operation.ToString().ToLower()} successfully!",
                Data = (output == null) ? null : Simplify(outputType, output)
            };
            return response;
        }


        public Response Render(KeyValuePair<bool, string> validationResult)
        {
            Response response = new Response
            {
                Status = "error",
                Message = validationResult.Value,
                Data = null
            };
            return response;
        }


        private dynamic Simplify(string type, dynamic item)
        {
            dynamic result = null;
            switch (type) 
            {
                case "User":
                    result = new SimplifiedUser();
                    (result.Id, result.Username, result.Email, result.Registered) = (User)item;
                    break;
                case "Group":
                    result = new SimplifiedGroup();
                    (result.Id, result.Title, result.Purpose, result.CreatorId, result.Created) = (Group)item;
                    break;
                    //case "Membership":
                    //    result = new SimplifiedMembership();
                    //    (result.Id, result.GroupId, result.MemberId, result.MemberRole, result.Joined) = (Membership)item;
                    //    break;
                    //case "Message":
                    //    result = new SimplifiedMessage();
                    //    (result.Id, result.Content, result.Status, result.Priority, result.SenderId, result.GroupId, result.Sent) = (Message)item;
                    //    break;
                    //case "Notification":
                    //    result = new SimplifiedNotification();
                    //    (result.Id, result.RecipientId, result.MessageId, result.Status, result.Received) = (Message)item;
                    //    break;
            }
            return result;
        }


        private dynamic Simplify(string type, IEnumerable<dynamic> items)
        {
            string itemType = type[0..^1];
            List<dynamic> result = new List<dynamic>();
            foreach(var item in items)
            {
                result.Add(Simplify(itemType, item));
            }
            return result;
        }
    }


    public class Response
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
    }


    public class Request
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
        public PriorityLevel Priority { get; set; }
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
            title = Title;
            purpose = Purpose;
            isActive = IsActiveGroup;
        }

        // Request body for ..api/Memberships
        public void Deconstruct(out string memberId, out string role)
        {
            memberId = MemberId;
            role = MemberRole;
        }

        // Request body for ..api/Messages
        public void Deconstruct(out string content, out PriorityLevel priority)
        {
            //HINT: Pass senderId in request header 
            content = Content;
            priority = Priority;
        }

        // Request body for ..api/Notifications
        public void Deconstruct(out string status)
        {
            status = NotificationStatus;
        }
    }


    public enum Operation
    {
        Created,
        Retrieved,
        Updated,
        Deleted
    }
}


