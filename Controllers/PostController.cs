using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.Repositories;
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
        private readonly PostRepository Repository;
        private readonly IResponseModelFactory ResponseModelFactory;

        public PostController(IResponseModelFactory responseModelFactory,
                                RepositoryFactory repositoryProvider)
        {
            ResponseModelFactory = responseModelFactory;
            Repository = (PostRepository)repositoryProvider.GetRepository<Post>();
        }

        [HttpGet]
        public async Task<object> GetPostById([FromRoute] int id)
        {
            var dbResult = await Repository.FindByIdAsync(id);
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
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var current = await Repository.FindByIdAsync(id);
            if (current is null)
            {
                return NotFound();
            }

            if (userId != current?.UserId)
            {
                return BadRequest(new { Err = "You do not have permission to change this post!" });
            }

            // perform update
            var result = await Repository.UpdateAsync(current, new Post() {
                Caption = toBeUpdated.Caption
            });

            return Ok(ResponseModelFactory.Create(result));
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePost([FromRoute] int id)
        {
            // get the post to be deleted
            Post toBeDeleted = await Repository.FindByIdAsync(id);
            if (toBeDeleted is null)
            {
                return NotFound();
            }

            // verify the current user
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (userId != toBeDeleted.UserId)
            {
                return BadRequest(new { Err = "You do not have permission to delete this post!" });
            }

            // perform deletion
            await Repository.DeleteAsync(toBeDeleted);

            return Ok();
        }
    }
}
