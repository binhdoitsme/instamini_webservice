using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels.Factory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstaminiWebService.Controllers
{
    [Route("posts/{id}/likes")]
    [ApiController]
    public class PostReactionController : ControllerBase
    {
        private readonly InstaminiContext DbContext;
        private readonly IResponseModelFactory ResponseModelFactory;

        public PostReactionController(InstaminiContext context, IResponseModelFactory responseModelFactory)
        {
            DbContext = context;
            ResponseModelFactory = responseModelFactory;
        }

        [HttpPost] [Authorize]
        public async Task<IActionResult> CreateLikeReactionForPost([FromRoute(Name = "id")] int postId)
        {
            int userId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // check if any exists
            var retrievedLike = await DbContext.Likes.Include(l => l.User).ThenInclude(u => u.AvatarPhoto)
                                                    .Include(l => l.LikedPostNavigation)
                                                    .FirstOrDefaultAsync(l =>
                                                        l.UserId == userId && l.LikedPost == postId);
            if (retrievedLike is null)
            {
                retrievedLike = new Like()
                {
                    LikedPost = postId,
                    UserId = userId,
                    IsActive = true
                };
                DbContext.Add(retrievedLike);
            } else
            {
                if (retrievedLike.IsActive.Value)
                {
                    return Ok(ResponseModelFactory.Create(retrievedLike));
                }
            }
            DbContext.Entry(retrievedLike).CurrentValues.SetValues(new { IsActive = true });
            await DbContext.SaveChangesAsync();
            await DbContext.Entry(retrievedLike).GetDatabaseValuesAsync();
            await DbContext.Entry(retrievedLike)
                            .Reference(l => l.User)
                            .Query()
                            .Include(u => u.AvatarPhoto)
                            .LoadAsync();
            
            return Ok(ResponseModelFactory.Create(retrievedLike));
        }

        [HttpDelete] [Authorize]
        public async Task<IActionResult> DeleteLikeReactionForPost([FromRoute(Name = "id")] int postId)
        {
            int userId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // check if any exists
            var retrievedLike = await DbContext.Likes.SingleOrDefaultAsync(l =>
                                                        l.UserId == userId && l.LikedPost == postId);
            Console.WriteLine(retrievedLike.IsActive);
            if (retrievedLike is null || !retrievedLike.IsActive.Value)
            {
                return BadRequest(new { Err = "Has not liked yet!" });
            }

            if (retrievedLike.UserId != userId)
            {
                return BadRequest(new { Err = "You cannot delete others' reactions!" });
            }

            DbContext.Entry(retrievedLike).CurrentValues.SetValues(new { IsActive = false });
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
