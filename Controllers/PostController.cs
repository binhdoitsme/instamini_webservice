using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels;
using InstaminiWebService.ResponseModels.Factory;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InstaminiWebService.Controllers
{
    [Route("posts/{id}")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly InstaminiContext DbContext;
        private readonly IResponseModelFactory ResponseModelFactory;

        public PostController(InstaminiContext context, IResponseModelFactory responseModelFactory)
        {
            DbContext = context;
            ResponseModelFactory = responseModelFactory;
        }

        [HttpGet]
        public async Task<object> GetPostById([FromRoute] int id)
        {
            var dbResult = await DbContext.Posts
                        .Include(p => p.User).ThenInclude(u => u.AvatarPhoto)
                        .Include(p => p.Likes).ThenInclude(l => l.User).ThenInclude(u => u.AvatarPhoto)
                        .Include(p => p.Photos)
                        .Include(p => p.Comments)
                        .FirstOrDefaultAsync(p => p.Id == id);
            if (dbResult is null)
            {
                return NotFound();
            }
            return ResponseModelFactory.Create(dbResult);
        }

        [HttpPatch]
        public async Task<IActionResult> UpdatePost([FromRoute] int id, [FromBody] Post toBeUpdated)
        {
            if (id != toBeUpdated.Id)
            {
                return BadRequest(new { Err = "The post to be updated and the content do not come from the same post!" });
            }

            // verify the current user
            string jwt = Request.Cookies["Token"];
            if (string.IsNullOrEmpty(jwt))
            {
                return BadRequest(new { Err = "Unauthorized user!" });
            }
            int userId = int.Parse(JwtUtils.ValidateJWT(jwt)?.Claims
                                .Where(claim => claim.Type == ClaimTypes.NameIdentifier)
                                .FirstOrDefault().Value);
            var current = await DbContext.Posts.Include(p => p.User).Where(p => p.Id == id).FirstOrDefaultAsync();
            if (userId != current?.UserId)
            {
                return BadRequest(new { Err = "You do not have permission to change this post!" });
            }

            // perform update
            var originalPost = await DbContext.Posts
                                    .Include(p => p.User).ThenInclude(u => u.AvatarPhoto)
                                    .Include(p => p.Likes).ThenInclude(l => l.User).ThenInclude(u => u.AvatarPhoto)
                                    .Include(p => p.Photos)
                                    .Include(p => p.Comments)
                                    .SingleOrDefaultAsync(p => p.Id == id);
            if (originalPost is null)
            {
                return NotFound();
            }
            DbContext.Entry(originalPost).CurrentValues.SetValues(new { toBeUpdated.Caption });
            await DbContext.SaveChangesAsync();

            return Ok(ResponseModelFactory.Create(originalPost));
        }

        [HttpDelete] [Authorize]
        public async Task<IActionResult> DeletePost([FromRoute] int id)
        {
            // get the post to be deleted
            Post toBeDeleted = await DbContext.Posts.FindAsync(id);
            if (toBeDeleted is null)
            {
                return NotFound();
            }

            // verify the current user
            string jwt = Request.Cookies["Token"];
            if (string.IsNullOrEmpty(jwt))
            {
                return BadRequest(new { Err = "Unauthorized user!" });
            }
            int userId = int.Parse(JwtUtils.ValidateJWT(jwt)?.Claims
                                .Where(claim => claim.Type == ClaimTypes.NameIdentifier)
                                .FirstOrDefault().Value);
            if (userId != toBeDeleted.UserId)
            {
                return BadRequest(new { Err = "You do not have permission to delete this post!" });
            }

            // perform deletion
            DbContext.Remove(toBeDeleted);
            await DbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
