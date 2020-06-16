using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers;
using InstaminiWebService.ModelWrappers.Factory;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InstaminiWebService.Controllers
{
    [Route("posts/{id}/comments")]
    [ApiController]
    public class PostCommentController : ControllerBase
    {
        private readonly InstaminiContext DbContext;
        private readonly IModelWrapperFactory ModelWrapperFactory;

        public PostCommentController(InstaminiContext context, IModelWrapperFactory modelWrapperFactory)
        {
            DbContext = context;
            ModelWrapperFactory = modelWrapperFactory;
        }

        [HttpGet] [AllowAnonymous]
        public async Task<IEnumerable<CommentWrapper>> GetCommentsByPostId([FromRoute(Name = "id")] int postId)
        {
            return await DbContext.Comments
                            .Include(c => c.User).ThenInclude(u => u.AvatarPhoto)
                            .Where(c => c.PostId == postId)
                            .Select(c => (CommentWrapper)ModelWrapperFactory.Create(c))
                            .ToListAsync();
        }

        [HttpPost] [Authorize]
        public async Task<IActionResult> CreateCommentForPost([FromRoute(Name = "id")] int postId, [FromBody] Comment comment)
        {
            if (postId != comment.PostId)
            {
                return BadRequest(new { Err = "You are creating a comment for the wrong post!" });
            }

            // verify the current user
            string jwt = Request.Cookies["Token"];
            if (string.IsNullOrEmpty(jwt))
            {
                return BadRequest(new { Err = "Unauthorized user!" });
            }
            int _userId = int.Parse(JwtUtils.ValidateJWT(jwt)?.Claims
                                .Where(claim => claim.Type == ClaimTypes.NameIdentifier)
                                .FirstOrDefault().Value);
            if (comment.UserId != _userId)
            {
                return BadRequest(new { Err = "Cannot create a comment on behalf of another!" });
            }

            comment.Timestamp = DateTimeOffset.UtcNow;
            DbContext.Add(comment);
            await DbContext.SaveChangesAsync();
            await DbContext.Entry(comment)
                            .Reference(c => c.User)
                            .Query()
                            .Include(u => u.AvatarPhoto)
                            .LoadAsync();
            var url = Url.Action("GetCommentById", "Comment", new { id = comment.Id });
            return Created(url, ModelWrapperFactory.Create(comment));
        }
    }
}