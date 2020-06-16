using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers;
using InstaminiWebService.ModelWrappers.Factory;
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
    [Route("/comments/{id}")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly InstaminiContext DbContext;
        private readonly IModelWrapperFactory ModelWrapperFactory;

        public CommentController(InstaminiContext context, IModelWrapperFactory modelWrapperFactory)
        {
            DbContext = context;
            ModelWrapperFactory = modelWrapperFactory;
        }

        [HttpGet]
        public async Task<CommentWrapper> GetCommentById([FromRoute] int id)
        {
            var result = await DbContext.Comments
                                .Include(c => c.User).ThenInclude(u => u.AvatarPhoto)
                                .Where(c => c.Id == id).FirstOrDefaultAsync();
            return (CommentWrapper)ModelWrapperFactory.Create(result);
        }

        [HttpPatch] [Authorize]
        public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromBody] Comment toBeUpdated)
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
                return BadRequest(new { Err = "You do not have permission to change this comment!" });
            }

            // perform update
            var originalPost = await DbContext.Comments.SingleOrDefaultAsync(c => c.Id == id);
            DbContext.Entry(originalPost).CurrentValues.SetValues(new { toBeUpdated.Content });
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete] [Authorize]
        public async Task<IActionResult> RemoveCommentById([FromRoute] int id)
        {
            // get the comment to be deleted
            Comment toBeDeleted = await DbContext.Comments.Where(c => c.Id == id).FirstOrDefaultAsync();
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
                return BadRequest(new { Err = "You do not have permission to delete this comment!" });
            }

            // perform deletion
            DbContext.Remove(toBeDeleted);
            await DbContext.SaveChangesAsync();

            return Ok();
        }
    }
}