using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.Repositories;
using InstaminiWebService.ResponseModels;
using InstaminiWebService.ResponseModels.Factory;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InstaminiWebService.Controllers
{
    [Route("users/{username}/posts")]
    [ApiController]
    public class UserPostController : ControllerBase
    {
        private readonly InstaminiContext DbContext;
        private readonly PostRepository PostRepository;
        private readonly UserRepository UserRepository;
        private readonly IResponseModelFactory ResponseModelFactory;
        private readonly string PhotoServingPath;

        public UserPostController(InstaminiContext dbContext,
                                    IResponseModelFactory responseModelFactory,
                                    IConfiguration configuration,
                                    RepositoryFactory repositoryProvider)
        {
            DbContext = dbContext;
            ResponseModelFactory = responseModelFactory;
            PhotoServingPath = configuration.GetValue<string>("PhotoServingAbsolutePath");
            PostRepository = (PostRepository)repositoryProvider.GetRepository<Post>();
            UserRepository = (UserRepository)repositoryProvider.GetRepository<User>();
        }

        [HttpPost] [Authorize]
        public async Task<IActionResult> CreatePost([FromForm] string caption, 
                                                    [FromForm] IList<IFormFile> uploads, 
                                                    [FromRoute] string username)
        {
            if (uploads.Count <= 0)
            {
                return BadRequest(new { Err = "Cannot create a post without any photo!" });
            }

            // validate user
            var user = await DbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user is null)
            {
                return BadRequest(new { Err = "Unauthorized user!" });
            }

            var transaction = DbContext.Database.BeginTransaction();

            // create post content
            var post = new Post()
            {
                Caption = caption,
                UserId = user.Id,
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

            await transaction.CommitAsync();

            return Created(Url.Action("GetPostById", "Post", new { id = post.Id }), ResponseModelFactory.Create(post));
        }

        [HttpGet] [AllowAnonymous]
        public async Task<IEnumerable<PostResponse>> GetPostsByUser([FromRoute] string username)
        {
            return (await PostRepository.FindByUserAsync(username))
                        .Select(p => (PostResponse)ResponseModelFactory.Create(p));
        }
        
        [HttpGet("/users/{username}/feed")] [Authorize]
        public async Task<IActionResult> GetFeedByUser([FromRoute] string username)
        {
            // verify the current user
            string jwtUsername = User.FindFirst(ClaimTypes.Name).Value;
            if (username != jwtUsername)
            {
                return BadRequest(new { Err = "This is not your feed, get out!" });
            }

            // get related users
            var relatedUsers = await UserRepository.FindRelatedByUsernameAsync(username);

            var result = (await PostRepository.FindRelatedPostsAsync(username, relatedUsers))
                            .Select(p => (PostResponse)ResponseModelFactory.Create(p));
            return Ok(result);
        }
    }
}
