using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.Repositories;
using InstaminiWebService.ResponseModels;
using InstaminiWebService.ResponseModels.Factory;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InstaminiWebService.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepository Repository;
        private readonly IResponseModelFactory ResponseModelFactory;
        private readonly ILogger Logger;

        public UserController(InstaminiContext context, 
                              IResponseModelFactory responseModelFactory,
                              ILogger<UserController> logger,
                              RepositoryFactory repositoryProvider)
        {
            Repository = (UserRepository)repositoryProvider.GetRepository<User>();
            ResponseModelFactory = responseModelFactory;
            Logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> FindUsersByQueryOrId([FromQuery(Name = "q")] string query,
                                                        [FromQuery] int? id)
        {
            if ((query is null && id is null) || (!(query is null) && !(id is null)))
            {
                return BadRequest(new { Err = "You must only supply id or query!" });
            }

            if (query != null)
            {
                var result = (await Repository.FindByQueryAsync(query))
                                        .Select(u => (UserResponse)ResponseModelFactory.Create(u));
                return Ok(result);
            }
            else
            {
                var result = await Repository.FindByIdAsync(id.Value);
                if (result is null)
                {
                    return NotFound();
                }
                return Ok(ResponseModelFactory.Create(result));
            }
            
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            bool hasDuplicate = await Repository.Exists(user);
            if (hasDuplicate)
            {
                return BadRequest(new { Err =  "Username is duplicated!" });
            }

            var result = await Repository.InsertAsync(user);
            
            return Created(Url.Action("GetUserByUsername", new { username = user.Username }), ResponseModelFactory.Create(user));
        }

        [HttpGet("{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserByUsername([FromRoute] string username)
        {
            var retrievedUser = await Repository.FindByUsernameAsync(username);
            if (retrievedUser is null)
            {
                return NotFound();
            }

            // format output
            return Ok(ResponseModelFactory.Create(retrievedUser));
        }

        [HttpPatch("{username}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] User user, 
                                                    [FromRoute] string username)
        {
            var retrievedUser = await Repository.FindByUsernameAsync(username);
            if (retrievedUser is null)
            {
                return NotFound();
            }

            if (retrievedUser.Username != username)
            {
                return BadRequest(new { Err =  "You are trying to update an account of another!" });
            }
            string jwtUsername = User.FindFirstValue(ClaimTypes.Name);

            if (jwtUsername != username)
            {
                return BadRequest(new { Err = "You cannot delete others' accounts!" });
            }

            // check username
            if (!string.IsNullOrEmpty(user.Username)
                && user.Username != username
                && await Repository.Exists(user))
            {
                return BadRequest(new { Err = "Username is duplicated!" });
            }

            // Perform password update right here
            // ----------------------------------
            var result = await Repository.UpdateAsync(retrievedUser, user);
            
            return Ok(ResponseModelFactory.Create(retrievedUser));
        }

        [HttpDelete("{username}")]
        [Authorize]
        public async Task<IActionResult> RemoveUser([FromRoute] string username)
        {
            // TODO: 
            // CAN ONLY REMOVE THE LOGGED IN USER WITH THE SAME ID, ELSE MUST FORBID
            // ---------------------------------------------
            string jwtUsername = User.FindFirstValue(ClaimTypes.Name);
            if (jwtUsername != username)
            {
                return BadRequest(new { Err =  "You cannot delete others' accounts!" });
            }
            // ---------------------------------------------
            var retrievedUser = await Repository.FindByUsernameAsync(username);
            if (retrievedUser is null)
            {
                return NotFound();
            }

            await Repository.DeleteAsync(retrievedUser);

            // remove token in response
            Response.Cookies.Delete("Token");
            return Ok();
        }
    }
}
