using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers;
using InstaminiWebService.ModelWrappers.Factory;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InstaminiWebService.Controllers
{
    [Route("users/{userId}/posts")]
    [ApiController]
    public class UserPostController : ControllerBase
    {
        private readonly InstaminiContext DbContext;
        private readonly IModelWrapperFactory ModelWrapperFactory;
        private readonly string PhotoServingPath;

        public UserPostController(InstaminiContext dbContext,
                                    IModelWrapperFactory modelWrapperFactory,
                                    IConfiguration configuration)
        {
            DbContext = dbContext;
            ModelWrapperFactory = modelWrapperFactory;
            PhotoServingPath = configuration.GetValue<string>("PhotoServingAbsolutePath");
        }

        [HttpPost] [Authorize]
        public async Task<IActionResult> CreatePost([FromForm] string caption, 
                                                    [FromForm] IList<IFormFile> uploads, 
                                                    [FromRoute] int userId)
        {
            if (uploads.Count <= 0)
            {
                return BadRequest(new { Err = "Cannot create a post without any photo!" });
            }

            // create post content
            var post = new Post()
            {
                Caption = caption,
                UserId = userId,
                Created = DateTimeOffset.UtcNow
            };
            DbContext.Add(post);
            await DbContext.SaveChangesAsync();
            post = await DbContext.Posts
                            .Include(p => p.User).ThenInclude(u => u.AvatarPhoto)
                            .Where(p => p.Id == post.Id)
                            .FirstOrDefaultAsync();

            // create file uploads
            var uploadedFiles = uploads.Select(async upload => await PhotoUtils.UploadPhotoAsync(upload, PhotoServingPath))
                                        .Select(task => task.Result)
                                        .Select(fileName => new Photo()
                                        {
                                            PostId = post.Id,
                                            FileName = fileName
                                        });
            DbContext.AddRange(uploadedFiles);
            await DbContext.SaveChangesAsync();

            return Created(Url.Action("GetPostById", "Post", new { id = post.Id }), ModelWrapperFactory.Create(post));
        }

        [HttpGet] [AllowAnonymous]
        public async Task<IEnumerable<PostWrapper>> GetPostsByUser([FromRoute] int userId)
        {
            return await DbContext.Posts
                                .Include(p => p.Photos)
                                .Include(p => p.Likes)
                                .Include(p => p.User).ThenInclude(u => u.AvatarPhoto)
                                .Where(p => p.UserId == userId)
                                .Select(p => (PostWrapper)ModelWrapperFactory.Create(p))
                                .AsNoTracking()
                                .ToListAsync();
        }
        
        [HttpGet("/users/{id}/feed")] [Authorize]
        public async Task<IActionResult> GetFeedByUser([FromRoute(Name = "id")] int userId)
        {
            // verify the current user
            string jwt = Request.Cookies["Token"];
            if (string.IsNullOrEmpty(jwt))
            {
                return BadRequest(new { Err = "Unauthorized user!" });
            }
            int _userId = int.Parse(JwtUtils.ValidateJWT(jwt)?.Claims
                                .Where(claim => claim.Type == ClaimTypes.NameIdentifier)
                                .FirstOrDefault().Value);
            if (userId != _userId)
            {
                return BadRequest(new { Err = "This is not your feed, get out!" });
            }

            var result =  await DbContext.Posts
                                .Include(p => p.Photos)
                                .Include(p => p.Likes)
                                .Include(p => p.User)
                                    .ThenInclude(u => u.Followings)
                                .Include(p => p.User)
                                    .ThenInclude(u => u.AvatarPhoto)
                                .Where(p => p.UserId == userId)
                                .Select(p => (PostWrapper)ModelWrapperFactory.Create(p))
                                .AsNoTracking()
                                .ToListAsync();
            return new JsonResult(result);
        }
    }
}
