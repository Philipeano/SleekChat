using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Api.Controllers
{
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ResponseBody))]
    [Authorize]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ICurrentUser currentUser;
        private readonly IMessageData messageData;
        private readonly IMembershipData membershipData;
        private readonly IGroupData groupData;
        private readonly IUserData userData;
        private readonly ValidationHelper validator;
        private readonly FormatHelper formatter;
        private readonly HttpHelper httpHelper;
        private KeyValuePair<bool, string> validationResult;

        public MessagesController(IGroupData groupData, IUserData userData, IMembershipData membershipData, IMessageData messageData, ICurrentUser currentUser)
        {
            this.currentUser = currentUser;
            this.messageData = messageData;
            this.membershipData = membershipData;
            this.groupData = groupData;
            this.userData = userData;
            validator = new ValidationHelper();
            formatter = new FormatHelper();
            httpHelper = new HttpHelper();
        }


        // GET: api/messages?senderId
        /// <summary>
        /// Fetch all existing messages, or messages sent by a specific user if 'senderId' is provided
        /// </summary>
        /// <param name="senderId">The 'id' of the user whose sent messages are to be fetched (Optional)</param>
        /// <returns>A list of messages, each with 'id', 'content', 'status', 'priority', 'group', 'sender' and 'dateSent' fields</returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="401">Unauthorised! You are not signed in.</response>
        /// <response code="404">Not found! The specified resource does not exist.</response>
        /// <response code="200">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBody))]
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
        /// <summary>
        /// Fetch a message with the specified 'msgId'
        /// </summary>
        /// <param name="msgId">The 'id' of the message to be fetched</param>
        /// <returns>A message with 'id', 'content', 'status', 'priority', 'group', 'sender' and 'sent' (date) fields</returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="401">Unauthorised! You are not signed in.</response>
        /// <response code="404">Not found! The specified resource does not exist.</response>
        /// <response code="200">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBody))]
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
        /// <summary>
        /// Fetch all messages for the group, or messages sent to the group by a specific user if 'senderId' is provided
        /// </summary>
        /// <param name="grpId">The 'id' of the group for which messages are to be fetched</param>
        /// <param name="senderId">The 'id' of the user whose sent messages are to be fetched (Optional)</param>
        /// <returns>A list of messages, each with 'id', 'content', 'status', 'priority', 'group', 'sender' and 'sent' (date) fields</returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="401">Unauthorised! You are not signed in.</response>
        /// <response code="404">Not found! The specified resource does not exist.</response>
        /// <response code="200">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBody))]
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
        /// <summary>
        /// Send a new message to the specified group using the fields supplied in 'reqBody'  
        /// </summary>
        /// <param name="grpId">The 'id' of the group to which the message will be posted</param>
        /// <param name="reqBody">A JSON object containing 'content' and 'priority' fields</param>
        /// <returns>The newly posted message with 'id', 'content', 'status', 'priority', 'group', 'sender' and 'sent' (date) fields</returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="401">Unauthorised! You are not signed in.</response>
        /// <response code="404">Not found! The specified resource does not exist.</response>
        /// <response code="403">Forbidden! You are not allowed to perform this operation.</response>
        /// <response code="201">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseBody))]
        [HttpPost("api/groups/{grpId}/messages")]
        public ActionResult Post([FromRoute] string grpId, [FromBody] MsgRequestBody reqBody)
        {
            (string content, string priority) = reqBody;

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

            Guid userId = currentUser.GetUserId();

            // Check if current user is a member of the group
            if (!membershipData.IsGroupMember(reqGroupId, userId))
            {
                formatter.RenderJson(validator.Result("You are not a member of this group."), out string responseTxt);
                httpHelper.Forbid(Response, responseTxt);
                return null;
            }

            validationResult = validator.IsBlank("content", content);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("priority", priority);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Message newMessage = messageData.CreateNewMessage(content, userId, reqGroupId, DataHelper.GetPriority(priority), MessageStatus.Visible);
            return Created("", formatter.Render(newMessage, "Message", Operation.Posted));
        }


        // PUT: api/groups/id/messages/id
        /// <summary>
        /// Update a message with id 'msgId' in the group with id 'grpId', using the fields in 'reqBody'  
        /// </summary>
        /// <param name="grpId">The 'id' of the group to which the message was sent</param>
        /// <param name="msgId">The 'id' of the message to be updated</param>
        /// <param name="reqBody">A JSON object containing 'content' and 'priority' fields</param>
        /// <returns>The newly updated message with 'id', 'content', 'status', 'priority', 'group', 'sender' and 'sent' (date) fields</returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="401">Unauthorised! You are not signed in.</response>
        /// <response code="404">Not found! The specified resource does not exist.</response>
        /// <response code="403">Forbidden! You are not allowed to perform this operation.</response>
        /// <response code="201">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseBody))]
        [HttpPut("api/groups/{grpId}/messages/{msgId}")]
        public ActionResult Put([FromRoute] string grpId, [FromRoute] string msgId, [FromBody] MsgRequestBody reqBody)
        {
            (string content, string priority) = reqBody;

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

            Guid userId = currentUser.GetUserId();

            // Check if current user is a member of the group
            if (!membershipData.IsGroupMember(reqGroupId, userId))
            {
                formatter.RenderJson(validator.Result("You are not a member of this group."), out string responseTxt);
                httpHelper.Forbid(Response, responseTxt);
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
            if (!messageData.IsMessageSender(reqMessageId, userId))
            {
                formatter.RenderJson(validator.Result("You are not the original sender of this message."), out string responseTxt);
                httpHelper.Forbid(Response, responseTxt);
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
        /// <summary>
        /// Delete a message with id 'msgId', which was previously sent to the group with id 'grpId'  
        /// </summary>
        /// <param name="grpId">The 'id' of the group to which the message was sent</param>
        /// <param name="msgId">The 'id' of the message to be deleted</param>
        /// <returns></returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="401">Unauthorised! You are not signed in.</response>
        /// <response code="404">Not found! The specified resource does not exist.</response>
        /// <response code="403">Forbidden! You are not allowed to perform this operation.</response>
        /// <response code="200">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBody))]
        [HttpDelete("api/groups/{grpId}/messages/{msgId}")]
        public ActionResult Delete([FromRoute] string grpId, [FromRoute] string msgId)
        {
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

            Guid userId = currentUser.GetUserId();

            string responseTxt;

            // Check if current user is a member of the group
            if (!membershipData.IsGroupMember(reqGroupId, userId))
            {
                formatter.RenderJson(validator.Result("You are not a member of this group."), out responseTxt);
                httpHelper.Forbid(Response, responseTxt);
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
            if (messageData.IsMessageSender(reqMessageId, userId) || groupData.IsGroupCreator(reqGroupId, userId))
            {
                messageData.DeleteMessage(reqMessageId);
                return Ok(formatter.Render(null, "Message", Operation.Deleted));
            }

            formatter.RenderJson(validator.Result("Sorry, you must either be the creator of this group, or the sender of the message."), out responseTxt);
            httpHelper.Forbid(Response, responseTxt);
            return null;
        }
    }
}