using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers;
using InstaminiWebService.ModelWrappers.Factory;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InstaminiWebService.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DbSet<User> UserContext;
        private readonly InstaminiContext DbContext;
        private readonly IModelWrapperFactory ModelWrapperFactory;

        public UserController(InstaminiContext context, IModelWrapperFactory modelWrapperFactory)
        {
            DbContext = context;
            UserContext = context.Users;
            ModelWrapperFactory = modelWrapperFactory;
        }

        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<UserWrapper> FindUsersByQuery([FromQuery(Name = "q")][Required] string query)
        {
            return UserContext.Where(u => u.Username.Contains(query))
                                .Select(u => (UserWrapper)ModelWrapperFactory.Create(u));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            bool hasDuplicate = UserContext.Any(u => u.Username == user.Username);
            if (hasDuplicate)
            {
                return BadRequest(new { err = "Username is duplicated!" });
            }

            string originalPass = user.Password;
            string salt = PasswordUtils.GenerateSalt();
            string hashedPass = PasswordUtils.HashPasswordWithSalt(originalPass, salt);
            var now = DateTimeOffset.UtcNow;

            user.Password = hashedPass;
            user.Salt = salt;
            user.Created = now;
            user.LastUpdate = now;

            await UserContext.AddAsync(user);
            await DbContext.SaveChangesAsync();
            return Created(Url.Action("GetUserById", new { id = user.Id }), ModelWrapperFactory.Create(user));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            var retrievedUser = await UserContext.Where(u => u.Id == id).FirstOrDefaultAsync();
            if (retrievedUser == null)
            {
                return NotFound();
            }
            return new JsonResult(retrievedUser);
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] User user, [FromRoute] int id)
        {
            if (user.Id != id)
            {
                return BadRequest(new { err = "You are trying to update an account of another!" });
            }
            // Perform password update right here
            // ----------------------------------
            User retrievedUser = UserContext.Find(id);
            if (user.Password != retrievedUser.Password)
            {
                string salt = PasswordUtils.GenerateSalt();
                string hashedPassword = PasswordUtils.HashPasswordWithSalt(user.Password, salt);
                user.Salt = salt;
                user.Password = hashedPassword;
            }

            // After password update
            UserContext.Update(user);
            await DbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> RemoveUser([FromRoute] int id)
        {
            // TODO: 
            // CAN ONLY REMOVE THE LOGGED IN USER WITH THE SAME ID, ELSE MUST FORBID
            // ---------------------------------------------
            string jwt = HttpContext.Request.Cookies["Token"];
            int userId = int.Parse(JwtUtils.ValidateJWT(jwt)?.Claims
                                .Where(claim => claim.Type == ClaimTypes.NameIdentifier)
                                .FirstOrDefault().Value);
            if (userId != id)
            {
                return BadRequest(new { err = "You cannot delete others' accounts!" });
            }
            // ---------------------------------------------
            var retrievedUser = await UserContext.Where(u => u.Id == id).FirstOrDefaultAsync();
            if (retrievedUser == null)
            {
                return NotFound();
            }

            UserContext.Remove(retrievedUser);
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
