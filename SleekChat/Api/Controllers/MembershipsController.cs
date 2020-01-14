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
    public class MembershipsController : ControllerBase
    {
        private readonly IGroupData groupData;
        private readonly IUserData userData;
        private readonly IMembershipData membershipData;
        private readonly ValidationHelper validator;
        private readonly FormatHelper formatter;
        private KeyValuePair<bool, string> validationResult;

        public MembershipsController(IGroupData groupData, IUserData userData, IMembershipData membershipData)
        {
            this.groupData = groupData;
            this.userData = userData;
            this.membershipData = membershipData;
            validator = new ValidationHelper();
            formatter = new FormatHelper();
        }

        // GET: api/memberships?memberId
        [HttpGet("api/memberships")]
        public ActionResult Get([FromQuery(Name = "memberId")] string memberId = "")
        {
            // If member id was not specified, return ALL memberships
            if (!Request.Query.ContainsKey("memberId"))
                return Ok(formatter.Render(membershipData.GetAllMemberships(), "Memberships", Operation.Retrieved));

            // Validate specified member id
            validationResult = validator.IsBlank("member id", memberId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("member id", memberId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqMemberId = Guid.Parse(memberId);

            User member = userData.GetUserById(reqMemberId);
            return member == null
                ? NotFound(formatter.Render(validator.Result("The specified member id does not match any existing user.")))
                : (ActionResult)Ok(formatter.Render(membershipData.GetMembershipsForAUser(reqMemberId), "Memberships", Operation.Retrieved));
        }

        //GET: api/groups/id/memberships
        [HttpGet("api/groups/{grpId}/memberships")]
        public ActionResult GetByGroupId([FromRoute] string grpId)
        {
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
            return Ok(formatter.Render(membershipData.GetGroupMemberships(reqGroupId), "Memberships", Operation.Retrieved));
        }

        // POST: api/groups/id/memberships
        [HttpPost("api/groups/{grpId}/memberships")]
        public ActionResult Post([FromRoute] string grpId, [FromBody]RequestBody reqBody, [FromHeader] string userId)
        {
            (string memberId, _, _, _) = reqBody;

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

            // Validate specified member id
            validationResult = validator.IsBlank("new member id", memberId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("new member id", memberId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqMemberId = Guid.Parse(memberId);

            User newMember = userData.GetUserById(reqMemberId);
            if (newMember == null)
                return NotFound(formatter.Render(validator.Result("The specified new member id does not match any existing user.")));

            // Check if current user created the group
            if (!groupData.IsGroupCreator(reqGroupId, reqUserId))
            {
                formatter.RenderJson(validator.Result("You are not the creator of this group."), out string responseTxt);
                Forbid(Response, responseTxt);
                return null;
            }

            // Check if specified new member is already in the group
            if (membershipData.IsGroupMember(reqGroupId, reqMemberId))
                return Conflict(formatter.Render(validator.Result($"The user '{newMember.Username}' is already a member of this group.")));

            Membership newMembership = membershipData.AddGroupMember(reqGroupId, reqMemberId, "Member");
            return Created("", formatter.Render(newMembership, "Membership", Operation.Created));
        }

        // DELETE: api/groups/id/memberships?memberId
        [HttpDelete("api/groups/{grpId}/memberships")]
        public ActionResult Delete([FromRoute] string grpId, [FromQuery(Name = "memberId")] string memberId, [FromHeader] string userId)
        {
            // If member id is not supplied, abort the operation
            if (!Request.Query.ContainsKey("memberId"))
                return BadRequest(formatter.Render(validator.Result("Required parameter 'member id' was not provided.")));

            // Validate specified member id
            validationResult = validator.IsBlank("member id", memberId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("member id", memberId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqMemberId = Guid.Parse(memberId);

            // Validate current user's id
            validationResult = validator.IsBlank("user id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("user id", userId);
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

            User member = userData.GetUserById(reqMemberId);
            if (member == null)
                return NotFound(formatter.Render(validator.Result("The specified member id does not match any existing user.")));

            // Check if specified user is currently a member the group
            if (!membershipData.IsGroupMember(reqGroupId, reqMemberId))
                return NotFound(formatter.Render(validator.Result($"The user '{member.Username}' is not a member of this group.")));

            // Check if current user created the group or is the member to be removed
            if (groupData.IsGroupCreator(reqGroupId, reqUserId) || (reqMemberId == reqUserId))
            {
                membershipData.RemoveGroupMember(reqGroupId, reqMemberId);
                return Ok(formatter.Render(null, "Membership", Operation.Deleted));
            }

            formatter.RenderJson(validator.Result("Sorry, you must either be the creator of this group, or the member to be removed."), out string responseTxt);
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
