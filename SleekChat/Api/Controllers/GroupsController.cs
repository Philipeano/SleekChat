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
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupData groupData;
        private readonly IUserData userData;
        private readonly ValidationHelper validator;
        private readonly FormatHelper formatter;
        private KeyValuePair<bool, string> validationResult;

        public GroupsController(IGroupData groupData, IUserData userData)
        {
            this.groupData = groupData;
            this.userData = userData;
            validator = new ValidationHelper();
            formatter = new FormatHelper();
        }

        // GET: api/groups
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(formatter.Render(groupData.GetAllGroups(), "Groups", Operation.Retrieved));
        }

        // GET api/groups/id
        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            validationResult = validator.IsBlank("Group Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("Group Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Group group = groupData.GetGroupById(Guid.Parse(id));
            if (group == null)
                return NotFound(formatter.Render(validator.Result("No such group exists.")));

            return Ok(formatter.Render(group, "Group", Operation.Retrieved));
        }

        // POST api/groups
        [HttpPost]
        public ActionResult Post([FromBody]Request reqBody, [FromHeader] string userId)
        {
            (string title, string purpose, _) = reqBody;

            validationResult = validator.IsBlank("creator id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("creator id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("title", title);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("purpose", purpose);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            User creator = userData.GetUserById(Guid.Parse(userId));
            if (creator == null)
                return NotFound(formatter.Render(validator.Result("This user id does not match any existing user account.")));

            if (groupData.TitleAlreadyTaken(title, out _))
                return Conflict(formatter.Render(validator.Result($"The group title '{title}' is already taken.")));

            Group newGroup = groupData.CreateNewGroup(Guid.Parse(userId), title, purpose, true);
            return Created("", formatter.Render(newGroup, "Group", Operation.Created));
        }


        // PUT api/groups/id
        [HttpPut("{id}")]
        public ActionResult Put([FromRoute] string id, [FromBody]Request reqBody, [FromHeader] string userId)
        {
            (string title, string purpose, bool isActive) = reqBody;

            validationResult = validator.IsBlank("Group Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("Group Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Group group = groupData.GetGroupById(Guid.Parse(id));
            if (group == null)
                return NotFound(formatter.Render(validator.Result("No such group exists.")));

            validationResult = validator.IsBlank("creator id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("creator id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("title", title);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("purpose", purpose);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidActiveStatus("is active", isActive.ToString());
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            if (!groupData.IsGroupCreator(Guid.Parse(id), Guid.Parse(userId)))
            {
                formatter.RenderJson(validator.Result("You are not the creator of this group."), out string responseTxt);
                Forbid(Response, responseTxt);
                return null;
            }

            Guid reqGroupId = Guid.Parse(id);
            if (groupData.TitleAlreadyTaken(title, out Group existingGroup) && (reqGroupId != existingGroup.Id))
                return Conflict(formatter.Render(validator.Result($"The title '{title}' is already in use by another group.")));

            groupData.UpdateGroup(reqGroupId, title, purpose, isActive, out Group updatedGroup);

            return Ok(formatter.Render(updatedGroup, "Group", Operation.Updated));
        }

        // DELETE api/groups/id
        [HttpDelete("{id}")]
        public ActionResult Delete([FromRoute] string id, [FromHeader] string userId)
        {
            validationResult = validator.IsBlank("User Id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("User Id", userId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("Group Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("Group Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            if (groupData.GetGroupById(Guid.Parse(id)) == null)
                return NotFound(formatter.Render(validator.Result("No such group exists.")));

            if (!groupData.IsGroupCreator(Guid.Parse(id), Guid.Parse(userId)))
            {
                formatter.RenderJson(validator.Result("You are not the creator of this group."), out string responseTxt);
                Forbid(Response, responseTxt);
                return null;
            }

            groupData.DeleteGroup(Guid.Parse(id));
            return Ok(formatter.Render(null, "Group", Operation.Deleted));
        }

        // Custom method for 403 response
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
}
