using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels;
using InstaminiWebService.ResponseModels.Factory;
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
        private readonly IResponseModelFactory ResponseModelFactory;

        public PostCommentController(InstaminiContext context, IResponseModelFactory responseModelFactory)
        {
            DbContext = context;
            ResponseModelFactory = responseModelFactory;
        }

        [HttpGet] [AllowAnonymous]
        public async Task<IEnumerable<CommentResponse>> GetCommentsByPostId([FromRoute(Name = "id")] int postId)
        {
            return await DbContext.Comments
                            .Include(c => c.User).ThenInclude(u => u.AvatarPhoto)
                            .Where(c => c.PostId == postId)
                            .Select(c => (CommentResponse)ResponseModelFactory.Create(c))
                            .OrderByDescending(c => c.Timestamp)
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
            int _userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            comment.UserId = _userId;
            comment.Timestamp = DateTimeOffset.UtcNow;
            DbContext.Add(comment);
            await DbContext.SaveChangesAsync();
            await DbContext.Entry(comment)
                            .Reference(c => c.User)
                            .Query()
                            .Include(u => u.AvatarPhoto)
                            .LoadAsync();
            var url = Url.Action("GetCommentById", "Comment", new { id = comment.Id });
            return Created(url, ResponseModelFactory.Create(comment));
        }
    }
}