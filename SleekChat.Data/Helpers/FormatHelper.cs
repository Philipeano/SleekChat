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

        public void RenderJson(KeyValuePair<bool, string> validationResult, out string responseJson)
        {
            responseJson = Render(validationResult).ToString();
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
}


