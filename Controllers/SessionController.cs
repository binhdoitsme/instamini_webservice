using System;
using System.Collections.Generic;
using System.Linq;
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
            User user = DbContext.Users
                            .Include(u => u.AvatarPhoto)
                            .Include(u => u.Followers)
                            .Include(u => u.Followings)
                            .Where(u => u.Username == _user.Username).FirstOrDefault();
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

            HttpContext.Response.Cookies.Append("Token", jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            });
            return Ok(new { userResponse.Id, userResponse.Username, Token = jwt, userResponse.Link });
        }

        [HttpGet] [AllowAnonymous]
        public IActionResult ValidateSession([FromQuery] string token)
        {
            return new JsonResult(new { valid = JwtUtils.ValidateJWT(token) != null });
        }

        [HttpDelete] [Authorize]
        public IActionResult InvalidateSession([FromQuery(Name = "key")] string jwt)
        {
            JwtUtils.InvalidateJWT(jwt);
            return Ok();
        }
    }
}
