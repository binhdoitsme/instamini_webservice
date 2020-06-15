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
    [Route("users/{id}/follows")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly InstaminiContext DbContext;

        public FollowController(InstaminiContext context)
        {
            DbContext = context;
        }

        [HttpGet]
        public IEnumerable<object> GetFollowersByUser([FromRoute] int id)
        {
            return DbContext.Follows.Include(f => f.Follower)
                    .Where(f => f.UserId == id)
                    .Select(f => new
                    {
                        f.Follower.Id,
                        f.Follower.Username,
                        Link = $"/users/{f.Follower.Id}"
                    });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MakeFollowRelationship([FromRoute] int id, [FromQuery] int followedUser)
        {
            if (id == followedUser)
            {
                return BadRequest(new { Err = "Cannot follow yourself!" });
            }

            string jwt = Request.Cookies["Token"];
            int userId = int.Parse(JwtUtils.ValidateJWT(jwt)?.Claims
                                .Where(claim => claim.Type == ClaimTypes.NameIdentifier)
                                .FirstOrDefault().Value);
            if (userId != id)
            {
                return BadRequest(new { Err = "You cannot make follows on others' accounts!" });
            }
            if (DbContext.Users.Find(followedUser) == null)
            {
                return BadRequest(new { Err = "Followed user does not exists!" });
            }

            // make follows
            Follow retrievedFollow = await DbContext.Follows
                                            .Where(f => f.UserId == id && f.FollowerId == followedUser)
                                            .FirstOrDefaultAsync();
            if (retrievedFollow is null)
            {
                DbContext.Add(new Follow()
                {
                    UserId = id,
                    FollowerId = followedUser,
                    IsActive = true
                });
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
            return Ok();
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> StopFollowing([FromRoute] int id, [FromQuery] int followedUser)
        {
            if (id == followedUser)
            {
                return BadRequest(new { Err = "Cannot follow yourself!" });
            }

            string jwt = Request.Cookies["Token"];
            int userId = int.Parse(JwtUtils.ValidateJWT(jwt)?.Claims
                                .Where(claim => claim.Type == ClaimTypes.NameIdentifier)
                                .FirstOrDefault().Value);
            if (userId != id)
            {
                return BadRequest(new { Err = "You cannot make follows on others' accounts!" });
            }
            if (DbContext.Users.Find(followedUser) == null)
            {
                return BadRequest(new { Err = "Followed user does not exists!" });
            }

            // deactivate follow
            Follow retrievedFollow = await DbContext.Follows
                                            .Where(f => f.UserId == id && f.FollowerId == followedUser)
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
