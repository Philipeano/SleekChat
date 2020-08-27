using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Api.Controllers
{
    [Authorize]
    [ApiController]
    public class MembershipsController : ControllerBase
    {
        private readonly ICurrentUser currentUser;
        private readonly IMembershipData membershipData;
        private readonly IGroupData groupData;
        private readonly IUserData userData;
        private readonly ValidationHelper validator;
        private readonly FormatHelper formatter;
        private readonly HttpHelper httpHelper;
        private KeyValuePair<bool, string> validationResult;

        public MembershipsController(IGroupData groupData, IUserData userData, IMembershipData membershipData, ICurrentUser currentUser)
        {
            this.currentUser = currentUser;
            this.membershipData = membershipData;
            this.groupData = groupData;
            this.userData = userData;
            validator = new ValidationHelper();
            formatter = new FormatHelper();
            httpHelper = new HttpHelper();
        }


        // GET: api/memberships?memberId
        /// <summary>
        /// Fetch all existing memberships, or a specific user's memberships if 'memberId' is provided
        /// </summary>
        /// <param name="memberId">The 'id' of the user whose memberships are to be fetched (Optional)</param>
        /// <returns>A list of memberships, each with 'id', 'group', 'member', 'role' and 'dateJoined' fields</returns>
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
        /// <summary>
        /// Fetch all memberships for the group with specified 'grpId'
        /// </summary>
        /// <param name="grpId">The 'id' of the group whose memberships are to be fetched</param>
        /// <returns>A list of memberships, each with 'id', 'group', 'member', 'role' and 'dateJoined' fields</returns>
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
        /// <summary>
        /// Add a user whose 'id' is supplied in 'reqBody' to the group with id 'grpId' 
        /// </summary>
        /// <param name="grpId">The 'id' of the group to which the new member will be added</param>
        /// <param name="reqBody">A JSON object containing the 'id' of the user to be added</param>
        /// <returns>The new membership with 'id', 'group', 'member', 'role' and 'dateJoined' fields</returns>
        [HttpPost("api/groups/{grpId}/memberships")]
        public ActionResult Post([FromRoute] string grpId, [FromBody] MbrshpReqBody reqBody)
        {
            string memberId = reqBody.MemberId;

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

            Guid userId = currentUser.GetUserId();

            // Check if current user created the group
            if (!groupData.IsGroupCreator(reqGroupId, userId))
            {
                formatter.RenderJson(validator.Result("You are not the creator of this group."), out string responseTxt);
                httpHelper.Forbid(Response, responseTxt);
                return null;
            }

            // Check if specified new member is already in the group
            if (membershipData.IsGroupMember(reqGroupId, reqMemberId))
                return Conflict(formatter.Render(validator.Result($"The user '{newMember.Username}' is already a member of this group.")));

            Membership newMembership = membershipData.AddGroupMember(reqGroupId, reqMemberId, "Member");
            return Created("", formatter.Render(newMembership, "Membership", Operation.Created));
        }


        // DELETE: api/groups/id/memberships?memberId
        /// <summary>
        /// Remove a user with id 'memberId' from the group with id 'grpId' 
        /// </summary>
        /// <param name="grpId">The 'id' of the group from which the user will be removed</param>
        /// <param name="memberId">The 'id' of the user to be removed from the group</param>
        /// <returns></returns>
        [HttpDelete("api/groups/{grpId}/memberships")]
        public ActionResult Delete([FromRoute] string grpId, [FromQuery(Name = "memberId")] string memberId)
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

            Guid userId = currentUser.GetUserId();

            // Check if current user created the group or is the member to be removed
            if (groupData.IsGroupCreator(reqGroupId, userId) || (reqMemberId == userId))
            {
                membershipData.RemoveGroupMember(reqGroupId, reqMemberId);
                return Ok(formatter.Render(null, "Membership", Operation.Deleted));
            }

            formatter.RenderJson(validator.Result("Sorry, you must be either the creator of this group, or the member to be removed."), out string responseTxt);
            httpHelper.Forbid(Response, responseTxt);
            return null;
        }
    }
}