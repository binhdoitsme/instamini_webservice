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
    [Route("/comments/{id}")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly CommentRepository Repository;
        private readonly IResponseModelFactory ResponseModelFactory;

        public CommentController(IResponseModelFactory responseModelFactory, RepositoryFactory repositoryProvider)
        {
            ResponseModelFactory = responseModelFactory;
            Repository = (CommentRepository)repositoryProvider.GetRepository<Comment>();
        }

        [HttpGet]
        public async Task<IActionResult> GetCommentById([FromRoute] int id)
        {
            var result = await Repository.FindByIdAsync(id);
            if (result is null)
            {
                return NotFound();
            }
            return Ok((CommentResponse)ResponseModelFactory.Create(result));
        }

        [HttpPatch] [Authorize]
        public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromBody] Comment toBeUpdated)
        {
            if (id != toBeUpdated.Id)
            {
                return BadRequest(new { Err = "The post to be updated and the content do not come from the same post!" });
            }

            // verify the current user
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var current = await Repository.FindByIdAsync(id);
            if (userId != current?.UserId)
            {
                return BadRequest(new { Err = "You do not have permission to change this comment!" });
            }

            // perform update
            if (current is null)
            {
                return NotFound();
            }
            
            var result = await Repository.UpdateAsync(current, toBeUpdated);

            return Ok(ResponseModelFactory.Create(result));
        }

        [HttpDelete] [Authorize]
        public async Task<IActionResult> RemoveCommentById([FromRoute] int id)
        {
            // get the comment to be deleted
            Comment toBeDeleted = await Repository.FindByIdAsync(id);
            if (toBeDeleted is null)
            {
                return NotFound();
            }

            // verify the current user
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (userId != toBeDeleted.UserId)
            {
                return BadRequest(new { Err = "You do not have permission to delete this comment!" });
            }

            // perform deletion
            await Repository.DeleteAsync(toBeDeleted);

            return Ok();
        }
    }
}