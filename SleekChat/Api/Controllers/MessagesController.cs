using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Api.Controllers
{
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IGroupData groupData;
        private readonly IUserData userData;
        private readonly IMembershipData membershipData;
        private readonly IMessageData messageData;
        private readonly ValidationHelper validator;
        private readonly FormatHelper formatter;
        private KeyValuePair<bool, string> validationResult;

        public MessagesController(IGroupData groupData, IUserData userData, IMembershipData membershipData, IMessageData messageData)
        {
            this.groupData = groupData;
            this.userData = userData;
            this.membershipData = membershipData;
            this.messageData = messageData;
            validator = new ValidationHelper();
            formatter = new FormatHelper();
        }

        // GET: api/messages?senderId
        [HttpGet("api/messages")]
        public ActionResult GetAll([FromQuery(Name = "senderId")] string senderId = "")
        {
            // If sender id was not specified, return ALL messages
            if (!Request.Query.ContainsKey("senderId"))
                return Ok(formatter.Render(messageData.GetAllMessages(), "Messages", Operation.Retrieved));

            // Validate specified sender id
            validationResult = validator.IsBlank("sender id", senderId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("sender id", senderId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqSenderId = Guid.Parse(senderId);

            User sender = userData.GetUserById(reqSenderId);
            return sender == null
                ? NotFound(formatter.Render(validator.Result("The specified sender id does not match any existing user.")))
                : (ActionResult)Ok(formatter.Render(messageData.GetAllMessagesFromAUser(reqSenderId), "Messages", Operation.Retrieved));
        }

        // GET: api/messages/id
        [HttpGet("api/messages/{msgId}")]
        public ActionResult GetById([FromRoute] string msgId)
        {
            // Validate specified message id
            validationResult = validator.IsBlank("message id", msgId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("message id", msgId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqMessageId = Guid.Parse(msgId);

            Message message = messageData.GetMessageById(reqMessageId);
            return message == null
                ? NotFound(formatter.Render(validator.Result("There is no message with the specified id.")))
                : (ActionResult)Ok(formatter.Render(message, "Message", Operation.Retrieved));
        }

        //GET: api/groups/id/messages?senderId
        [HttpGet("api/groups/{grpId}/messages")]
        public ActionResult GetByGroupId([FromRoute] string grpId, [FromQuery(Name = "senderId")] string senderId = "")
        {
            // Validate specified group id
            validationResult = validator.IsBlank("Group Id", grpId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("Group Id", grpId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Group group = groupData.GetGroupById(Guid.Parse(grpId));
            if (group == null)
                return NotFound(formatter.Render(validator.Result("No such group exists.")));

            Guid reqGroupId = Guid.Parse(grpId);

            // If sender id was not specified, return ALL messages for this group
            if (!Request.Query.ContainsKey("senderId"))
                return Ok(formatter.Render(messageData.GetGroupMessages(reqGroupId), "Messages", Operation.Retrieved));

            // Validate specified sender id
            validationResult = validator.IsBlank("sender id", senderId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("sender id", senderId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqSenderId = Guid.Parse(senderId);

            User sender = userData.GetUserById(reqSenderId);
            return sender == null
                ? NotFound(formatter.Render(validator.Result("The specified sender id does not match any existing user.")))
                : (ActionResult)Ok(formatter.Render(messageData.GetGroupMessagesFromAUser(reqGroupId, reqSenderId), "Messages", Operation.Retrieved));
        }

        // POST: api/groups/id/messages
        [HttpPost("api/groups/{grpId}/messages")]
        public ActionResult Post([FromRoute] string grpId, [FromHeader] string userId, [FromBody] RequestBody reqBody)
        {
            (string content, string priority) = reqBody;

            // Validate current user's id
            validationResult = validator.IsBlank("current user id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("current user id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqUserId = Guid.Parse(userId);

            User currentUser = userData.GetUserById(reqUserId);
            if (currentUser == null)
                return NotFound(formatter.Render(validator.Result("Your user id is invalid.")));

            // Validate specified group id
            validationResult = validator.IsBlank("group id", grpId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("group id", grpId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqGroupId = Guid.Parse(grpId);

            Group group = groupData.GetGroupById(reqGroupId);
            if (group == null)
                return NotFound(formatter.Render(validator.Result("No such group exists.")));

            // Check if current user is a member of the group
            if (!membershipData.IsGroupMember(reqGroupId, reqUserId))
            {
                formatter.RenderJson(validator.Result("You are not a member of this group."), out string responseTxt);
                Forbid(Response, responseTxt);
                return null;
            }

            validationResult = validator.IsBlank("content", content);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("priority", priority);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Message newMessage = messageData.CreateNewMessage(content, reqUserId, reqGroupId, DataHelper.GetPriority(priority), MessageStatus.Visible);
            return Created("", formatter.Render(newMessage, "Message", Operation.Posted));
        }

        // PUT: api/groups/id/messages/id
        [HttpPut("api/groups/{grpId}/messages/{msgId}")]
        public ActionResult Put([FromRoute] string grpId, [FromRoute] string msgId, [FromBody] RequestBody reqBody, [FromHeader] string userId)
        {
            (string content, string priority) = reqBody;

            // Validate current user's id
            validationResult = validator.IsBlank("current user id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("current user id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqUserId = Guid.Parse(userId);

            User currentUser = userData.GetUserById(reqUserId);
            if (currentUser == null)
                return NotFound(formatter.Render(validator.Result("Your user id is invalid.")));

            // Validate specified group id
            validationResult = validator.IsBlank("group id", grpId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("group id", grpId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqGroupId = Guid.Parse(grpId);

            Group group = groupData.GetGroupById(reqGroupId);
            if (group == null)
                return NotFound(formatter.Render(validator.Result("No such group exists.")));

            // Check if current user is a member of the group
            if (!membershipData.IsGroupMember(reqGroupId, reqUserId))
            {
                formatter.RenderJson(validator.Result("You are not a member of this group."), out string responseTxt);
                Forbid(Response, responseTxt);
                return null;
            }

            // Validate specified message id
            validationResult = validator.IsBlank("message id", msgId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("message id", msgId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqMessageId = Guid.Parse(msgId);
            Message message = messageData.GetGroupMessageById(reqGroupId, reqMessageId);
            if (message == null)
                return NotFound(formatter.Render(validator.Result("No message with the specified id was sent to this group.")));

            // Check if current user is the original sender of this message
            if (!messageData.IsMessageSender(reqMessageId, reqUserId))
            {
                formatter.RenderJson(validator.Result("You are not the original sender of this message."), out string responseTxt);
                Forbid(Response, responseTxt);
                return null;
            }

            validationResult = validator.IsBlank("content", content);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("priority", priority.ToString());
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            _ = messageData.UpdateMessage(reqMessageId, content, DataHelper.GetPriority(priority), out Message updatedMessage);
            return Ok(formatter.Render(updatedMessage, "Message", Operation.Updated));
        }

        // DELETE: api/groups/id/messages/id
        [HttpDelete("api/groups/{grpId}/messages/{msgId}")]
        public ActionResult Delete([FromRoute] string grpId, [FromRoute] string msgId, [FromHeader] string userId)
        {
            // Validate current user's id
            validationResult = validator.IsBlank("current user id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("current user id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqUserId = Guid.Parse(userId);

            User currentUser = userData.GetUserById(reqUserId);
            if (currentUser == null)
                return NotFound(formatter.Render(validator.Result("Your user id is invalid.")));

            // Validate specified group id
            validationResult = validator.IsBlank("group id", grpId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("group id", grpId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqGroupId = Guid.Parse(grpId);

            Group group = groupData.GetGroupById(reqGroupId);
            if (group == null)
                return NotFound(formatter.Render(validator.Result("No such group exists.")));

            string responseTxt;

            // Check if current user is a member of the group
            if (!membershipData.IsGroupMember(reqGroupId, reqUserId))
            {
                formatter.RenderJson(validator.Result("You are not a member of this group."), out responseTxt);
                Forbid(Response, responseTxt);
                return null;
            }

            // Validate specified message id
            validationResult = validator.IsBlank("message id", msgId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("message id", msgId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqMessageId = Guid.Parse(msgId);
            Message message = messageData.GetGroupMessageById(reqGroupId, reqMessageId);
            if (message == null)
                return NotFound(formatter.Render(validator.Result("No message with the specified id was sent to this group.")));

            // Allow deletion only if current user created the group or is the original sender of the message
            if (messageData.IsMessageSender(reqMessageId, reqUserId) || groupData.IsGroupCreator(reqGroupId, reqUserId))
            {
                messageData.DeleteMessage(reqMessageId);
                return Ok(formatter.Render(null, "Message", Operation.Deleted));
            }

            formatter.RenderJson(validator.Result("Sorry, you must either be the creator of this group, or the sender of the message."), out responseTxt);
            Forbid(Response, responseTxt);
            return null;
        }

        // Custom method for 403:Forbidden response
        private void Forbid(HttpResponse responseObj, string responseJson)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            responseObj.StatusCode = 403;
            responseObj.ContentType = "application/json";
            _ = responseObj.WriteAsync(responseJson, System.Text.Encoding.Default, token);
            source.Dispose();
        }
    }
}
