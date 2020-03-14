using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Helpers
{
    public class FormatHelper
    {

        public ResponseBody Render(dynamic output, string outputType, Operation operation)
        {
            ResponseBody response = new ResponseBody
            {
                Status = "normal",
                Message = $"{(outputType == "AuthenticatedUser" ? "User" : outputType)} {operation.ToString().ToLower()} successfully!",
                Data = (output == null) ? null : Simplify(outputType, output)
            };
            return response;
        }


        public ResponseBody Render(KeyValuePair<bool, string> validationResult)
        {
            ResponseBody response = new ResponseBody
            {
                Status = "error",
                Message = validationResult.Value,
                Data = null
            };
            return response;
        }


        public void RenderJson(KeyValuePair<bool, string> validationResult, out string responseJson)
        {
            responseJson = Render(validationResult).ToString();
        }


        private dynamic Simplify(string type, dynamic item)
        {
            if (item is null) return null;
            dynamic result; User objUser; Group objGroup;
            switch (type)
            {
                case "User":
                    result = new SimplifiedUser();
                    (result.Id, result.Username, result.Email, result.Registered) = (User)item;
                    break;
                case "AuthenticatedUser":
                    result = new AuthenticatedUser();
                    (result.Id, result.Username, result.Email, result.Registered, result.Token) = (AuthenticatedUser)item;
                    break;
                case "Group":
                    result = new SimplifiedGroup();
                    (result.Id, result.Title, result.Purpose, result.CreatorId, objUser, result.Created) = (Group)item;
                    result.Creator = Simplify("User", objUser);
                    break;
                case "Membership":
                    result = new SimplifiedMembership();
                    (result.Id, result.GroupId, objGroup, result.MemberId, objUser, result.MemberRole, result.Joined) = (Membership)item;
                    result.Group = Simplify("Group", objGroup);
                    result.Member = Simplify("User", objUser);
                    break;
                case "Message":
                    result = new SimplifiedMessage();
                    (result.Id, result.Content, result.Status, result.Priority, result.GroupId, objGroup, result.SenderId, objUser, result.Sent) = (Message)item;
                    result.Group = Simplify("Group", objGroup);
                    result.Sender = Simplify("User", objUser);
                    break;
                case "Notification":
                    Notification notification = (Notification)item;
                    result = new BasicNotification()
                    {
                        Id = notification.Id,
                        Group = notification.Message.Group.Title,
                        Sender = notification.Message.Sender.Username,
                        Recipient = notification.Recipient.Username,
                        Message = notification.Message.Content,
                        Status = notification.Status.ToString(),
                        Received = notification.DateCreated
                    };
                    break;
                default:
                    result = null;
                    break;
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
}