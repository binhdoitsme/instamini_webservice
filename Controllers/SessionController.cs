using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.Repositories;
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
        private readonly UserRepository Repository;
        private readonly IResponseModelFactory ResponseModelFactory;

        public SessionController(InstaminiContext context,
                                IResponseModelFactory responseModelFactory,
                                RepositoryFactory repositoryProvider)
        {
            DbContext = context;
            ResponseModelFactory = responseModelFactory;
            Repository = (UserRepository)repositoryProvider.GetRepository<User>();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> BeginSession([FromBody] User _user)
        {
            // validate username/password
            User user = await Repository.FindByUsernameAsync(_user.Username);
            bool isValidUser = false;
            if (user != null)
            {
                isValidUser = PasswordUtils.ValidatePasswordWithSalt(_user.Password, user.Salt, user.Password);
            }

            // if not valid username/password return BadRequest
            if (!isValidUser)
            {
                return BadRequest(new { Err = "Username or password is incorrect!" });
            }

            // else create and set-cookie JWT string
            await Repository.UpdateAsync(user, new User()
            {
                LastLogin = DateTimeOffset.UtcNow
            });

            string jwt = JwtUtils.CreateJwt(user.Username, user.Id);
            var principal = JwtUtils.ValidateJWT(jwt);
            var userResponse = (UserResponse)ResponseModelFactory.Create(user);

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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateSession([FromQuery(Name = "key")] string token)
        {
            var validatedPrincipal = JwtUtils.ValidateJWT(token);
            if (validatedPrincipal is null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(validatedPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var validatedUser = await Repository.FindByIdAsync(userId);
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

        [HttpDelete]
        [Authorize]
        public IActionResult InvalidateSession([FromQuery(Name = "key")] string jwt)
        {
            var principal = JwtUtils.ValidateJWT(jwt);
            if (principal is null)
            {
                return Unauthorized();
            }
            JwtUtils.InvalidateJWT(jwt);
            return Ok();
        }
    }
}
