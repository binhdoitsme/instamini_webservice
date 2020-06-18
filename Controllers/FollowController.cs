using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstaminiWebService.Controllers
{
    [Route("users/{username}/follows")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly InstaminiContext DbContext;

        public FollowController(InstaminiContext context)
        {
            DbContext = context;
        }

        [HttpGet]
        public IEnumerable<object> GetFollowsByUsername([FromRoute] string username)
        {
            return DbContext.Follows
                    .Include(f => f.User)
                    .Include(f => f.Follower)
                    .Where(f => f.User.Username == username)
                    .Select(f => CreateFollowResponse(username, f));
        }

        private static dynamic CreateFollowResponse(string username, Follow f)
        {
            if (f.User.Username == username)
            {
                return new
                {
                    f.Follower.Id,
                    f.Follower.Username,
                    Type = "following",
                    Link = $"/users/{f.Follower.Username}"
                };
            } else if (f.Follower.Username == username)
            {
                return new
                {
                    f.User.Id,
                    f.User.Username,
                    Type = "follower",
                    Link = $"/users/{f.User.Username}"
                };
            } else
            {
                throw new InvalidOperationException();
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MakeFollowRelationship([FromRoute] string username, 
                                                                [FromQuery(Name = "f")] string followedUser)
        {
            if (username == followedUser)
            {
                return BadRequest(new { Err = "Cannot follow yourself!" });
            }

            string jwt = Request.Cookies["Token"];
            string jwtUsername = JwtUtils.ValidateJWT(jwt)?.Claims
                                    .Where(claim => claim.Type == ClaimTypes.Name)
                                    .FirstOrDefault().Value;
            if (jwtUsername != username)
            {
                return BadRequest(new { Err = "You cannot make follows on others' accounts!" });
            }
            var user = await DbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
            var userToBeFollowed = await DbContext.Users.SingleOrDefaultAsync(u => u.Username == followedUser);
            if (userToBeFollowed is null)
            {
                return BadRequest(new { Err = "Followed user does not exists!" });
            }

            // make follows
            Follow retrievedFollow = await DbContext.Follows
                                            .Where(f => f.UserId == user.Id
                                                        && f.FollowerId == userToBeFollowed.Id)
                                            .FirstOrDefaultAsync();
            if (retrievedFollow is null)
            {
                retrievedFollow = new Follow()
                {
                    UserId = user.Id,
                    FollowerId = userToBeFollowed.Id,
                    IsActive = true
                };
                DbContext.Add(retrievedFollow);
            }
            else
            {
                if (retrievedFollow.IsActive.Value)
                {
                    return BadRequest(new { Err = "Already followed!" });
                }
                DbContext.Entry(retrievedFollow).CurrentValues.SetValues(new { IsActive = true });
            }

            await DbContext.SaveChangesAsync();
            return Ok(new
            {
                retrievedFollow.Follower.Id,
                retrievedFollow.Follower.Username,
                Type = "following",
                Link = $"/users/{retrievedFollow.Follower.Username}"
            });
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> StopFollowing([FromRoute] string username,
                                                        [FromQuery(Name = "f")] string followedUser)
        {
            if (username == followedUser)
            {
                return BadRequest(new { Err = "Cannot follow yourself!" });
            }

            string jwt = Request.Cookies["Token"];
            string jwtUsername = JwtUtils.ValidateJWT(jwt)?.Claims
                                    .Where(claim => claim.Type == ClaimTypes.Name)
                                    .FirstOrDefault().Value;
            if (jwtUsername != username)
            {
                return BadRequest(new { Err = "You cannot make follows on others' accounts!" });
            }
            var user = await DbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
            var userToBeFollowed = await DbContext.Users.SingleOrDefaultAsync(u => u.Username == followedUser);
            if (userToBeFollowed is null)
            {
                return BadRequest(new { Err = "Followed user does not exists!" });
            }

            // deactivate follow
            Follow retrievedFollow = await DbContext.Follows
                                            .Where(f => f.UserId == user.Id
                                                        && f.FollowerId == userToBeFollowed.Id)
                                            .FirstOrDefaultAsync();
            if (retrievedFollow is null)
            {
                return BadRequest(new { Err = "Not yet followed!" });
            }
            if (!retrievedFollow.IsActive.Value)
            {
                return BadRequest(new { Err = "Already unfollowed!" });
            }

            DbContext.Entry(retrievedFollow).CurrentValues.SetValues(new { IsActive = false });
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
