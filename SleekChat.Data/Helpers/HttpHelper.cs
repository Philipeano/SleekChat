using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace SleekChat.Data.Helpers
{
    public class HttpHelper
    {
        /// <summary>
        /// A custom HTTP handler for generating 403Forbidden responses where the built-in handlers are inadequate
        /// </summary>
        /// <param name="responseObj">The response object which the server generated for the current request</param>
        /// <param name="responseJson">A string representing the JSON payload to be included in the response</param>
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

    /// <summary>
    /// A list of operations that can be performed on the entities. 
    /// These items are helpful when generating response messages.
    /// </summary>
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


    /// <summary>
    /// The default response body for all requests to this API with Status, Message and Data fields 
    /// </summary>
    public class ResponseBody
    {
        /// <summary>
        /// An one-word indication of whether the operation succeeded (normal) or failed (error)
        /// </summary>
        [Required]
        public string Status { get; set; }

        /// <summary>
        /// A one-sentence report of the operation's outcome, or any error encountered
        /// </summary>
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// This dynamic property, if not null, contains the payload returned for a given request
        /// </summary>
        public dynamic Data { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, null);
        }
    }


    // Request body for ..api/Users/authenticate
    /// <summary>
    /// Login details for a user, with Username and Password fields
    /// </summary>
    public class AuthReqBody
    {
        /// <summary>
        /// The username of the user
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// The password of the user
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Password { get; set; }
    }


    // Request body for ..api/Users/register
    /// <summary>
    /// A user to be registered or updated with Username, Email, Password and CPassword (confirm password) fields 
    /// </summary>
    public class UserReqBody
    {
        /// <summary>
        /// The username of the user
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// The email address of the user
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// The password of the user
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Password { get; set; }

        /// <summary>
        /// A re-type of the password for confirmation
        /// </summary>
        [Required]
        [MaxLength(50)]
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
    /// <summary>
    /// A group to be created or updated with Title, Purpose and IsActive (optional) fields 
    /// </summary>
    public class GroupReqBody
    {
        /// <summary>
        /// The title/name of the group
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        /// <summary>
        /// The purpose or description of the group
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Purpose { get; set; }

        /// <summary>
        /// Whether or not the group is active
        /// </summary>
        [DefaultValue(true)]
        public bool IsActive { get; set; }

        public void Deconstruct(out string title, out string purpose, out bool isActive)
        {
            title = Title;
            purpose = Purpose;
            isActive = IsActive;
        }
    }


    // Request body for ..api/Memberships
    /// <summary>
    /// A membership to be created with MemberId (user id) and MemberRole fields 
    /// </summary>
    public class MbrshpReqBody
    {
        /// <summary>
        /// The id of the user to be added to the group
        /// </summary>
        [Required]
        public string MemberId { get; set; }

        /// <summary>
        /// The role of the new member within the group. The default is 'Member'
        /// </summary>
        [Required]
        public string MemberRole { get; set; }
    }


    // Request body for ..api/Messages
    /// <summary>
    /// A message to be posted or updated with Content and Priority fields
    /// </summary>
    public class MsgRequestBody
    {
        /// <summary>
        /// The content/text of the message
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        /// <summary>
        /// The priority associated with the message. The default is 'normal'
        /// </summary>
        [Required]
        public string Priority { get; set; }

        public void Deconstruct(out string content, out string priority)
        {
            content = Content;
            priority = Priority;
        }
    }


    // Request body for ..api/Notifications
    /// <summary>
    /// A notification with Status field
    /// </summary>
    public class NotftnReqBody
    {
        /// <summary>
        /// The status of the notification
        /// </summary>
        [Required]
        public string NotificationStatus { get; set; }
    }
}