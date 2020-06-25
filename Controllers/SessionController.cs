using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels;
using InstaminiWebService.ResponseModels.Factory;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstaminiWebService.Controllers
{
    [Route("session")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly InstaminiContext DbContext;
        private readonly IResponseModelFactory ResponseModelFactory;

        public SessionController(InstaminiContext context, IResponseModelFactory responseModelFactory)
        {
            DbContext = context;
            ResponseModelFactory = responseModelFactory;
        }

        [HttpPost] [AllowAnonymous]
        public async Task<IActionResult> BeginSession([FromBody] User _user)
        {
            // validate username/password
            User user = await DbContext.Users
                                .Include(u => u.AvatarPhoto)
                                .Include(u => u.Followers)
                                    .ThenInclude(f => f.Follower)
                                        .ThenInclude(f => f.AvatarPhoto)
                                .Include(u => u.Followings)
                                    .ThenInclude(f => f.User)
                                        .ThenInclude(f => f.AvatarPhoto)
                                .FirstOrDefaultAsync(u => u.Username == _user.Username);
            bool isValidUser = false;
            if (user != null)
            {
                isValidUser = PasswordUtils.ValidatePasswordWithSalt(_user.Password, user.Salt, user.Password);
            }

            // if not valid username/password return BadRequest
            if (!isValidUser)
            {
                return BadRequest(new { Err =  "Username or password is incorrect!" });
            }

            // else create and set-cookie JWT string
            DbContext.Entry(user).CurrentValues.SetValues(new { LastLogin = DateTimeOffset.UtcNow });
            await DbContext.SaveChangesAsync();
            
            string jwt = JwtUtils.CreateJwt(user.Username, user.Id);
            var principal = JwtUtils.ValidateJWT(jwt);
            var userResponse = (UserResponse)ResponseModelFactory.Create(user);

            HttpContext.Response.Cookies.Append("Token", jwt);
            return Ok(new
            {
                Token = jwt,
                userResponse.Id,
                userResponse.Username,
                userResponse.DisplayName,
                userResponse.AvatarLink,
                userResponse.Followings,
                userResponse.Link
            });
        }

        [HttpGet] [AllowAnonymous]
        public async Task<IActionResult> ValidateSession([FromQuery(Name = "key")] string token)
        {
            var validatedPrincipal = JwtUtils.ValidateJWT(token);
            if (validatedPrincipal is null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(validatedPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var validatedUser = await DbContext.Users.Include(u => u.AvatarPhoto)
                                                .Include(u => u.Followers)
                                                    .ThenInclude(f => f.Follower)
                                                        .ThenInclude(f => f.AvatarPhoto)
                                                .Include(u => u.Followings)
                                                    .ThenInclude(f => f.User)
                                                        .ThenInclude(f => f.AvatarPhoto)
                                                .FirstOrDefaultAsync(u => u.Id == userId);
            var userResponse = (UserResponse)ResponseModelFactory.Create(validatedUser);

            return Ok(new 
            {
                Token = token,
                userResponse.Id, 
                userResponse.Username, 
                userResponse.DisplayName, 
                userResponse.AvatarLink,
                userResponse.Followings,
                userResponse.Link 
            });
        }

        [HttpDelete] [Authorize]
        public IActionResult InvalidateSession([FromQuery(Name = "key")] string jwt)
        {
            var principal = JwtUtils.ValidateJWT(jwt);
            if (principal is null)
            {
                return Unauthorized();
            }
            Response.Cookies.Delete("Token"); 
            JwtUtils.InvalidateJWT(jwt);
            return Ok();
        }
    }
}
