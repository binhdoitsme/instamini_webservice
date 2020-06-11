using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstaminiWebService.Controllers
{
    [Route("session")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly InstaminiContext DbContext;

        public SessionController(InstaminiContext context)
        {
            DbContext = context;
        }

        [HttpPost] [AllowAnonymous]
        public IActionResult BeginSession([FromBody] User _user)
        {
            // validate username/password
            User user = DbContext.Users.Where(u => u.Username == _user.Username).FirstOrDefault();
            bool isValidUser = false;
            if (user != null)
            {
                isValidUser = PasswordUtils.ValidatePasswordWithSalt(_user.Password, user.Salt, user.Password);
            }

            // if not valid username/password return BadRequest
            if (!isValidUser)
            {
                return BadRequest(new { err = "Username or password is incorrect!" });
            }

            // else create and set-cookie JWT string
            string jwt = JwtUtils.CreateJwt(user.Username, user.Id);
            var principal = JwtUtils.ValidateJWT(jwt);

            HttpContext.Response.Cookies.Append("Token", jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            });
            return Ok(new { token = jwt });
        }

        [HttpGet] [AllowAnonymous]
        public IActionResult ValidateSession([FromQuery] string token)
        {
            return new JsonResult(new { valid = JwtUtils.ValidateJWT(token) != null });
        }

        [HttpDelete] [Authorize]
        public IActionResult InvalidateSession([FromBody] string jwt)
        {
            JwtUtils.InvalidateJWT(jwt);
            return Ok();
        }
    }
}
