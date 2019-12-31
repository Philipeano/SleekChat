using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserData userData;
        private readonly ValidationHelper validator;
        private readonly FormatHelper formatter;
        private KeyValuePair<bool, string> validationResult;

        public UsersController(IUserData userData)
        {
            this.userData = userData;
            validator = new ValidationHelper();
            formatter = new FormatHelper();
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

        // POST api/users
        [HttpPost]
        public ActionResult Post([FromBody]Request reqBody)
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

            User newUser = userData.CreateNewUser(username, email, cPassword, true);
            return Created("", formatter.Render(newUser, "User", Operation.Created));
        }

        // PUT api/users/id
        [HttpPut("{id}")]
        public ActionResult Put([FromRoute] string id, [FromBody]Request reqBody)
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

        // DELETE api/users/id
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

            userData.DeleteUser(Guid.Parse(id));
            return Ok(formatter.Render(null, "User", Operation.Deleted));
        }
    }
}
