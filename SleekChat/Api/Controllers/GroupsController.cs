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
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly ICurrentUser currentUser;
        private readonly IGroupData groupData;
        private readonly IUserData userData;
        private readonly ValidationHelper validator;
        private readonly FormatHelper formatter;
        private readonly HttpHelper httpHelper;
        private KeyValuePair<bool, string> validationResult;

        public GroupsController(IGroupData groupData, IUserData userData, ICurrentUser currentUser)
        {
            this.currentUser = currentUser;
            this.groupData = groupData;
            this.userData = userData;
            validator = new ValidationHelper();
            formatter = new FormatHelper();
            httpHelper = new HttpHelper();
        }


        // GET: api/groups
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(formatter.Render(groupData.GetAllGroups(), "Groups", Operation.Retrieved));
        }


        // GET: api/groups/id
        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            validationResult = validator.IsBlank("group Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("group Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Group group = groupData.GetGroupById(Guid.Parse(id));
            if (group == null)
                return NotFound(formatter.Render(validator.Result("No such group exists.")));

            return Ok(formatter.Render(group, "Group", Operation.Retrieved));
        }


        // POST: api/groups
        [HttpPost]
        public ActionResult Post([FromBody] GroupReqBody reqBody)
        {
            (string title, string purpose, _) = reqBody;

            validationResult = validator.IsBlank("title", title);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("purpose", purpose);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid userId = currentUser.GetUserId();
            User creator = userData.GetUserById(userId);
            if (creator == null)
                return NotFound(formatter.Render(validator.Result("This user id does not match any existing user account.")));

            if (groupData.TitleAlreadyTaken(title, out _))
                return Conflict(formatter.Render(validator.Result($"The group title '{title}' is already taken.")));

            Group newGroup = groupData.CreateNewGroup(userId, title, purpose, true);
            return Created("", formatter.Render(newGroup, "Group", Operation.Created));
        }


        // PUT: api/groups/id
        [HttpPut("{id}")]
        public ActionResult Put([FromRoute] string id, [FromBody] GroupReqBody reqBody)
        {
            (string title, string purpose, bool isActive) = reqBody;

            validationResult = validator.IsBlank("group id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("group id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Group group = groupData.GetGroupById(Guid.Parse(id));
            if (group == null)
                return NotFound(formatter.Render(validator.Result("No such group exists.")));

            validationResult = validator.IsBlank("title", title);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("purpose", purpose);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidActiveStatus("is active", isActive.ToString());
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid userId = currentUser.GetUserId();
            if (!groupData.IsGroupCreator(Guid.Parse(id), userId))
            {
                formatter.RenderJson(validator.Result("You are not the creator of this group."), out string responseTxt);
                httpHelper.Forbid(Response, responseTxt);
                return null;
            }

            Guid reqGroupId = Guid.Parse(id);
            if (groupData.TitleAlreadyTaken(title, out Group existingGroup) && (reqGroupId != existingGroup.Id))
                return Conflict(formatter.Render(validator.Result($"The title '{title}' is already in use by another group.")));

            groupData.UpdateGroup(reqGroupId, title, purpose, out Group updatedGroup);
            return Ok(formatter.Render(updatedGroup, "Group", Operation.Updated));
        }


        // DELETE: api/groups/id
        [HttpDelete("{id}")]
        public ActionResult Delete([FromRoute] string id)
        {
            Guid userId = currentUser.GetUserId();
            validationResult = validator.IsBlank("group id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("group id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            if (groupData.GetGroupById(Guid.Parse(id)) == null)
                return NotFound(formatter.Render(validator.Result("No such group exists.")));

            if (!groupData.IsGroupCreator(Guid.Parse(id), userId))
            {
                formatter.RenderJson(validator.Result("You are not the creator of this group."), out string responseTxt);
                httpHelper.Forbid(Response, responseTxt);
                return null;
            }

            groupData.DeleteGroup(Guid.Parse(id));
            return Ok(formatter.Render(null, "Group", Operation.Deleted));
        }
    }
}