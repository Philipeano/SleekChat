using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace SleekChat.Data.Helpers
{
    public class HttpHelper
    {

        public void Forbid(HttpResponse responseObj, string responseJson)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            responseObj.StatusCode = 403;
            responseObj.ContentType = "application/json";
            _ = responseObj.WriteAsync(responseJson, System.Text.Encoding.Default, token);
            source.Dispose();
        }
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

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, null);
        }
    }


    // Request body for ..api/Users/authenticate
    public class AuthReqBody
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }


    // Request body for ..api/Users/register
    public class UserReqBody
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CPassword { get; set; }

        public void Deconstruct(out string username, out string email, out string password, out string cPassword)
        {
            username = Username;
            email = Email;
            password = Password;
            cPassword = CPassword;
        }
    }


    // Request body for ..api/Groups
    public class GroupReqBody
    {
        public string Title { get; set; }
        public string Purpose { get; set; }
        public bool IsActive { get; set; }

        public void Deconstruct(out string title, out string purpose, out bool isActive)
        {
            title = Title;
            purpose = Purpose;
            isActive = IsActive;
        }
    }


    // Request body for ..api/Memberships
    public class MbrshpReqBody
    {
        public string MemberId { get; set; }
        public string MemberRole { get; set; }
    }


    // Request body for ..api/Messages
    public class MsgRequestBody
    {
        public string Content { get; set; }
        public string Priority { get; set; }

        public void Deconstruct(out string content, out string priority)
        {
            content = Content;
            priority = Priority;
        }
    }


    // Request body for ..api/Notifications
    public class NotftnReqBody
    {
        public string NotificationStatus { get; set; }
    }
}