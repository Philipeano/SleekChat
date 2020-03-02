using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserData userData;
        private readonly ICurrentUser currentUser;
        private readonly ValidationHelper validator;
        private readonly FormatHelper formatter;
        public readonly HttpHelper httpHelper;
        private KeyValuePair<bool, string> validationResult;
        private readonly IOptions<AppSettings> config;

        public UsersController(IUserData userData, IOptions<AppSettings> config, ICurrentUser currentUser)
        {
            this.userData = userData;
            this.config = config;
            this.currentUser = currentUser;
            validator = new ValidationHelper();
            formatter = new FormatHelper();
            httpHelper = new HttpHelper();
        }


        // GET: api/users
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(formatter.Render(userData.GetAllUsers(), "Users", Operation.Retrieved));
        }


        // GET: api/users/id
        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            validationResult = validator.IsBlank("User Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("User Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            User user = userData.GetUserById(Guid.Parse(id));
            if (user == null)
                return NotFound(formatter.Render(validator.Result("No such user exists.")));

            return Ok(formatter.Render(user, "User", Operation.Retrieved));
        }


        // POST: api/users/register
        [AllowAnonymous]
        [HttpPost("register")]
        public ActionResult Register([FromBody] RequestBody reqBody)
        {
            (string username, string email, string password, string cPassword, _) = reqBody;

            validationResult = validator.IsBlank("username", username);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("email", email);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidEmail(email);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.PasswordsMatch(password, cPassword);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            if (userData.UsernameAlreadyTaken(username, out _))
                return Conflict(formatter.Render(validator.Result($"The username '{username}' is already taken.")));

            if (userData.EmailAlreadyTaken(email, out _))
                return Conflict(formatter.Render(validator.Result($"The email address '{email}' is already in use.")));

            User newUser = userData.CreateNewUser(username, email, password, true);
            return Created("", formatter.Render(newUser, "User", Operation.Registered));
        }


        // POST: api/users/authenticate
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public ActionResult Authenticate([FromBody] AuthRequestBody reqBody)
        {
            validationResult = validator.IsBlank("username", reqBody.Username);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("password", reqBody.Password);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            AuthenticatedUser user = userData.Authenticate(reqBody, config);
            if (user == null)
                return BadRequest(formatter.Render(validator.Result("Username or password is invalid.")));

            return Ok(formatter.Render(user, "AuthenticatedUser", Operation.Authenticated));
        }


        // PUT: api/users/id
        [HttpPut("{id}")]
        public ActionResult Put([FromRoute] string id, [FromBody] RequestBody reqBody)
        {
            (string username, string email, string password, string cPassword, _) = reqBody;           

            validationResult = validator.IsBlank("User Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("User Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            User user = userData.GetUserById(Guid.Parse(id));
            if (user == null)
                return NotFound(formatter.Render(validator.Result("No such user exists.")));

            if (currentUser.GetUserId() != Guid.Parse(id))
            {
                formatter.RenderJson(validator.Result("Sorry, you cannot edit another user's profile."), out string responseTxt);
                httpHelper.Forbid(Response, responseTxt);
                return null;
            }

            validationResult = validator.IsBlank("username", username);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsBlank("email", email);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidEmail(email);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.PasswordsMatch(password, cPassword);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqUserId = Guid.Parse(id);
            if (userData.UsernameAlreadyTaken(username, out User existingUser) && (reqUserId != existingUser.Id))
                return Conflict(formatter.Render(validator.Result($"The username '{username}' is already in use by another user.")));

            if (userData.EmailAlreadyTaken(email, out existingUser) && (reqUserId != existingUser.Id))
                return Conflict(formatter.Render(validator.Result($"The email address '{email}' is already in use by another user.")));

            userData.UpdateUser(reqUserId, username, email, password, out User updatedUser);
            return Ok(formatter.Render(updatedUser, "User", Operation.Updated));
        }


        // DELETE: api/users/id
        [HttpDelete("{id}")]
        public ActionResult Delete([FromRoute] string id)
        {
            validationResult = validator.IsBlank("User Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("User Id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            if (userData.GetUserById(Guid.Parse(id)) == null)
                return NotFound(formatter.Render(validator.Result("No such user exists.")));

            if (currentUser.GetUserId() != Guid.Parse(id))
            {
                formatter.RenderJson(validator.Result("Sorry, you cannot delete another user's profile."), out string responseTxt);
                httpHelper.Forbid(Response, responseTxt);
                return null;
            }

            userData.DeleteUser(Guid.Parse(id));
            return Ok(formatter.Render(null, "User", Operation.Deleted));
        }
    }
}
