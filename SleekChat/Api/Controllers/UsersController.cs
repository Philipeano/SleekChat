using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Api.Controllers
{
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseBody))]
    [Produces("application/json")]
    ////[Consumes("application/json")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ICurrentUser currentUser;
        private readonly IUserData userData;
        private readonly ValidationHelper validator;
        private readonly FormatHelper formatter;
        private readonly HttpHelper httpHelper;
        private KeyValuePair<bool, string> validationResult;
        private readonly IOptions<AppSettings> config;

        public UsersController(IUserData userData, IOptions<AppSettings> config, ICurrentUser currentUser)
        {
            this.currentUser = currentUser;
            this.userData = userData;
            this.config = config;
            validator = new ValidationHelper();
            formatter = new FormatHelper();
            httpHelper = new HttpHelper();
        }


        // GET: api/users
        /// <summary>
        /// Fetch all registered users
        /// </summary>
        /// <returns>A list of users, each with 'id', 'username', 'email' and 'registered' (date) fields </returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="401">Unauthorised! You are not signed in.</response>
        /// <response code="200">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBody))]
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(formatter.Render(userData.GetAllUsers(), "Users", Operation.Retrieved));
        }


        // GET: api/users/id
        /// <summary>
        /// Fetch a user with the specified 'id'
        /// </summary>
        /// <param name="id">The 'id' of the user to be fetched</param>
        /// <returns>A user with 'id', 'username', 'email' and 'registered' (date) fields</returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="401">Unauthorised! You are not signed in.</response>
        /// <response code="404">Not found! The specified resource does not exist.</response>
        /// <response code="200">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBody))]
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
        /// <summary>
        /// Register a new user with the fields supplied in 'reqBody'  
        /// </summary>
        /// <param name="reqBody">A JSON object containing 'username', 'email', 'password' and 'confirmPassword' fields</param>
        /// <returns>The newly created user with 'username', 'email', 'password' and 'registered' (date) fields</returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="409">Conflict! A resource with the same identifier already exists.</response>
        /// <response code="201">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseBody))]
        [AllowAnonymous]
        [HttpPost("register")]
        public ActionResult<ResponseBody> Register([FromBody] UserReqBody reqBody)
        {
            (string username, string email, string password, string cPassword) = reqBody;

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
        /// <summary>
        /// Sign in a user with 'username' and 'password' supplied in 'reqBody'
        /// </summary>
        /// <param name="reqBody">A JSON object containing 'username' and 'password' fields</param>
        /// <returns>Authenticated user with 'id', 'username', 'email' and 'registered' (date) fields, along with a 'token' </returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="200">Success! Operation completed successfully</response> 
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBody))]
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public ActionResult Authenticate([FromBody] AuthReqBody reqBody)
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
        /// <summary>
        /// Update an existing user with the fields supplied in 'reqBody'  
        /// </summary>
        /// <param name="id">The 'id' of the user to be updated</param>
        /// <param name="reqBody">A JSON object containing 'username', 'email', 'password' and 'confirmPassword' fields</param>
        /// <returns>The updated user with 'username', 'email', 'password' and 'registered' (date) fields</returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="401">Unauthorised! You are not signed in.</response>
        /// <response code="404">Not found! The specified resource does not exist.</response>
        /// <response code="403">Forbidden! You are not allowed to perform this operation.</response>
        /// <response code="409">Conflict! A resource with the same identifier already exists.</response>
        /// <response code="200">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBody))]
        [HttpPut("{id}")]
        public ActionResult Put([FromRoute] string id, [FromBody] UserReqBody reqBody)
        {
            (string username, string email, string password, string cPassword) = reqBody;

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
        /// <summary>
        /// Delete a user with the specified 'id'   
        /// </summary>
        /// <param name="id">The 'id' of the user to be deleted</param>
        /// <returns></returns>
        /// <response code="400">Bad request! Check for any error, and try again.</response>
        /// <response code="401">Unauthorised! You are not signed in.</response>
        /// <response code="404">Not found! The specified resource does not exist.</response>
        /// <response code="403">Forbidden! You are not allowed to perform this operation.</response>
        /// <response code="200">Success! Operation completed successfully</response> 
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ResponseBody))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBody))]
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